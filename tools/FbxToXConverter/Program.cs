using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// -----------------------------------------------------------------------------
// FbxToXConverter
//
// The MonoGame content pipeline imports FBX 2011+ and DirectX .x files, but
// the original Galactik Wars models were exported as ASCII FBX 6.0 from Maya
// in 2008, which MonoGame's importer cannot read. This tool converts those
// FBX 6 files into DirectX .x text meshes so they can be built with the
// MonoGame XImporter.
//
// The converter:
//   - parses Mesh Model nodes (vertices, polygon indices, normals, UVs),
//   - unrolls polygon-vertex attributes into a split vertex list,
//   - fan-triangulates polygons,
//   - bakes the node transform into a FrameTransformMatrix, so the bone
//     hierarchy matches what the original XNA pipeline produced,
//   - writes a DirectX .x text file next to the source, referencing the
//     model texture from the Textures folder.
// -----------------------------------------------------------------------------

namespace FbxToXConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: FbxToXConverter <input.fbx> [more.fbx ...]");
                Environment.Exit(1);
            }

            foreach (string path in args)
            {
                ConvertFile(path);
            }
        }

        static void ConvertFile(string path)
        {
            Console.WriteLine($"Converting {path} ...");

            string text = File.ReadAllText(path);

            // --- Parse Model nodes ---------------------------------------------
            List<FbxObject> meshes = new List<FbxObject>();

            Regex modelHeader = new Regex(
                @"\bModel:\s*""(?<name>[^""]*)"",\s*""(?<sub>[^""]*)""\s*(?=\{)",
                RegexOptions.Compiled);

            foreach (Match match in modelHeader.Matches(text))
            {
                string body = ExtractBlock(text, match.Index + match.Length);

                if (body != null &&
                    match.Groups[2].Value == "Mesh" &&
                    body.Contains("Vertices:"))
                {
                    meshes.Add(new FbxObject
                    {
                        Name = match.Groups[1].Value,
                        Body = body,
                    });
                }
            }

            if (meshes.Count == 0)
            {
                throw new InvalidDataException("No mesh found in " + path);
            }

            // --- Extract texture filename --------------------------------------
            string textureFile = null;

            Match textureMatch = Regex.Match(text, @"RelativeFilename:\s+""([^""]*)""");
            if (textureMatch.Success)
            {
                textureFile = textureMatch.Groups[1].Value;

                // Resolve against the Textures folder, matching on file name.
                string name = Path.GetFileName(textureFile);
                string modelsDir = Path.GetDirectoryName(Path.GetFullPath(path));
                string texturesDir = Path.Combine(modelsDir, "..", "Textures");

                foreach (string candidate in Directory.GetFiles(texturesDir))
                {
                    if (string.Equals(Path.GetFileName(candidate), name,
                                      StringComparison.OrdinalIgnoreCase))
                    {
                        textureFile = $@"..\Textures\{Path.GetFileName(candidate)}";
                        break;
                    }
                }
            }

            // --- Build combined mesh -------------------------------------------
            List<float> outPositions = new List<float>();
            List<float> outNormals = new List<float>();
            List<float> outUvs = new List<float>();
            List<int[]> outFaces = new List<int[]>();

            foreach (FbxObject mesh in meshes)
            {
                Vector3 translation = ReadLcl(mesh.Body, "Lcl Translation");
                Vector3 rotation = ReadLcl(mesh.Body, "Lcl Rotation");
                Vector3 scaling = ReadLcl(mesh.Body, "Lcl Scaling",
                                          new Vector3(1, 1, 1));

                AddGeometry(mesh.Body, translation, rotation, scaling,
                            outPositions, outNormals, outUvs, outFaces);
            }

            // --- Write .x -------------------------------------------------------
            string output = BuildXFile(outPositions, outNormals, outUvs,
                                       outFaces, textureFile);

            string outputPath = Path.ChangeExtension(path, ".x");
            File.WriteAllText(outputPath, output, Encoding.ASCII);

            Console.WriteLine($"  vertices: {outPositions.Count / 3}, " +
                              $"faces: {outFaces.Count}, " +
                              $"texture: {textureFile ?? "none"}");
            Console.WriteLine($"  wrote {outputPath}");
        }

        // -----------------------------------------------------------------------
        // Object model
        // -----------------------------------------------------------------------

        class FbxObject
        {
            public string Name;
            public string Body;
        }

        struct Vector3
        {
            public float X, Y, Z;

            public Vector3(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        /// <summary>
        /// Extracts the balanced { ... } block that starts after the given
        /// position.
        /// </summary>
        static string ExtractBlock(string text, int start)
        {
            int open = text.IndexOf('{', start);

            if (open < 0)
            {
                return null;
            }

            int depth = 0;

            for (int i = open; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '{')
                {
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return text.Substring(open + 1, i - open - 1);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Reads an "Lcl ..." property:
        /// Property: "Lcl Translation", "Lcl Translation", "A+",x,y,z
        /// </summary>
        static Vector3 ReadLcl(string body, string property, Vector3 fallback = default)
        {
            Match match = Regex.Match(body,
                $@"""{Regex.Escape(property)}""\s*,\s*(?:""[^""]*""\s*,\s*)*" +
                @"(?:""A\+"",|""A"",)?\s*" +
                @"(-?[\d.eE+-]+)\s*,\s*(-?[\d.eE+-]+)\s*,\s*(-?[\d.eE+-]+)");

            if (!match.Success)
            {
                return fallback;
            }

            return new Vector3
            {
                X = ParseFloat(match.Groups[1].Value),
                Y = ParseFloat(match.Groups[2].Value),
                Z = ParseFloat(match.Groups[3].Value),
            };
        }

        static float ParseFloat(string value)
        {
            return float.Parse(value,
                               System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture);
        }

        // -----------------------------------------------------------------------
        // Geometry conversion
        // -----------------------------------------------------------------------

        class MeshLayer
        {
            public string Mapping;     // ByPolygonVertex, ByVertice, ByPolygon
            public string Reference;   // Direct, IndexToDirect
            public float[] Data;
            public int[] Indices;
        }

        /// <summary>
        /// Appends one geometry to the combined output buffers.
        /// </summary>
        static void AddGeometry(string body, Vector3 translation, Vector3 rotation,
                                Vector3 scaling, List<float> outPositions,
                                List<float> outNormals, List<float> outUvs,
                                List<int[]> outFaces)
        {
            float[] vertices = ReadFloatArray(body, "Vertices");
            int[] polygonIndices = ReadIntArray(body, "PolygonVertexIndex");

            MeshLayer normalLayer = ReadLayer(body, "LayerElementNormal",
                                              "Normals", "NormalsIndex");
            MeshLayer uvLayer = ReadLayer(body, "LayerElementUV", "UV", "UVIndex");

            Matrix transform = Matrix.Create(translation, rotation, scaling);

            int globalCorner = 0;
            List<int> face = new List<int>();

            for (int i = 0; i <= polygonIndices.Length; i++)
            {
                bool endOfPolygon = i == polygonIndices.Length;

                if (!endOfPolygon)
                {
                    int index = polygonIndices[i];

                    // FBX marks the last vertex of each polygon as -(index + 1).
                    endOfPolygon = index < 0;

                    if (endOfPolygon)
                    {
                        index = -index - 1;
                    }

                    face.Add(index);
                }

                if (endOfPolygon && face.Count > 0)
                {
                    // Emit one split vertex per corner, then fan triangulate.
                    int[] cornerVertex = new int[face.Count];

                    for (int c = 0; c < face.Count; c++)
                    {
                        cornerVertex[c] = EmitCorner(face[c], globalCorner + c,
                                                     vertices, normalLayer,
                                                     uvLayer, transform,
                                                     outPositions, outNormals,
                                                     outUvs);
                    }

                    for (int c = 1; c < face.Count - 1; c++)
                    {
                        outFaces.Add(new[]
                        {
                            cornerVertex[0], cornerVertex[c], cornerVertex[c + 1],
                        });
                    }

                    globalCorner += face.Count;
                    face.Clear();
                }
            }
        }

        static int EmitCorner(int vertexIndex, int corner, float[] vertices,
                              MeshLayer normalLayer, MeshLayer uvLayer,
                              Matrix transform, List<float> outPositions,
                              List<float> outNormals, List<float> outUvs)
        {
            // Position.
            Vector3 position = new Vector3
            {
                X = vertices[vertexIndex * 3],
                Y = vertices[vertexIndex * 3 + 1],
                Z = vertices[vertexIndex * 3 + 2],
            };
            position = transform.Apply(position);

            outPositions.Add(position.X);
            outPositions.Add(position.Y);
            outPositions.Add(position.Z);

            // Normal.
            float nx = 0, ny = 0, nz = 1;

            if (normalLayer != null && normalLayer.Data != null &&
                normalLayer.Data.Length >= 3)
            {
                int normalIndex = ResolveIndex(normalLayer, corner, vertexIndex);

                nx = normalLayer.Data[normalIndex * 3];
                ny = normalLayer.Data[normalIndex * 3 + 1];
                nz = normalLayer.Data[normalIndex * 3 + 2];
            }

            outNormals.Add(nx);
            outNormals.Add(ny);
            outNormals.Add(nz);

            // Texture coordinates.
            float u = 0, v = 0;

            if (uvLayer != null && uvLayer.Data != null &&
                uvLayer.Data.Length >= 2)
            {
                int uvIndex = ResolveIndex(uvLayer, corner, vertexIndex);

                u = uvLayer.Data[uvIndex * 2];
                v = uvLayer.Data[uvIndex * 2 + 1];
            }

            outUvs.Add(u);
            outUvs.Add(v);

            return outPositions.Count / 3 - 1;
        }

        /// <summary>
        /// Resolves the data index for a corner according to the layer's
        /// mapping and reference modes.
        /// </summary>
        static int ResolveIndex(MeshLayer layer, int corner, int vertexIndex)
        {
            if (layer.Mapping == "ByVertice" || layer.Mapping == "ByVertex")
            {
                return vertexIndex;
            }

            // ByPolygonVertex (or ByPolygon as a best effort).
            if (layer.Reference == "IndexToDirect" && layer.Indices != null)
            {
                return layer.Indices[corner];
            }

            return corner;
        }

        // -----------------------------------------------------------------------
        // 3x3 + translation matrix (row vector convention: v' = v * M)
        // -----------------------------------------------------------------------

        class Matrix
        {
            public float M11, M12, M13, M21, M22, M23, M31, M32, M33;
            public float TX, TY, TZ;

            public static Matrix Create(Vector3 translation, Vector3 rotation,
                                        Vector3 scaling)
            {
                // XYZ rotation order: apply scale, then Rx, Ry, Rz, then
                // translation.
                Matrix scale = Diagonal(scaling.X, scaling.Y, scaling.Z);
                Matrix rx = AxisX(rotation.X * MathF.PI / 180f);
                Matrix ry = AxisY(rotation.Y * MathF.PI / 180f);
                Matrix rz = AxisZ(rotation.Z * MathF.PI / 180f);
                Matrix t = Translation(translation.X, translation.Y, translation.Z);

                return Multiply(Multiply(Multiply(Multiply(scale, rx), ry), rz), t);
            }

            static Matrix Diagonal(float x, float y, float z) =>
                new Matrix { M11 = x, M22 = y, M33 = z };

            static Matrix Translation(float x, float y, float z) =>
                new Matrix { M11 = 1, M22 = 1, M33 = 1, TX = x, TY = y, TZ = z };

            static Matrix AxisX(float a)
            {
                float c = MathF.Cos(a), s = MathF.Sin(a);
                return new Matrix { M11 = 1, M22 = c, M23 = s, M32 = -s, M33 = c };
            }

            static Matrix AxisY(float a)
            {
                float c = MathF.Cos(a), s = MathF.Sin(a);
                return new Matrix { M11 = c, M13 = -s, M22 = 1, M31 = s, M33 = c };
            }

            static Matrix AxisZ(float a)
            {
                float c = MathF.Cos(a), s = MathF.Sin(a);
                return new Matrix { M11 = c, M12 = s, M21 = -s, M22 = 1, M33 = 1 };
            }

            public static Matrix Multiply(Matrix a, Matrix b)
            {
                return new Matrix
                {
                    M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                    M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                    M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
                    M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                    M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                    M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
                    M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                    M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                    M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33,
                    TX = a.TX * b.M11 + a.TY * b.M21 + a.TZ * b.M31 + b.TX,
                    TY = a.TX * b.M12 + a.TY * b.M22 + a.TZ * b.M32 + b.TY,
                    TZ = a.TX * b.M13 + a.TY * b.M23 + a.TZ * b.M33 + b.TZ,
                };
            }

            public Vector3 Apply(Vector3 v) =>
                new Vector3
                {
                    X = M11 * v.X + M12 * v.Y + M13 * v.Z + TX,
                    Y = M21 * v.X + M22 * v.Y + M23 * v.Z + TY,
                    Z = M31 * v.X + M32 * v.Y + M33 * v.Z + TZ,
                };
        }

        // -----------------------------------------------------------------------
        // .x output
        // -----------------------------------------------------------------------

        static string BuildXFile(List<float> positions, List<float> normals,
                                 List<float> uvs, List<int[]> faces,
                                 string textureFile)
        {
            StringBuilder x = new StringBuilder();

            x.AppendLine("xof 0303txt 0032");

            AppendTemplates(x);

            x.AppendLine("Header {");
            x.AppendLine("\t1; //Major version");
            x.AppendLine("\t0; //Minor version");
            x.AppendLine("\t1; //Flags");
            x.AppendLine("}");

            x.AppendLine("Mesh ConvertedMesh {");
            x.AppendLine($"\t{positions.Count / 3};\t//{positions.Count / 3} vertices");

            for (int i = 0; i < positions.Count; i += 3)
            {
                x.AppendLine($"\t{F(positions[i])};{F(positions[i + 1])};{F(positions[i + 2])};,");
            }

            x.AppendLine($"\t{faces.Count};\t//{faces.Count} faces");

            for (int i = 0; i < faces.Count; i++)
            {
                int[] face = faces[i];
                x.AppendLine($"\t3;{face[0]},{face[1]},{face[2]};;");
            }

            // One material for every face.
            x.AppendLine("\tMeshMaterialList {");
            x.AppendLine("\t\t1;");
            x.AppendLine($"\t\t{faces.Count};");
            x.Append("\t\t0");

            for (int i = 1; i < faces.Count; i++)
            {
                x.Append(",0");
            }
            x.AppendLine(";;");

            x.AppendLine("\t\tMaterial Material0 {");
            x.AppendLine("\t\t\t1.000000;1.000000;1.000000;1.000000;;");
            x.AppendLine("\t\t\t18.000000;");
            x.AppendLine("\t\t\t0.000000;0.000000;0.000000;;");
            x.AppendLine("\t\t\t0.000000;0.000000;0.000000;;");

            if (textureFile != null)
            {
                x.AppendLine("\t\t\tTextureFilename {");
                x.AppendLine($"\t\t\t\t\"{textureFile}\";");
                x.AppendLine("\t\t\t}");
            }

            x.AppendLine("\t\t}");
            x.AppendLine("\t}");

            // Normals, indexed exactly like the vertices.
            x.AppendLine("\tMeshNormals {");
            x.AppendLine($"\t\t{normals.Count / 3};");

            for (int i = 0; i < normals.Count; i += 3)
            {
                x.AppendLine($"\t\t{F(normals[i])};{F(normals[i + 1])};{F(normals[i + 2])};,");
            }

            x.AppendLine($"\t\t{faces.Count};");

            for (int i = 0; i < faces.Count; i++)
            {
                int[] face = faces[i];
                x.AppendLine($"\t3;{face[0]},{face[1]},{face[2]};;");
            }

            x.AppendLine("\t}");

            // Texture coordinates, one per vertex.
            x.AppendLine("\tMeshTextureCoords {");
            x.AppendLine($"\t\t{uvs.Count / 2};");

            for (int i = 0; i < uvs.Count; i += 2)
            {
                x.AppendLine($"\t\t{F(uvs[i])};{F(uvs[i + 1])};,");
            }

            x.AppendLine("\t}");
            x.AppendLine("}");

            return x.ToString();
        }

        static void AppendTemplates(StringBuilder x)
        {
            x.AppendLine(@"template Vector {
 <3D82AB5E-62DA-11cf-AB39-0020AF71E433>
 FLOAT x;
 FLOAT y;
 FLOAT z;
}

template MeshFace {
 <3D82AB5F-62DA-11cf-AB39-0020AF71E433>
 DWORD nFaceVertexIndices;
 array DWORD faceVertexIndices[nFaceVertexIndices];
}

template Mesh {
 <3D82AB44-62DA-11cf-AB39-0020AF71E433>
 DWORD nVertices;
 array Vector vertices[nVertices];
 DWORD nFaces;
 array MeshFace faces[nFaces];
 [...]
}

template Coords2d {
 <F6F23F44-7686-11cf-8F52-0040333594A3>
 FLOAT u;
 FLOAT v;
}

template MeshTextureCoords {
 <F6F23F40-7686-11cf-8F52-0040333594A3>
 DWORD nTextureCoords;
 array Coords2d textureCoords[nTextureCoords];
}

template MeshNormals {
 <F6F23F43-7686-11cf-8F52-0040333594A3>
 DWORD nNormals;
 array Vector normals[nNormals];
 DWORD nFaceNormals;
 array MeshFace faceNormals[nFaceNormals];
}

template TextureFilename {
 <A42790E1-7810-11cf-8F52-0040333594A3>
 STRING filename;
}

template Material {
 <3D82AB4D-62DA-11cf-AB39-0020AF71E433>
 ColorRGBA faceColor;
 FLOAT power;
 ColorRGB specularColor;
 ColorRGB emissiveColor;
 [...]
}

template MeshMaterialList {
 <F6F23F42-7686-11cf-8F52-0040333594A3>
 DWORD nMaterials;
 DWORD nFaceIndexes;
 array DWORD faceIndexes[nFaceIndexes];
 [Material]
}

template ColorRGB {
 <D3E16E81-7838-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
}

template ColorRGBA {
 <35FF44E1-6C7C-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
 FLOAT alpha;
}");
        }

        static string F(float value)
        {
            return value.ToString("0.000000",
                                  System.Globalization.CultureInfo.InvariantCulture);
        }

        // -----------------------------------------------------------------------
        // FBX array parsing (FBX 6.0 inline form: "Name: v,v,v,..." spanning
        // lines, terminated by a blank line or a non numeric line)
        // -----------------------------------------------------------------------

        static float[] ReadFloatArray(string body, string name)
        {
            return ParseFloatList(ReadArrayText(body, name));
        }

        static int[] ReadIntArray(string body, string name)
        {
            return ParseIntList(ReadArrayText(body, name));
        }

        static string ReadArrayText(string body, string name)
        {
            Regex start = new Regex(
                $@"(?m)^\s*{Regex.Escape(name)}:\s*(.*)$",
                RegexOptions.Compiled);

            Match match = start.Match(body);

            if (!match.Success)
            {
                throw new InvalidDataException($"Missing {name} array.");
            }

            StringBuilder text = new StringBuilder();
            text.Append(match.Groups[1].Value);

            int position = body.IndexOf('\n', match.Index + match.Length);

            if (position >= 0)
            {
                // Skip past the first line's newline.
                position += 1;
            }

            while (position >= 0 && position < body.Length)
            {
                int end = body.IndexOf('\n', position);
                string line = (end < 0
                    ? body.Substring(position)
                    : body.Substring(position, end - position)).Trim();

                if (line.Length == 0 || !IsNumericLine(line))
                {
                    break;
                }

                text.Append(',');
                text.Append(line);

                position = end < 0 ? -1 : end + 1;
            }

            return text.ToString();
        }

        static bool IsNumericLine(string line)
        {
            // A continuation line of an FBX array only holds numbers,
            // separators and exponent characters.
            return Regex.IsMatch(line, @"^[\d.eE+,+\s-]*$");
        }

        /// <summary>
        /// Reads one attribute layer (normals or UVs) including its mapping
        /// and reference information.
        /// </summary>
        static MeshLayer ReadLayer(string body, string layerName,
                                   string dataName, string indexName)
        {
            int layerIndex = body.IndexOf(layerName, StringComparison.Ordinal);

            if (layerIndex < 0)
            {
                return null;
            }

            // The layer ends at the next "LayerElement" or the end of the body.
            int nextLayer = body.IndexOf("LayerElement", layerIndex + 1,
                                         StringComparison.Ordinal);

            string slice = nextLayer > 0
                ? body.Substring(layerIndex, nextLayer - layerIndex)
                : body.Substring(layerIndex);

            MeshLayer layer = new MeshLayer
            {
                Mapping = ReadValue(slice, "MappingInformationType"),
                Reference = ReadValue(slice, "ReferenceInformationType"),
            };

            layer.Data = ParseFloatListSafe(ReadArrayTextSafe(slice, dataName));
            layer.Indices = ParseIntListSafe(ReadArrayTextSafe(slice, indexName));

            return layer;
        }

        static string ReadArrayTextSafe(string body, string name)
        {
            try
            {
                return ReadArrayText(body, name);
            }
            catch (InvalidDataException)
            {
                return null;
            }
        }

        static float[] ParseFloatListSafe(string text)
        {
            return text == null ? null : ParseFloatList(text);
        }

        static int[] ParseIntListSafe(string text)
        {
            return text == null ? null : ParseIntList(text);
        }

        static string ReadValue(string body, string name)
        {
            Match match = Regex.Match(body, $@"{name}:\s+""([^""]*)""");
            return match.Success ? match.Groups[1].Value : null;
        }

        static float[] ParseFloatList(string text)
        {
            string[] tokens = text.Split(new[] { ',', '\n', '\r', ' ', '\t' },
                                         StringSplitOptions.RemoveEmptyEntries);

            float[] result = new float[tokens.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                result[i] = float.Parse(tokens[i],
                                        System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture);
            }

            return result;
        }

        static int[] ParseIntList(string text)
        {
            string[] tokens = text.Split(new[] { ',', '\n', '\r', ' ', '\t' },
                                         StringSplitOptions.RemoveEmptyEntries);

            int[] result = new int[tokens.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                result[i] = int.Parse(tokens[i],
                                      System.Globalization.CultureInfo.InvariantCulture);
            }

            return result;
        }
    }
}
