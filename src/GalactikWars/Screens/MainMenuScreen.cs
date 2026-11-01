#region File Description
//-----------------------------------------------------------------------------
//MainMenuScreen.cs
//
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Net;
#endregion

namespace GalactikWars
{
    ///<summary>
    ///The main menu screen is the first thing displayed when the game starts up.
    ///</summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization


        ///<summary>
        ///Constructor fills in the menu contents.
        ///</summary>
        public MainMenuScreen()
            : base(Resources.MainMenu)
        {
            //Create our menu entries.
            MenuEntry singlePlayerMenuEntry = new MenuEntry(Resources.SinglePlayer);
            MenuEntry liveMenuEntry = new MenuEntry(Resources.PlayerMatch);
            MenuEntry systemLinkMenuEntry = new MenuEntry(Resources.SystemLink);
            MenuEntry optionsMenuEntry = new MenuEntry(Resources.Options);
            MenuEntry creditsMenuEntry = new MenuEntry(Resources.Credits);
            MenuEntry exitMenuEntry = new MenuEntry(Resources.Exit);

            //Hook up menu event handlers.
            singlePlayerMenuEntry.Selected += SinglePlayerMenuEntrySelected;
            liveMenuEntry.Selected += LiveMenuEntrySelected;
            systemLinkMenuEntry.Selected += SystemLinkMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenu;
            creditsMenuEntry.Selected += CreditsMenu;
            exitMenuEntry.Selected += OnCancel;

            //Add entries to the menu.
            MenuEntries.Add(singlePlayerMenuEntry);
            MenuEntries.Add(liveMenuEntry);
            MenuEntries.Add(systemLinkMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(creditsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }


        #endregion

        #region Handle Input


        ///<summary>
        ///Event handler for when the Single Player menu entry is selected.
        ///</summary>
        void SinglePlayerMenuEntrySelected(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new GameplayScreenSinglePlayer());
        }


        ///<summary>
        ///Event handler for when the Live menu entry is selected.
        ///</summary>
        void LiveMenuEntrySelected(object sender, EventArgs e)
        {
            CreateOrFindSession(NetworkSessionType.PlayerMatch);
        }


        ///<summary>
        ///Event handler for when the System Link menu entry is selected.
        ///</summary>
        void SystemLinkMenuEntrySelected(object sender, EventArgs e)
        {
            CreateOrFindSession(NetworkSessionType.SystemLink);
        }

        ///<summary>
        ///Event handler for options
        ///</summary>
        void OptionsMenu(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new OptionsMenuScreen(
                Resources.Options));
        }

        ///<summary>
        ///Event handler for credits
        ///</summary>
        void CreditsMenu(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new CreditsScreen(null));
        }


        ///<summary>
        ///Helper method shared by the Live and System Link menu event handlers.
        ///</summary>
        void CreateOrFindSession(NetworkSessionType sessionType)
        {
            //First, we need to make sure a suitable gamer profile is signed in.
            ProfileSignInScreen profileSignIn = new ProfileSignInScreen(sessionType);

            //Hook up an event so once the ProfileSignInScreen is happy,
            //it will activate the CreateOrFindSessionScreen.
            profileSignIn.ProfileSignedIn += delegate
            {
                ScreenManager.AddScreen(new CreateOrFindSessionScreen(sessionType));
            };

            //Activate the ProfileSignInScreen.
            ScreenManager.AddScreen(profileSignIn);
        }


        ///<summary>
        ///When the user cancels the main menu, ask if they want to exit the sample.
        ///</summary>
        protected override void OnCancel()
        {
            MessageBoxScreen confirmExitMessageBox =
                                    new MessageBoxScreen(Resources.ConfirmExitSample);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox);
        }


        ///<summary>
        ///Event handler for when the user selects ok on the "are you sure
        ///you want to exit" message box.
        ///</summary>
        void ConfirmExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        #endregion
    }
}
