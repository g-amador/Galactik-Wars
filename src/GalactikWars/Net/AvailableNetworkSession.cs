#region File Description
//-----------------------------------------------------------------------------
//AvailableNetworkSession.cs
//
//Galactik Wars LAN networking layer.
//Session discovery results mirroring the XNA AvailableNetworkSession types.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Net;
#endregion

namespace GalactikWars.Net
{
    ///<summary>
    ///Network quality of service information. The XNA version measured this
    ///against Xbox LIVE; the LAN version measures the ping time to the host.
    ///</summary>
    public class QualityOfService
    {
        #region Properties

        ///<summary>
        ///Gets whether quality of service information is available.
        ///</summary>
        public bool IsAvailable { get; internal set; }

        ///<summary>
        ///Gets the average round trip time to the session host.
        ///</summary>
        public TimeSpan AverageRoundtripTime { get; internal set; }

        #endregion

        #region Initialization

        public static readonly QualityOfService Empty =
            new QualityOfService();

        #endregion
    }

    ///<summary>
    ///Describes an available network session found on the local network.
    ///</summary>
    public class AvailableNetworkSession
    {
        #region Properties

        ///<summary>
        ///Gets the gamertag of the session host.
        ///</summary>
        public string HostGamertag { get; internal set; }

        ///<summary>
        ///Gets the number of players currently in the session.
        ///</summary>
        public int CurrentGamerCount { get; internal set; }

        ///<summary>
        ///Gets how many more players may join the session.
        ///</summary>
        public int OpenPublicGamerSlots { get; internal set; }

        ///<summary>
        ///Gets quality of service information for the session.
        ///</summary>
        public QualityOfService QualityOfService { get; internal set; }

        ///<summary>
        ///Endpoint of the session host, used internally to join.
        ///</summary>
        internal IPEndPoint HostEndPoint { get; set; }

        #endregion
    }

    ///<summary>
    ///Collection of the network sessions found on the local network.
    ///</summary>
    public class AvailableNetworkSessionCollection : IReadOnlyList<AvailableNetworkSession>, IDisposable
    {
        readonly List<AvailableNetworkSession> sessions =
            new List<AvailableNetworkSession>();

        public int Count
        {
            get { return sessions.Count; }
        }

        public AvailableNetworkSession this[int index]
        {
            get { return sessions[index]; }
        }

        internal void Add(AvailableNetworkSession session)
        {
            sessions.Add(session);
        }

        IEnumerator<AvailableNetworkSession> IEnumerable<AvailableNetworkSession>.GetEnumerator()
        {
            return sessions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return sessions.GetEnumerator();
        }

        ///<summary>
        ///Releases the search results. Nothing to do in the LAN implementation.
        ///</summary>
        public void Dispose()
        { }
    }
}
