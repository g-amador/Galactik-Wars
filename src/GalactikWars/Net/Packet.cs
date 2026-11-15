#region File Description
//-----------------------------------------------------------------------------
//PacketWriter.cs
//
//Galactik Wars LAN networking layer.
//Serialization helpers mirroring the XNA PacketWriter/PacketReader API,
//backed by System.IO.BinaryWriter/BinaryReader.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System.IO;
#endregion

namespace GalactikWars.Net
{
    ///<summary>
    ///Writes primitive types and XNA structures into a network packet buffer.
    ///</summary>
    public class PacketWriter : BinaryWriter
    {
        #region Fields

        private readonly MemoryStream memoryStream;

        #endregion

        #region Initialization

        ///<summary>
        ///Constructs a new packet writer.
        ///</summary>
        public PacketWriter()
            : this(new MemoryStream())
        { }

        private PacketWriter(MemoryStream memoryStream)
            : base(memoryStream)
        {
            this.memoryStream = memoryStream;
        }

        #endregion

        #region Methods

        ///<summary>
        ///Writes a Vector3 value.
        ///</summary>
        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }

        ///<summary>
        ///Clears the buffer, ready to write a new packet.
        ///</summary>
        public void Flush()
        {
            base.Flush();
            memoryStream.SetLength(0);
            memoryStream.Position = 0;
        }

        ///<summary>
        ///Gets the packet payload bytes written so far.
        ///</summary>
        internal byte[] GetBuffer()
        {
            base.Flush();
            return memoryStream.ToArray();
        }

        #endregion
    }

    ///<summary>
    ///Reads primitive types and XNA structures from a network packet buffer.
    ///</summary>
    public class PacketReader : BinaryReader
    {
        #region Initialization

        ///<summary>
        ///Constructs a new packet reader.
        ///</summary>
        public PacketReader()
            : base(new MemoryStream())
        { }

        #endregion

        #region Methods

        ///<summary>
        ///Reads a Vector3 value.
        ///</summary>
        public Vector3 ReadVector3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        ///<summary>
        ///Replaces the underlying buffer, ready to read a new packet.
        ///</summary>
        internal void SetBuffer(byte[] buffer)
        {
            ((MemoryStream)BaseStream).SetLength(0);
            ((MemoryStream)BaseStream).Write(buffer, 0, buffer.Length);
            ((MemoryStream)BaseStream).Position = 0;
        }

        #endregion
    }
}
