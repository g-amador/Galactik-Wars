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
using GalactikWars.Net;
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
            MenuEntry multiplayerMenuEntry = new MenuEntry(Resources.Multiplayer);
            MenuEntry optionsMenuEntry = new MenuEntry(Resources.Options);
            MenuEntry creditsMenuEntry = new MenuEntry(Resources.Credits);
            MenuEntry exitMenuEntry = new MenuEntry(Resources.Exit);

            //Hook up menu event handlers.
            singlePlayerMenuEntry.Selected += SinglePlayerMenuEntrySelected;
            multiplayerMenuEntry.Selected += MultiplayerMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenu;
            creditsMenuEntry.Selected += CreditsMenu;
            exitMenuEntry.Selected += OnCancel;

            //Add entries to the menu.
            MenuEntries.Add(singlePlayerMenuEntry);
            MenuEntries.Add(multiplayerMenuEntry);
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
        ///Event handler for when the Multiplayer menu entry is selected.
        ///</summary>
        void MultiplayerMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(
                new CreateOrFindSessionScreen(NetworkSessionType.PlayerMatch));
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
