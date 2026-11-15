#region File Description
//-----------------------------------------------------------------------------
//Exceptions.cs
//
//Galactik Wars LAN networking layer.
//Exception types mirroring the XNA networking exceptions.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace GalactikWars.Net
{
    ///<summary>
    ///Generic networking exception, raised when a network operation fails.
    ///</summary>
    public class NetworkException : Exception
    {
        public NetworkException()
        { }

        public NetworkException(string message)
            : base(message)
        { }

        public NetworkException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    ///<summary>
    ///Exception raised when joining a network session fails.
    ///</summary>
    public class NetworkSessionJoinException : NetworkException
    {
        #region Properties

        ///<summary>
        ///Gets the reason the join attempt failed.
        ///</summary>
        public NetworkSessionJoinError JoinError
        {
            get { return joinError; }
        }

        private readonly NetworkSessionJoinError joinError;

        #endregion

        #region Initialization

        ///<summary>
        ///Constructs a join exception for the specified failure reason.
        ///</summary>
        public NetworkSessionJoinException(NetworkSessionJoinError joinError)
            : base(GetErrorMessage(joinError))
        {
            this.joinError = joinError;
        }

        private static string GetErrorMessage(NetworkSessionJoinError joinError)
        {
            return joinError switch
            {
                NetworkSessionJoinError.SessionFull => "The session is full.",
                NetworkSessionJoinError.SessionNotFound => "The session could not be found.",
                NetworkSessionJoinError.SessionNotJoinable => "The session is not joinable.",
                _ => "The session could not be joined.",
            };
        }

        #endregion
    }
}
