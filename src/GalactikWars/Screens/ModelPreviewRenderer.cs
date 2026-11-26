#region File Description
//-----------------------------------------------------------------------------
//ModelPreviewRenderer.cs
//
//Draws a rotating 3D ship model inside a rectangle of the back buffer, so the
//menu screens can show the real hulls (and their team colors) before the
//match starts. The camera sits in front of the hull and looks down at it from
//30 degrees, so the spin shows its top and sides instead of a flat top view.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
#endregion

namespace GalactikWars.Screens
{
    internal sealed class ModelPreviewRenderer
    {
        #region Fields

        //how much of the preview's height the hull's bounding sphere fills
        private const float FillFactor = 0.90f;

        //the preview camera sits this many hull radii away and uses a narrow
        //field of view, so the hull keeps its proportions while the camera
        //looks down at it
        private const float CameraDistanceInRadii = 40.0f;

        //the camera looks down at the hull from 30 degrees above the plane,
        //so the ship is seen from the front instead of straight from above
        private const float CameraElevation = MathHelper.Pi / 6.0f;

        private readonly ContentManager content;
        private readonly GraphicsDevice graphicsDevice;

        private readonly Dictionary<string, Model> models = [];
        private readonly Dictionary<string, Matrix[]> transforms = [];
        private readonly Dictionary<string, BoundingSphere> boundingSpheres = [];

        #endregion

        #region Initialization

        public ModelPreviewRenderer(ContentManager content,
            GraphicsDevice graphicsDevice)
        {
            this.content = content;
            this.graphicsDevice = graphicsDevice;
        }

        #endregion

        #region Loading

        ///<summary>
        ///Loads (once) a model and caches its absolute bone transforms and the
        ///bounding sphere that frames it in the preview.
        ///</summary>
        public Model LoadModel(string modelName)
        {
            if (models.TryGetValue(modelName, out Model model))
            {
                return model;
            }

            model = content.Load<Model>(modelName);

            Matrix[] absoluteTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            BoundingSphere sphere = default;
            bool hasSphere = false;

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere meshSphere = mesh.BoundingSphere.Transform(
                    absoluteTransforms[mesh.ParentBone.Index]);

                if (!hasSphere)
                {
                    sphere = meshSphere;
                    hasSphere = true;
                }
                else
                {
                    sphere = BoundingSphere.CreateMerged(sphere, meshSphere);
                }
            }

            models[modelName] = model;
            transforms[modelName] = absoluteTransforms;
            boundingSpheres[modelName] = sphere;

            return model;
        }

        #endregion

        #region Draw

        ///<summary>
        ///Draws a hull spinning around its own center inside <paramref name="bounds"/>.
        ///Pass a tint to draw the ambient light in a team color (allies and
        ///enemies); the pilot's own hull is drawn untinted, like in gameplay.
        ///</summary>
        public void Draw(string modelName, Rectangle bounds, float rotation,
            Vector3? tint)
        {
            Model model = LoadModel(modelName);

            if (model == null || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            Matrix[] absoluteTransforms = transforms[modelName];
            BoundingSphere sphere = boundingSpheres[modelName];
            float radius = MathF.Max(sphere.Radius, 1.0f);

            //frame the hull from far away with a narrow field of view, then
            //look at it from the front, 30 degrees above its plane
            float aspect = (float)bounds.Width / bounds.Height;
            float halfHeight = radius / FillFactor;

            //tall, narrow previews need a wider view so the sphere still fits
            //horizontally
            if (aspect < 1.0f)
            {
                halfHeight /= aspect;
            }

            float distance = radius * CameraDistanceInRadii;
            float fieldOfView = 2.0f * MathF.Atan(halfHeight / distance);

            Vector3 cameraPosition = new Vector3(0.0f,
                MathF.Cos(CameraElevation), MathF.Sin(CameraElevation))
                * distance;

            Matrix view = Matrix.CreateLookAt(cameraPosition,
                Vector3.Zero, Vector3.UnitZ);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView,
                aspect, MathF.Max(1.0f, distance - 2 * radius),
                distance + 2 * radius);

            //same orientation convention as Ship.RotationMatrix (the nose stays
            //in the top-down plane), spun about the hull's own center
            Matrix world = Matrix.CreateTranslation(-sphere.Center) *
                Matrix.CreateRotationX(MathHelper.PiOver2) *
                Matrix.CreateRotationZ(rotation);

            Viewport previousViewport = graphicsDevice.Viewport;
            graphicsDevice.Viewport = new Viewport(bounds.X, bounds.Y,
                bounds.Width, bounds.Height);

            //previews draw first in the frame; clearing depth keeps stale depth
            //from the previous frame from poking holes in the hulls
            graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.BlendState = BlendState.Opaque;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor =
                        tint ?? new Vector3(0.55f, 0.55f, 0.55f);
                    effect.World =
                        absoluteTransforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }

            graphicsDevice.Viewport = previousViewport;
        }

        #endregion
    }
}
