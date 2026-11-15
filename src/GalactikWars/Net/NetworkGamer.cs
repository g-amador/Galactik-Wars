#region File Description
//-----------------------------------------------------------------------------
//NetworkGamer.cs
//
//Galactik Wars LAN networking layer.
//Gamer types mirroring the XNA networking gamer classes, plus the gamer
//collections used by the screens.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#endregion

namespace GalactikWars.Net
{
    ///<summary>
    ///Describes a signed in player profile. The XNA version exposed Xbox LIVE
    ///profile data; the LAN version generates a small identity picture.
    ///</summary>
    public class SignedInGamerProfile
    {
        ///<summary>
        ///Gets the profile picture texture. Created lazily on first access,
        ///using the graphics device of the active session.
        ///</summary>
        public Texture2D GamerPicture
        {
            get
            {
                GraphicsDevice device = NetworkSession.Current != null
                    ? NetworkSession.Current.GraphicsDevice
                    : null;

                if (gamerPicture == null && device != null)
                {
                    gamerPicture = CreateGamerPicture(device, Color);
                }
                return gamerPicture;
            }
        }

        internal Color Color;

        private Texture2D gamerPicture;

        ///<summary>
        ///Creates a small colored square with a darker border as the picture.
        ///</summary>
        private static Texture2D CreateGamerPicture(GraphicsDevice device, Color color)
        {
            const int size = 32;

            Color[] data = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool border = (x < 2) || (y < 2) ||
                                  (x >= size - 2) || (y >= size - 2);

                    data[y * size + x] = border ? Color.Black : color;
                }
            }

            Texture2D texture = new(device, size, size);
            texture.SetData(data);
            return texture;
        }
    }

    ///<summary>
    ///Describes a signed in player. The XNA version was created by the
    ///Guide sign-in UI; the LAN version identifies the player by their
    ///Windows user name.
    ///</summary>
    public class SignedInGamer
    {
        #region Properties

        ///<summary>
        ///Gets the player gamertag.
        ///</summary>
        public string Gamertag { get; internal set; }

        ///<summary>
        ///Gets the controller index of this gamer. On the PC there is only
        ///a single logical local player, so this is always PlayerIndex.One.
        ///</summary>
        public PlayerIndex PlayerIndex
        {
            get { return PlayerIndex.One; }
        }

        #endregion

        #region Initialization

        internal SignedInGamer(string gamertag, byte id)
        {
            Gamertag = gamertag;
            this.id = id;
        }

        private readonly byte id;

        #endregion

        #region Profile

        private SignedInGamerProfile profile;

        ///<summary>
        ///Gets the profile for this gamer.
        ///</summary>
        public SignedInGamerProfile GetProfile()
        {
            profile ??= new SignedInGamerProfile
            {
                Color = NetworkSession.GamerColor(id),
            };
            return profile;
        }

        #endregion
    }

    ///<summary>
    ///Describes one player taking part in a network session.
    ///</summary>
    public class NetworkGamer
    {
        #region Properties

        ///<summary>
        ///Gets the player gamertag.
        ///</summary>
        public string Gamertag { get; internal set; }

        ///<summary>
        ///Gets whether this gamer is the host of the session.
        ///</summary>
        public bool IsHost { get; internal set; }

        ///<summary>
        ///Gets or sets whether this gamer is marked as ready to play.
        ///</summary>
        public bool IsReady { get; set; }

        ///<summary>
        ///Gets whether this gamer is a player on the local machine.
        ///</summary>
        public virtual bool IsLocal
        {
            get { return false; }
        }

        ///<summary>
        ///Gets whether this gamer has been removed from the session.
        ///</summary>
        public bool IsDisposed { get; internal set; }

        ///<summary>
        ///Voice chat is not supported by the LAN implementation,
        ///so this is always false.
        ///</summary>
        public bool IsTalking
        {
            get { return false; }
        }

        ///<summary>
        ///Voice chat is not supported by the LAN implementation,
        ///so this is always true (everyone "has voice" but is never talking).
        ///</summary>
        public bool HasVoice
        {
            get { return true; }
        }

        ///<summary>
        ///Voice chat is not supported by the LAN implementation,
        ///so this is always false.
        ///</summary>
        public bool IsMutedByLocalUser
        {
            get { return false; }
        }

        ///<summary>
        ///Unique byte id of this gamer within the session.
        ///</summary>
        internal byte Id { get; set; }

        #endregion

        #region Initialization

        internal NetworkGamer(byte id, string gamertag, bool isHost)
        {
            Id = id;
            Gamertag = gamertag;
            IsHost = isHost;
        }

        #endregion

        #region Profile

        ///<summary>
        ///Gets the profile for this gamer.
        ///</summary>
        public SignedInGamerProfile GetProfile()
        {
            return signedInGamer.GetProfile();
        }

        internal SignedInGamer signedInGamer;

        #endregion
    }

    ///<summary>
    ///Describes a player on the local machine taking part in a session.
    ///</summary>
    public class LocalNetworkGamer : NetworkGamer
    {
        #region Properties

        ///<summary>
        ///Gets whether this gamer is a player on the local machine.
        ///</summary>
        public override bool IsLocal
        {
            get { return true; }
        }

        ///<summary>
        ///Gets the signed in gamer corresponding to this local player.
        ///</summary>
        public SignedInGamer SignedInGamer
        {
            get { return signedInGamer; }
        }

        ///<summary>
        ///Gets whether there is network data waiting to be read.
        ///</summary>
        public bool IsDataAvailable
        {
            get { return incomingPackets.Count > 0; }
        }

        ///<summary>
        ///Marks whether this gamer is ready. Setting this property sends
        ///the state to the other players in the session.
        ///</summary>
        public bool Ready
        {
            get { return IsReady; }
            set { IsReady = value; }
        }

        #endregion

        #region Fields

        internal class IncomingPacket
        {
            public NetworkGamer Sender;
            public byte[] Payload;
        }

        internal readonly Queue<IncomingPacket> incomingPackets =
            new();

        #endregion

        #region Initialization

        internal LocalNetworkGamer(byte id, string gamertag, bool isHost)
            : base(id, gamertag, isHost)
        {
            signedInGamer = new SignedInGamer(gamertag, id);
        }

        #endregion

        #region Receive Data

        ///<summary>
        ///Reads one pending packet. The packet payload is loaded into the
        ///packet reader, and the sender of the packet is output.
        ///</summary>
        public bool ReceiveData(PacketReader packetReader, out NetworkGamer sender)
        {
            sender = null;

            if (incomingPackets.Count == 0)
            {
                return false;
            }

            IncomingPacket packet = incomingPackets.Dequeue();

            packetReader.SetBuffer(packet.Payload);

            sender = packet.Sender;
            return true;
        }

        #endregion

        #region Send Data

        ///<summary>
        ///Sends a packet to all the other players in the session.
        ///</summary>
        public void SendData(PacketWriter packetWriter, SendDataOptions options)
        {
            if (NetworkSession.Current != null)
            {
                NetworkSession.Current.SendDataInternal(this,
                                                        packetWriter.GetBuffer());
            }
        }

        #endregion
    }

    ///<summary>
    ///Collection of the gamers taking part in a network session.
    ///</summary>
    public class NetworkGamerCollection : IReadOnlyList<NetworkGamer>
    {
        private readonly List<NetworkGamer> gamers = [];

        public int Count
        {
            get { return gamers.Count; }
        }

        public NetworkGamer this[int index]
        {
            get { return gamers[index]; }
        }

        internal void Add(NetworkGamer gamer)
        {
            gamers.Add(gamer);
        }

        internal void Remove(NetworkGamer gamer)
        {
            gamers.Remove(gamer);
        }

        internal void Clear()
        {
            gamers.Clear();
        }

        internal void Sort(IComparer<NetworkGamer> comparer)
        {
            gamers.Sort(comparer);
        }

        ///<summary>
        ///Finds the position of the specified gamer in the collection.
        ///</summary>
        public int IndexOf(NetworkGamer gamer)
        {
            return gamers.IndexOf(gamer);
        }

        internal NetworkGamer FindById(byte id)
        {
            foreach (NetworkGamer gamer in gamers)
            {
                if (gamer.Id == id)
                {
                    return gamer;
                }
            }
            return null;
        }

        IEnumerator<NetworkGamer> IEnumerable<NetworkGamer>.GetEnumerator()
        {
            return gamers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return gamers.GetEnumerator();
        }
    }

    ///<summary>
    ///Collection of the gamers playing on the local machine.
    ///</summary>
    public class LocalNetworkGamerCollection : IReadOnlyList<LocalNetworkGamer>
    {
        private readonly List<LocalNetworkGamer> gamers = [];

        public int Count
        {
            get { return gamers.Count; }
        }

        public LocalNetworkGamer this[int index]
        {
            get { return gamers[index]; }
        }

        internal void Add(LocalNetworkGamer gamer)
        {
            gamers.Add(gamer);
        }

        internal void Clear()
        {
            gamers.Clear();
        }

        ///<summary>
        ///Finds the position of the specified gamer in the collection.
        ///</summary>
        public int IndexOf(LocalNetworkGamer gamer)
        {
            return gamers.IndexOf(gamer);
        }

        IEnumerator<LocalNetworkGamer> IEnumerable<LocalNetworkGamer>.GetEnumerator()
        {
            return gamers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return gamers.GetEnumerator();
        }
    }
}
