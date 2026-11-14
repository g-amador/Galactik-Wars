#region File Description
//-----------------------------------------------------------------------------
//Enums.cs
//
//Galactik Wars LAN networking layer.
//Replaces the XNA Game Studio 2.0 "Games for Windows - LIVE" networking
//enums (Microsoft.Xna.Framework.Net) with LAN equivalents.
//-----------------------------------------------------------------------------
#endregion

namespace GalactikWars.Net
{
    ///<summary>
    ///Type of network session. In the XNA version, PlayerMatch was an
    ///Xbox LIVE online match. In this LAN implementation both values map to
    ///the same system-link style LAN session; the distinction is kept only
    ///so the original menu structure remains intact.
    ///</summary>
    public enum NetworkSessionType
    {
        PlayerMatch,
        SystemLink,
    }

    ///<summary>
    ///Current state of a network session.
    ///</summary>
    public enum NetworkSessionState
    {
        Lobby,
        Playing,
        Ended,
    }

    ///<summary>
    ///Reason a network session ended.
    ///</summary>
    public enum NetworkSessionEndReason
    {
        ClientSignedOut,
        HostEndedSession,
        RemovedByHost,
        Disconnected,
    }

    ///<summary>
    ///Requested delivery options for network data. The LAN implementation
    ///sends datagrams over UDP, so all options are effectively unreliable,
    ///but the gameplay state is refreshed every frame which makes loss
    ///self-correcting.
    ///</summary>
    public enum SendDataOptions
    {
        None,
        Reliable,
        ReliableInOrder,
    }

    ///<summary>
    ///Reasons a join attempt can fail.
    ///</summary>
    public enum NetworkSessionJoinError
    {
        SessionFull,
        SessionNotFound,
        SessionNotJoinable,
    }
}
