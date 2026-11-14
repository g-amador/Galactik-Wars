#region File Description
//-----------------------------------------------------------------------------
//NetworkSession.cs
//
//Galactik Wars LAN networking layer.
//Reimplementation of the XNA NetworkSession API over UDP, supporting
//session discovery, join, lobby, gameplay data relay and host timeout
//handling. Host migration is accepted but not implemented: when the host
//goes away the session ends with a disconnect notification.
//
//Topology:
//  - The host owns the session state and listens on the well known port.
//  - Clients talk to the host; gameplay data is relayed by the host to
//    all other clients (a simple star topology, adequate for the six
//    player maximum of the original game).
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GalactikWars.Net
{
    ///<summary>
    ///Event arguments for the GamerJoined event.
    ///</summary>
    public class GamerJoinedEventArgs : EventArgs
    {
        public NetworkGamer Gamer { get; internal set; }
    }

    ///<summary>
    ///Event arguments for the GamerLeft event.
    ///</summary>
    public class GamerLeftEventArgs : EventArgs
    {
        public NetworkGamer Gamer { get; internal set; }
    }

    ///<summary>
    ///Event arguments for the SessionEnded event.
    ///</summary>
    public class NetworkSessionEndedEventArgs : EventArgs
    {
        public NetworkSessionEndReason EndReason { get; internal set; }
    }

    ///<summary>
    ///Manages one LAN network session: create, find, join, lobby, gameplay
    ///and shutdown, mirroring the XNA NetworkSession API surface.
    ///</summary>
    public class NetworkSession : IDisposable
    {
        #region Constants

        ///<summary>
        ///UDP port the session host listens on.
        ///</summary>
        const int DefaultPort = 26900;

        ///<summary>
        ///Interval between session discovery beacons, in milliseconds.
        ///</summary>
        const int BeaconInterval = 1000;

        ///<summary>
        ///Interval between roster synchronization packets, in milliseconds.
        ///</summary>
        const int RosterInterval = 500;

        ///<summary>
        ///How long a gamer may go silent before being dropped.
        ///</summary>
        const int GamerTimeout = 5000;

        ///<summary>
        ///How long a client may go without hearing from the host.
        ///</summary>
        const int HostTimeout = 5000;

        ///<summary>
        ///How long the session discovery search listens for replies.
        ///</summary>
        const int FindWindow = 2000;

        ///<summary>
        ///Timeout for each join attempt, in milliseconds.
        ///</summary>
        const int JoinReplyTimeout = 1500;

        ///<summary>Maximum number of gamers in a session.</summary>
        internal const int MaxGamersLimit = 6;

        #endregion

        #region Packet Types

        enum PacketType : byte
        {
            DiscoveryRequest = 1,
            DiscoveryReply = 2,
            JoinRequest = 3,
            JoinReplyOk = 4,
            JoinReplyFail = 5,
            Roster = 6,
            Ready = 7,
            StartGame = 8,
            EndGame = 9,
            Kick = 10,
            Leave = 11,
            Data = 12,
            Ping = 13,
            Pong = 14,
        }

        #endregion

        #region Fields

        Socket socket;

        readonly bool isHost;
        readonly List<IPEndPoint> clientEndpoints = new List<IPEndPoint>();

        readonly NetworkGamerCollection allGamers = new NetworkGamerCollection();
        readonly LocalNetworkGamerCollection localGamers = new LocalNetworkGamerCollection();

        NetworkSessionState state = NetworkSessionState.Lobby;

        DateTime lastBeacon = DateTime.MinValue;
        DateTime lastRoster = DateTime.MinValue;

        ///<summary>Last time each remote client was heard from (host only).</summary>
        readonly Dictionary<IPEndPoint, DateTime> clientSeen =
            new Dictionary<IPEndPoint, DateTime>();

        ///<summary>Last time the host was heard from (client only).</summary>
        DateTime hostSeen = DateTime.UtcNow;

        ///<summary>Receiving buffer.</summary>
        readonly byte[] receiveBuffer = new byte[8192];

        ///<summary>
        ///The id the host assigned to the local gamer of a joining client.
        ///</summary>
        byte pendingLocalId;

        ///<summary>Whether a join reply is still pending processing.</summary>
        bool pendingJoin;

        ///<summary>Endpoint of the host (clients only).</summary>
        IPEndPoint hostEndPoint;

        ///<summary>Graphics device, used to create gamer pictures.</summary>
        internal GraphicsDevice GraphicsDevice { get; set; }

        ///<summary>
        ///The session currently owned by this process, used by the local
        ///gamer SendData implementation and profile pictures.
        ///</summary>
        internal static NetworkSession Current { get; private set; }

        #endregion

        #region Events

        ///<summary>
        ///Event raised when a gamer joins the session.
        ///</summary>
        public event EventHandler<GamerJoinedEventArgs> GamerJoined;

        ///<summary>
        ///Event raised when a gamer leaves the session.
        ///</summary>
        public event EventHandler<GamerLeftEventArgs> GamerLeft;

        ///<summary>
        ///Event raised when the session ends.
        ///</summary>
        public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded;

        #endregion

        #region Properties

        ///<summary>
        ///Gets the gamers taking part in the session.
        ///</summary>
        public NetworkGamerCollection AllGamers
        {
            get { return allGamers; }
        }

        ///<summary>
        ///Gets the gamers playing on the local machine.
        ///</summary>
        public LocalNetworkGamerCollection LocalGamers
        {
            get { return localGamers; }
        }

        ///<summary>
        ///Gets whether the local player is the host of this session.
        ///</summary>
        public bool IsHost
        {
            get { return isHost; }
        }

        ///<summary>
        ///Gets whether every gamer has marked themselves as ready to play.
        ///</summary>
        public bool IsEveryoneReady
        {
            get
            {
                if (allGamers.Count == 0)
                {
                    return false;
                }

                foreach (NetworkGamer gamer in allGamers)
                {
                    if (!gamer.IsReady)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        ///<summary>
        ///Gets the current state of the session.
        ///</summary>
        public NetworkSessionState SessionState
        {
            get { return state; }
        }

        ///<summary>
        ///Gets or sets whether the host should migrate the session when the
        ///host leaves. Host migration is not implemented in the LAN layer;
        ///when the host goes away, the session ends with a disconnect
        ///notification.
        ///</summary>
        public bool AllowHostMigration { get; set; }

        ///<summary>
        ///Gets or sets whether players may join while the game is in progress.
        ///</summary>
        public bool AllowJoinInProgress { get; set; }

        #endregion

        #region Static Identity

        static string localGamertag;

        ///<summary>
        ///Gets or sets the gamertag used by the local player. Defaults to
        ///the Windows user name.
        ///</summary>
        public static string LocalGamertag
        {
            get
            {
                if (string.IsNullOrEmpty(localGamertag))
                {
                    string name = Environment.UserName;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "Player";
                    }
                    localGamertag = name;
                }
                return localGamertag;
            }
            set { localGamertag = value; }
        }

        #endregion

        #region Initialization

        ///<summary>
        ///Constructs a session. Use the static Create, Find and Join methods.
        ///</summary>
        NetworkSession(bool isHost)
        {
            this.isHost = isHost;

            socket = new Socket(AddressFamily.InterNetwork,
                                SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;
        }

        ///<summary>
        ///Creates a new network session hosted by this machine.
        ///</summary>
        public static NetworkSession Create(NetworkSessionType sessionType,
                                            int maxLocalGamers, int maxGamers)
        {
            NetworkSession session = new NetworkSession(true);

            try
            {
                session.socket.Bind(new IPEndPoint(IPAddress.Any, DefaultPort));
            }
            catch (SocketException exception)
            {
                session.socket.Close();
                session.socket = null;

                throw new NetworkException(
                    "Could not bind the session port (another session may " +
                    "already be hosted on this machine).", exception);
            }

            AddLocalGamer(session, 0, LocalGamertag, true);

            session.state = NetworkSessionState.Lobby;
            Current = session;

            return session;
        }

        ///<summary>
        ///Creates the local gamer for a new session, wiring up its profile.
        ///</summary>
        static void AddLocalGamer(NetworkSession session, byte id,
                                  string gamertag, bool isHost)
        {
            LocalNetworkGamer gamer = new LocalNetworkGamer(id, gamertag, isHost);

            session.allGamers.Add(gamer);
            session.localGamers.Add(gamer);
        }

        #endregion

        #region Find

        ///<summary>
        ///Searches the local network for available sessions.
        ///</summary>
        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType,
                                                             int maxLocalGamers,
                                                             object searchProperties)
        {
            AvailableNetworkSessionCollection results =
                new AvailableNetworkSessionCollection();

            Socket searchSocket = new Socket(AddressFamily.InterNetwork,
                                             SocketType.Dgram, ProtocolType.Udp);
            try
            {
                searchSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                searchSocket.Blocking = false;

                //Broadcast a discovery request to the well known port.
                byte[] request = EncodePacket(PacketType.DiscoveryRequest, null);
                searchSocket.SendTo(request,
                    new IPEndPoint(IPAddress.Broadcast, DefaultPort));

                //Collect replies for a fixed window of time.
                DateTime deadline = DateTime.UtcNow.AddMilliseconds(FindWindow);
                byte[] buffer = new byte[8192];

                Dictionary<IPEndPoint, int> pingSentAt =
                    new Dictionary<IPEndPoint, int>();

                while (DateTime.UtcNow < deadline)
                {
                    if (!searchSocket.Poll(10000, SelectMode.SelectRead))
                    {
                        continue;
                    }

                    EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    int length = searchSocket.ReceiveFrom(buffer, ref remote);

                    PacketType type = (PacketType)buffer[0];

                    if (type == PacketType.DiscoveryReply)
                    {
                        //Parse the reply.
                        IPEndPoint endPoint = (IPEndPoint)remote;

                        bool duplicate = false;

                        foreach (AvailableNetworkSession existing in results)
                        {
                            if (existing.HostEndPoint.Equals(endPoint))
                            {
                                duplicate = true;
                                break;
                            }
                        }

                        if (!duplicate)
                        {
                            AvailableNetworkSession available =
                                ParseDiscoveryReply(buffer, length);

                            if (available != null)
                            {
                                available.HostEndPoint = endPoint;
                                results.Add(available);

                                //Start measuring latency with a few pings.
                                byte[] ping = EncodePacket(PacketType.Ping, null);
                                for (int i = 0; i < 3; i++)
                                {
                                    searchSocket.SendTo(ping, endPoint);
                                }
                                pingSentAt[endPoint] = Environment.TickCount;
                            }
                        }
                    }
                    else if (type == PacketType.Pong)
                    {
                        if (pingSentAt.TryGetValue((IPEndPoint)remote, out int sentAt))
                        {
                            AvailableNetworkSession session = FindSession(results,
                                (IPEndPoint)remote);

                            if (session != null)
                            {
                                int elapsed = Environment.TickCount - sentAt;

                                session.QualityOfService = new QualityOfService
                                {
                                    IsAvailable = true,
                                    AverageRoundtripTime =
                                        TimeSpan.FromMilliseconds(elapsed),
                                };
                            }
                        }
                    }
                }
            }
            catch (SocketException exception)
            {
                throw new NetworkException(
                    "Session discovery failed.", exception);
            }
            finally
            {
                searchSocket.Close();
            }

            return results;
        }

        static AvailableNetworkSession FindSession(
            AvailableNetworkSessionCollection sessions, IPEndPoint endPoint)
        {
            foreach (AvailableNetworkSession session in sessions)
            {
                if (session.HostEndPoint.Equals(endPoint))
                {
                    return session;
                }
            }
            return null;
        }

        #endregion

        #region Join

        ///<summary>
        ///Joins the specified available network session.
        ///</summary>
        public static NetworkSession Join(AvailableNetworkSession availableSession)
        {
            NetworkSession session = new NetworkSession(false);

            try
            {
                session.socket.Bind(new IPEndPoint(IPAddress.Any, 0));

                IPEndPoint host = availableSession.HostEndPoint;
                session.hostEndPoint = host;

                byte[] request = EncodePacket(PacketType.JoinRequest, writer =>
                    writer.Write(LocalGamertag));

                //Send the join request, waiting for a reply.
                byte[] buffer = new byte[8192];
                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

                for (int attempt = 0; attempt < 4; attempt++)
                {
                    session.socket.SendTo(request, host);

                    if (WaitForReply(session.socket, ref remote,
                                     buffer, JoinReplyTimeout))
                    {
                        PacketType type = (PacketType)buffer[0];

                        if (type == PacketType.JoinReplyOk)
                        {
                            session.ReadJoinReplyOk(buffer);
                            session.hostSeen = DateTime.UtcNow;
                            Current = session;
                            return session;
                        }

                        if (type == PacketType.JoinReplyFail)
                        {
                            session.socket.Close();
                            session.socket = null;

                            NetworkSessionJoinError error = buffer[1] == 0
                                ? NetworkSessionJoinError.SessionFull
                                : NetworkSessionJoinError.SessionNotJoinable;

                            throw new NetworkSessionJoinException(error);
                        }
                    }
                }

                throw new NetworkSessionJoinException(
                    NetworkSessionJoinError.SessionNotFound);
            }
            catch (SocketException exception)
            {
                throw new NetworkException("Could not join the session.", exception);
            }
        }

        ///<summary>
        ///Waits for any packet to arrive on the socket, up to timeout ms.
        ///</summary>
        static bool WaitForReply(Socket socket, ref EndPoint remote,
                                 byte[] buffer, int timeout)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeout);

            while (DateTime.UtcNow < deadline)
            {
                if (socket.Poll(50000, SelectMode.SelectRead))
                {
                    int length = socket.ReceiveFrom(buffer, ref remote);
                    return length > 0;
                }
            }
            return false;
        }

        #endregion

        #region Asynchronous Wrappers

        ///<summary>
        ///Begins an asynchronous session creation. The returned Task doubles
        ///as the IAsyncResult the original XNA API exposed.
        ///</summary>
        public static IAsyncResult BeginCreate(NetworkSessionType sessionType,
                                               int maxLocalGamers, int maxGamers,
                                               AsyncCallback callback, object state)
        {
            return Task.Run(() => Create(sessionType, maxLocalGamers, maxGamers));
        }

        ///<summary>
        ///Ends an asynchronous session creation.
        ///</summary>
        public static NetworkSession EndCreate(IAsyncResult asyncResult)
        {
            return UnwrapResult<NetworkSession>(asyncResult);
        }

        ///<summary>
        ///Begins an asynchronous session search.
        ///</summary>
        public static IAsyncResult BeginFind(NetworkSessionType sessionType,
                                             int maxLocalGamers,
                                             object searchProperties,
                                             AsyncCallback callback, object state)
        {
            return Task.Run(() => Find(sessionType, maxLocalGamers, searchProperties));
        }

        ///<summary>
        ///Ends an asynchronous session search.
        ///</summary>
        public static AvailableNetworkSessionCollection EndFind(IAsyncResult asyncResult)
        {
            return UnwrapResult<AvailableNetworkSessionCollection>(asyncResult);
        }

        ///<summary>
        ///Begins an asynchronous session join.
        ///</summary>
        public static IAsyncResult BeginJoin(AvailableNetworkSession availableSession,
                                             AsyncCallback callback, object state)
        {
            return Task.Run(() => Join(availableSession));
        }

        ///<summary>
        ///Ends an asynchronous session join.
        ///</summary>
        public static NetworkSession EndJoin(IAsyncResult asyncResult)
        {
            return UnwrapResult<NetworkSession>(asyncResult);
        }

        ///<summary>
        ///Waits for the result of an asynchronous operation, unwrapping any
        ///exception thrown by the operation itself.
        ///</summary>
        static T UnwrapResult<T>(IAsyncResult asyncResult)
        {
            Task<T> task = asyncResult as Task<T>;

            if (task == null)
            {
                throw new ArgumentException("Invalid asynchronous result.");
            }

            try
            {
                task.Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerExceptions[0];
            }

            return task.Result;
        }

        #endregion

        #region Session Lifecycle

        ///<summary>
        ///Transitions the session into gameplay. Only the host may do this.
        ///</summary>
        public void StartGame()
        {
            if (!isHost)
            {
                throw new NetworkException("Only the host can start the game.");
            }

            state = NetworkSessionState.Playing;

            Broadcast(EncodePacket(PacketType.StartGame, null));
        }

        ///<summary>
        ///Returns the session to the lobby. Only the host may do this.
        ///</summary>
        public void EndGame()
        {
            if (!isHost)
            {
                throw new NetworkException("Only the host can end the game.");
            }

            state = NetworkSessionState.Lobby;

            Broadcast(EncodePacket(PacketType.EndGame, null));
        }

        ///<summary>
        ///Disposes the session, disconnecting all the other players.
        ///</summary>
        public void Dispose()
        {
            if (socket != null)
            {
                //Tell the others we are going away.
                try
                {
                    if (isHost)
                    {
                        Broadcast(EncodePacket(PacketType.Kick, null));
                    }
                    else if (localGamers.Count > 0)
                    {
                        SendToHost(EncodePacket(PacketType.Leave, writer =>
                            writer.Write(localGamers[0].Id)));
                    }
                }
                catch (SocketException)
                {
                    //The network may already be gone; nothing to do.
                }

                socket.Close();
                socket = null;
            }

            if (state != NetworkSessionState.Ended)
            {
                state = NetworkSessionState.Ended;
            }

            if (Current == this)
            {
                Current = null;
            }
        }

        ///<summary>
        ///Ends the session locally, raising the SessionEnded event.
        ///</summary>
        void EndSession(NetworkSessionEndReason reason)
        {
            if (state == NetworkSessionState.Ended)
            {
                return;
            }

            state = NetworkSessionState.Ended;

            if (SessionEnded != null)
            {
                SessionEnded(this, new NetworkSessionEndedEventArgs
                {
                    EndReason = reason,
                });
            }
        }

        #endregion

        #region Update

        ///<summary>
        ///Pumps the network, processing all pending packets and timers.
        ///Call this once per frame from a game component.
        ///</summary>
        public void Update()
        {
            if (state == NetworkSessionState.Ended || socket == null)
            {
                return;
            }

            //Read every pending datagram.
            while (socket.Poll(0, SelectMode.SelectRead))
            {
                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

                int length;
                try
                {
                    length = socket.ReceiveFrom(receiveBuffer, ref remote);
                }
                catch (SocketException)
                {
                    //A datagram may have been dropped; keep going.
                    continue;
                }

                if (length > 0)
                {
                    try
                    {
                        if (isHost)
                        {
                            ProcessPacketAsHost((PacketType)receiveBuffer[0],
                                                receiveBuffer, length,
                                                (IPEndPoint)remote);
                        }
                        else
                        {
                            ProcessPacketAsClient((PacketType)receiveBuffer[0],
                                                  receiveBuffer, length,
                                                  (IPEndPoint)remote);
                        }
                    }
                    catch (SocketException exception)
                    {
                        Trace.WriteLine("Network send failed: " + exception.Message);
                    }
                }
            }

            if (isHost)
            {
                UpdateAsHost();
            }
            else
            {
                UpdateAsClient();
            }
        }

        #endregion

        #region Host Logic

        void UpdateAsHost()
        {
            DateTime now = DateTime.UtcNow;

            //Drop clients we have not heard from in a while.
            for (int i = clientEndpoints.Count - 1; i >= 0; i--)
            {
                IPEndPoint endpoint = clientEndpoints[i];

                if (now - clientSeen[endpoint] >
                    TimeSpan.FromMilliseconds(GamerTimeout))
                {
                    NetworkGamer gamer = FindGamerByEndpoint(endpoint);

                    if (gamer != null)
                    {
                        RemoveGamer(gamer);
                    }
                }
            }

            //Broadcast the roster periodically.
            if ((now - lastRoster).TotalMilliseconds >= RosterInterval)
            {
                lastRoster = now;
                BroadcastRoster();
            }

            //Broadcast discovery beacons while in the lobby.
            if (state == NetworkSessionState.Lobby &&
                (now - lastBeacon).TotalMilliseconds >= BeaconInterval)
            {
                lastBeacon = now;

                Broadcast(EncodePacket(PacketType.DiscoveryReply, writer =>
                    WriteSessionInfo(writer)));
            }
        }

        void ProcessPacketAsHost(PacketType type, byte[] buffer, int length,
                                 IPEndPoint remote)
        {
            switch (type)
            {
                case PacketType.DiscoveryRequest:
                    SendTo(remote, EncodePacket(PacketType.DiscoveryReply, writer =>
                        WriteSessionInfo(writer)));
                    break;

                case PacketType.Ping:
                    SendTo(remote, EncodePacket(PacketType.Pong, null));
                    break;

                case PacketType.JoinRequest:
                    HandleJoinRequest(remote, buffer);
                    break;

                case PacketType.Ready:
                {
                    byte id = buffer[1];
                    bool isReady = buffer[2] != 0;

                    NetworkGamer gamer = allGamers.FindById(id);

                    if (gamer != null && !gamer.IsLocal)
                    {
                        gamer.IsReady = isReady;
                        MarkClientSeen(remote);
                    }
                    break;
                }

                case PacketType.Data:
                {
                    NetworkGamer sender = allGamers.FindById(buffer[1]);

                    if (sender != null && !sender.IsLocal)
                    {
                        //Queue the packet for local reading.
                        QueueIncoming(sender, buffer, 2, length);

                        //Relay to all other clients.
                        byte[] payload = new byte[length - 2];
                        Array.Copy(buffer, 2, payload, 0, payload.Length);

                        byte[] relay = EncodePacket(PacketType.Data, writer =>
                            writer.Write(sender.Id));

                        byte[] relayed = Combine(relay, payload);

                        foreach (IPEndPoint endpoint in clientEndpoints)
                        {
                            if (!endpoint.Equals(remote))
                            {
                                SendTo(endpoint, relayed);
                            }
                        }

                        MarkClientSeen(remote);
                    }
                    break;
                }

                case PacketType.Leave:
                {
                    byte id = buffer[1];
                    NetworkGamer gamer = allGamers.FindById(id);

                    if (gamer != null)
                    {
                        RemoveGamer(gamer);
                    }
                    break;
                }
            }
        }

        void HandleJoinRequest(IPEndPoint remote, byte[] buffer)
        {
            string gamertag = DecodeString(buffer, 1, out _);

            bool joinable = (state == NetworkSessionState.Lobby) || AllowJoinInProgress;

            if (!joinable)
            {
                SendTo(remote, EncodePacket(PacketType.JoinReplyFail, writer =>
                    writer.Write((byte)1)));
                return;
            }

            if (allGamers.Count >= MaxGamersLimit)
            {
                SendTo(remote, EncodePacket(PacketType.JoinReplyFail, writer =>
                    writer.Write((byte)0)));
                return;
            }

            //Assign a free gamer id and remember the client endpoint.
            byte newId = FindFreeGamerId();

            NetworkGamer gamer = new NetworkGamer(newId, gamertag, false);

            clientEndpoints.Add(remote);
            clientSeen[remote] = DateTime.UtcNow;
            allGamers.Add(gamer);

            //Reply with the assigned id, current state, and roster.
            SendTo(remote, EncodePacket(PacketType.JoinReplyOk, writer =>
            {
                writer.Write(newId);
                writer.Write((byte)state);
                writer.Write((byte)state);
                WriteRoster(writer);
            }));

            //Let everyone know about the new player.
            BroadcastRoster();
        }

        byte FindFreeGamerId()
        {
            for (byte id = 0; id < MaxGamersLimit; id++)
            {
                if (allGamers.FindById(id) == null)
                {
                    return id;
                }
            }
            return MaxGamersLimit;
        }

        #endregion

        #region Client Logic

        void UpdateAsClient()
        {
            DateTime now = DateTime.UtcNow;

            //Heartbeat: tell the host we are alive, and our ready state.
            if ((now - lastRoster).TotalMilliseconds >= RosterInterval)
            {
                lastRoster = now;

                if (localGamers.Count > 0)
                {
                    LocalNetworkGamer local = localGamers[0];

                    SendToHost(EncodePacket(PacketType.Ready, writer =>
                    {
                        writer.Write(local.Id);
                        writer.Write(local.IsReady);
                    }));
                }
            }

            //Detect a lost host.
            if (now - hostSeen > TimeSpan.FromMilliseconds(HostTimeout))
            {
                EndSession(NetworkSessionEndReason.Disconnected);
            }
        }

        void ProcessPacketAsClient(PacketType type, byte[] buffer, int length,
                                   IPEndPoint remote)
        {
            //Only trust packets from the host we joined.
            if (hostEndPoint != null && !hostEndPoint.Equals(remote))
            {
                return;
            }

            switch (type)
            {
                case PacketType.Roster:
                    ReadRoster(buffer);
                    hostSeen = DateTime.UtcNow;
                    break;

                case PacketType.Data:
                {
                    NetworkGamer sender = allGamers.FindById(buffer[1]);

                    if (sender != null)
                    {
                        QueueIncoming(sender, buffer, 2, length);
                    }
                    hostSeen = DateTime.UtcNow;
                    break;
                }

                case PacketType.StartGame:
                    state = NetworkSessionState.Playing;
                    hostSeen = DateTime.UtcNow;
                    break;

                case PacketType.EndGame:
                    state = NetworkSessionState.Lobby;
                    hostSeen = DateTime.UtcNow;
                    break;

                case PacketType.Kick:
                    EndSession(NetworkSessionEndReason.HostEndedSession);
                    break;

                case PacketType.Ping:
                    SendToHost(EncodePacket(PacketType.Pong, null));
                    break;
            }
        }

        #endregion

        #region Roster

        void BroadcastRoster()
        {
            Broadcast(EncodePacket(PacketType.Roster, writer =>
            {
                writer.Write((byte)state);
                WriteRoster(writer);
            }));
        }

        void WriteRoster(BinaryPayload writer)
        {
            writer.Write((byte)allGamers.Count);

            foreach (NetworkGamer gamer in allGamers)
            {
                writer.Write(gamer.Id);
                writer.Write(gamer.Gamertag);
                writer.Write(gamer.IsHost);
                writer.Write(gamer.IsReady);
            }
        }

        ///<summary>
        ///Reads a roster packet. Layout: [0] type, [1] state,
        ///[2] gamer count, then per gamer: id, gamertag, isHost, isReady.
        ///</summary>
        void ReadRoster(byte[] buffer)
        {
            int offset = 1;

            NetworkSessionState hostState = (NetworkSessionState)buffer[offset++];

            byte count = buffer[offset++];

            HashSet<byte> seen = new HashSet<byte>();
            List<NetworkGamer> joined = new List<NetworkGamer>();

            for (int i = 0; i < count; i++)
            {
                byte id = buffer[offset++];
                string gamertag = DecodeString(buffer, offset, out int consumed);
                offset += consumed;

                bool isHost = buffer[offset++] != 0;
                bool isReady = buffer[offset++] != 0;

                seen.Add(id);

                NetworkGamer gamer = allGamers.FindById(id);

                if (gamer == null)
                {
                    //A gamer we have not yet seen: it joined.
                    if (pendingJoin && id == pendingLocalId)
                    {
                        pendingJoin = false;

                        LocalNetworkGamer local = localGamers[0];
                        local.Id = id;
                        local.IsHost = isHost;
                        local.IsReady = isReady;

                        gamer = local;
                    }
                    else
                    {
                        gamer = new NetworkGamer(id, gamertag, isHost)
                        {
                            IsReady = isReady,
                        };

                        gamer.signedInGamer = new SignedInGamer(gamertag, id);
                    }

                    allGamers.Add(gamer);
                    joined.Add(gamer);
                }
                else
                {
                    gamer.IsHost = isHost;
                    gamer.IsReady = isReady;
                }
            }

            //Anyone not in the roster has left.
            List<NetworkGamer> left = new List<NetworkGamer>();

            foreach (NetworkGamer gamer in allGamers)
            {
                if (!seen.Contains(gamer.Id))
                {
                    left.Add(gamer);
                }
            }

            foreach (NetworkGamer gamer in left)
            {
                allGamers.Remove(gamer);
                gamer.IsDisposed = true;

                if (gamer.IsLocal)
                {
                    //We have been removed from the session.
                    EndSession(NetworkSessionEndReason.RemovedByHost);
                }
                else if (GamerLeft != null)
                {
                    GamerLeft(this, new GamerLeftEventArgs { Gamer = gamer });
                }
            }

            //Fire join events after the removals.
            foreach (NetworkGamer gamer in joined)
            {
                if (GamerJoined != null)
                {
                    GamerJoined(this, new GamerJoinedEventArgs { Gamer = gamer });
                }
            }

            //Follow the host session state.
            if (state != NetworkSessionState.Ended)
            {
                state = hostState;
            }
        }

        ///<summary>
        ///Reads a join reply packet. Layout: [0] type, [1] assigned id,
        ///[2] state, [3] gamer count, then the roster entries.
        ///</summary>
        void ReadJoinReplyOk(byte[] buffer)
        {
            pendingLocalId = buffer[1];

            NetworkSessionState hostState = (NetworkSessionState)buffer[2];

            //Create the local gamer with a placeholder id; the roster read
            //below corrects it to the id assigned by the host.
            LocalNetworkGamer local = new LocalNetworkGamer(0, LocalGamertag, false);
            localGamers.Add(local);

            pendingJoin = true;

            //Reuse the roster reader: rebuild a buffer in roster layout
            //([0] type, [1] state, [2] count, entries...).
            byte[] roster = new byte[buffer.Length - 1];
            roster[0] = (byte)PacketType.Roster;
            roster[1] = buffer[2];
            Array.Copy(buffer, 3, roster, 2, buffer.Length - 3);

            ReadRoster(roster);

            state = hostState;
        }

        #endregion

        #region Data Sending and Receiving

        ///<summary>
        ///Sends a gameplay packet on behalf of the local gamer.
        ///</summary>
        internal void SendDataInternal(LocalNetworkGamer gamer, byte[] payload)
        {
            if (socket == null)
            {
                return;
            }

            byte[] packet = EncodePacket(PacketType.Data, writer =>
                writer.Write(gamer.Id));

            byte[] datagram = Combine(packet, payload);

            if (isHost)
            {
                //The host sends to every client.
                foreach (IPEndPoint endpoint in clientEndpoints)
                {
                    SendTo(endpoint, datagram);
                }
            }
            else
            {
                //Clients send everything to the host, which relays it.
                SendToHost(datagram);
            }
        }

        void QueueIncoming(NetworkGamer sender, byte[] buffer, int offset, int length)
        {
            byte[] payload = new byte[length - offset];
            Array.Copy(buffer, offset, payload, 0, payload.Length);

            foreach (LocalNetworkGamer local in localGamers)
            {
                local.incomingPackets.Enqueue(new LocalNetworkGamer.IncomingPacket
                {
                    Sender = sender,
                    Payload = payload,
                });
            }
        }

        #endregion

        #region Gamer Management

        void RemoveGamer(NetworkGamer gamer)
        {
            allGamers.Remove(gamer);
            gamer.IsDisposed = true;

            IPEndPoint endpoint = FindEndpointByGamer(gamer);

            if (endpoint != null)
            {
                clientEndpoints.Remove(endpoint);
                clientSeen.Remove(endpoint);
            }

            if (GamerLeft != null)
            {
                GamerLeft(this, new GamerLeftEventArgs { Gamer = gamer });
            }
        }

        NetworkGamer FindGamerByEndpoint(IPEndPoint endpoint)
        {
            int index = clientEndpoints.IndexOf(endpoint);

            if (index < 0)
            {
                return null;
            }

            int remoteCount = 0;

            foreach (NetworkGamer gamer in allGamers)
            {
                if (!gamer.IsLocal)
                {
                    if (remoteCount == index)
                    {
                        return gamer;
                    }
                    remoteCount++;
                }
            }
            return null;
        }

        IPEndPoint FindEndpointByGamer(NetworkGamer target)
        {
            int index = 0;

            foreach (NetworkGamer gamer in allGamers)
            {
                if (gamer.IsLocal)
                {
                    continue;
                }

                if (gamer == target)
                {
                    return clientEndpoints[index];
                }
                index++;
            }
            return null;
        }

        ///<summary>Pastel palette used for gamer profile pictures.</summary>
        internal static Color GamerColor(byte id)
        {
            switch (id % 6)
            {
                case 0: return new Color((byte)80, (byte)160, (byte)255);
                case 1: return new Color((byte)255, (byte)120, (byte)80);
                case 2: return new Color((byte)120, (byte)220, (byte)100);
                case 3: return new Color((byte)255, (byte)200, (byte)70);
                case 4: return new Color((byte)200, (byte)120, (byte)255);
                default: return new Color((byte)100, (byte)220, (byte)220);
            }
        }

        #endregion

        #region Socket Helpers

        void SendToHost(byte[] datagram)
        {
            if (hostEndPoint != null)
            {
                SendTo(hostEndPoint, datagram);
            }
        }

        void SendTo(EndPoint remote, byte[] datagram)
        {
            if (socket != null)
            {
                socket.SendTo(datagram, remote);
            }
        }

        void Broadcast(byte[] datagram)
        {
            if (!isHost)
            {
                return;
            }

            foreach (IPEndPoint endpoint in clientEndpoints)
            {
                SendTo(endpoint, datagram);
            }
        }

        void MarkClientSeen(IPEndPoint endpoint)
        {
            clientSeen[endpoint] = DateTime.UtcNow;
        }

        #endregion

        #region Packet Encoding

        delegate void PayloadWriter(BinaryPayload writer);

        ///<summary>Minimal binary writer used to encode packets.</summary>
        class BinaryPayload : IDisposable
        {
            readonly MemoryStream stream = new MemoryStream();
            readonly BinaryWriter writer;

            public BinaryPayload()
            {
                writer = new BinaryWriter(stream);
            }

            public void Write(byte value) { writer.Write(value); }
            public void Write(bool value) { writer.Write(value); }
            public void Write(string value) { writer.Write(value); }

            public byte[] ToArray()
            {
                writer.Flush();
                return stream.ToArray();
            }

            public void Dispose() { writer.Dispose(); }
        }

        static byte[] EncodePacket(PacketType type, PayloadWriter payload)
        {
            using (BinaryPayload writer = new BinaryPayload())
            {
                writer.Write((byte)type);

                if (payload != null)
                {
                    payload(writer);
                }

                return writer.ToArray();
            }
        }

        static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Array.Copy(first, 0, result, 0, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }

        void WriteSessionInfo(BinaryPayload writer)
        {
            writer.Write(LocalGamertag);
            writer.Write((byte)allGamers.Count);
            writer.Write((byte)(MaxGamersLimit - allGamers.Count));
            writer.Write((byte)state);
        }

        static AvailableNetworkSession ParseDiscoveryReply(byte[] buffer, int length)
        {
            try
            {
                string hostGamertag = DecodeString(buffer, 1, out int stringLength);
                int offset = 1 + stringLength;

                AvailableNetworkSession session = new AvailableNetworkSession
                {
                    HostGamertag = hostGamertag,
                    CurrentGamerCount = buffer[offset],
                    OpenPublicGamerSlots = buffer[offset + 1],
                    QualityOfService = QualityOfService.Empty,
                };

                return session;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        ///<summary>
        ///Decodes a string written by BinaryWriter: a 7-bit encoded length
        ///prefix followed by UTF-8 bytes. Outputs the total bytes consumed.
        ///</summary>
        static string DecodeString(byte[] buffer, int offset, out int length)
        {
            int position = offset;
            int stringLength = 0;
            int shift = 0;

            while (true)
            {
                byte next = buffer[position++];
                stringLength |= (next & 0x7F) << shift;
                if ((next & 0x80) == 0)
                {
                    break;
                }
                shift += 7;
            }

            length = position - offset + stringLength;

            return Encoding.UTF8.GetString(buffer, position, stringLength);
        }

        #endregion
    }
}
