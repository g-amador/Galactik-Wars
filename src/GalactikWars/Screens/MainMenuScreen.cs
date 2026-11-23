#region File Description
//-----------------------------------------------------------------------------
//MainMenuScreen.cs
//
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GalactikWars.Handlers;
using GalactikWars.Net;
using GalactikWars.Networking;
using Microsoft.Xna.Framework;
using System;
#endregion

namespace GalactikWars.Screens
{
    ///<summary>
    ///The main menu screen is the first thing displayed when the game starts up.
    ///</summary>
    internal class MainMenuScreen : MenuScreen
    {
        #region Initialization


        ///<summary>
        ///Constructor fills in the menu contents.
        ///</summary>
        public MainMenuScreen()
            : base(Resources.MainMenu)
        {
            //Create our menu entries.
            MenuEntry singlePlayerMenuEntry = new(Resources.SinglePlayer);
            MenuEntry multiplayerMenuEntry = new(Resources.Multiplayer);
            MenuEntry optionsMenuEntry = new(Resources.Options);
            MenuEntry creditsMenuEntry = new(Resources.Credits);
            MenuEntry exitMenuEntry = new(Resources.Exit);

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


        ///<summary>
        ///Makes sure the menu track is playing whenever the main menu is
        ///visible (e.g. returning here from the credits screen).
        ///</summary>
        public override void Update(GameTime gameTime,
                                    bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            ScreenManager.SoundHandler.PlayTrack(SoundHandler.MenuTrack);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        #endregion

        #region Handle Input


        ///<summary>
        ///Event handler for when the Single Player menu entry is selected.
        ///</summary>
        private void SinglePlayerMenuEntrySelected(object sender, EventArgs e)
        {
            //pick the escort first; it grants the pilot's loadout advantage
            ScreenManager.AddScreen(new EscortSelectionScreen());
        }


        ///<summary>
        ///Event handler for when the Multiplayer menu entry is selected.
        ///</summary>
        private void MultiplayerMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(
                new CreateOrFindSessionScreen(NetworkSessionType.PlayerMatch));
        }

        ///<summary>
        ///Event handler for options
        ///</summary>
        private void OptionsMenu(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new OptionsMenuScreen(
                Resources.Options));
        }

        ///<summary>
        ///Event handler for credits
        ///</summary>
        private void CreditsMenu(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new CreditsScreen(null));
        }


        ///<summary>
        ///When the user cancels the main menu, ask if they want to exit the sample.
        ///</summary>
        protected override void OnCancel()
        {
            MessageBoxScreen confirmExitMessageBox =
                                    new(Resources.ConfirmExitSample);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox);
        }


        ///<summary>
        ///Event handler for when the user selects ok on the "are you sure
        ///you want to exit" message box.
        ///</summary>
        private void ConfirmExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        #endregion
    }
}
