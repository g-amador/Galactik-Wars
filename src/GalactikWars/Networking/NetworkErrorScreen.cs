#region File Description
//-----------------------------------------------------------------------------
//NetworkErrorScreen.cs
//
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using GalactikWars.Net;
#endregion

namespace GalactikWars
{
    ///<summary>
    ///Specialized message box subclass, used to display network error messages.
    ///</summary>
    class NetworkErrorScreen : MessageBoxScreen
    {
        #region Initialization


        ///<summary>
        ///Constructs an error message box from the specified exception.
        ///</summary>
        public NetworkErrorScreen(Exception exception)
            : base(GetErrorMessage(exception), false)
        { }


        ///<summary>
        ///Converts a network exception into a user friendly error message.
        ///</summary>
        static string GetErrorMessage(Exception exception)
        {
            Trace.WriteLine(string.Format("Network operation threw {0}: {1}",
                                          exception, exception.Message));

            //Is it a NetworkSessionJoinException?
            NetworkSessionJoinException joinException = exception as
                                                            NetworkSessionJoinException;

            if (joinException != null)
            {
                switch (joinException.JoinError)
                {
                    case NetworkSessionJoinError.SessionFull:
                        return Resources.ErrorSessionFull;

                    case NetworkSessionJoinError.SessionNotFound:
                        return Resources.ErrorSessionNotFound;

                    case NetworkSessionJoinError.SessionNotJoinable:
                        return Resources.ErrorSessionNotJoinable;
                }
            }

            //Otherwise just a generic error message.
            return Resources.ErrorNetwork;
        }


        #endregion
    }
}
