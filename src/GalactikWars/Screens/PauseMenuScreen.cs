#region File Description
//-----------------------------------------------------------------------------
//PauseMenuScreen.cs
//
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GalactikWars
{
    ///<summary>
    ///The pause menu comes up over the top of the game,
    ///giving the player options to resume or quit.
    ///</summary>
    ///
    class PauseMenuScreen : MenuScreen
    {
        #region Fields

        NetworkSession networkSession;

        string message = "Music Volume: ";
        string message2 = "                         Effects Volume: ";
        float music = SoundHandler.musicVolume * 100, 
            effects = SoundHandler.effectsVolume * 100;
        
        #endregion

        #region Initialization

        ///<summary>
        ///Constructor.
        ///</summary>
        public PauseMenuScreen(NetworkSession networkSession)
            : base(Resources.Paused)
        {
            this.networkSession = networkSession;

            //Flag that there is no need for the game to transition
            //off when the pause menu is on top of it.
            IsPopup = true;

            //sound
            MenuEntry MusicUpMenuEntry = new MenuEntry(Resources.MusicUp);
            MenuEntry MusicDownMenuEntry = new MenuEntry(Resources.MusicDown);
            MenuEntry effectsUpMenuEntry = new MenuEntry(Resources.EffectsUp);
            MenuEntry effectsDownMenuEntry = new MenuEntry(Resources.EffectsDown);

            MusicUpMenuEntry.Selected += MusicUpMenuEntrySelected;
            MusicDownMenuEntry.Selected += MusicDownMenuEntrySelected;
            effectsUpMenuEntry.Selected += effectsUpMenuEntrySelected;
            effectsDownMenuEntry.Selected += effectsDownMenuEntrySelected;

            MenuEntries.Add(MusicUpMenuEntry);
            MenuEntries.Add(MusicDownMenuEntry);
            MenuEntries.Add(effectsUpMenuEntry);
            MenuEntries.Add(effectsDownMenuEntry);

            //Add the Resume Game menu entry.
            MenuEntry resumeGameMenuEntry = new MenuEntry(Resources.ResumeGame);
            resumeGameMenuEntry.Selected += OnCancel;
            MenuEntries.Add(resumeGameMenuEntry);

            if (networkSession == null)
            {
                //If this is a single player game, add the Quit menu entry.
                MenuEntry quitGameMenuEntry = new MenuEntry(Resources.QuitGame);
                quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;
                MenuEntries.Add(quitGameMenuEntry);
            }
            else
            {
                //If we are hosting a network game, add the Return to Lobby menu entry.
                if (networkSession.IsHost)
                {
                    MenuEntry lobbyMenuEntry = new MenuEntry(Resources.ReturnToLobby);
                    lobbyMenuEntry.Selected += ReturnToLobbyMenuEntrySelected;
                    MenuEntries.Add(lobbyMenuEntry);
                }

                //Add the End/Leave Session menu entry.
                string leaveEntryText = networkSession.IsHost ? Resources.EndSession :
                                                                Resources.LeaveSession;

                MenuEntry leaveSessionMenuEntry = new MenuEntry(leaveEntryText);
                leaveSessionMenuEntry.Selected += LeaveSessionMenuEntrySelected;
                MenuEntries.Add(leaveSessionMenuEntry);
            }
        }

        #endregion

        #region Sound Menu Screen

        public static float PauseMenuScreen_music(float music)
        {
            music = SoundHandler.musicVolume;
            return (music);
        }


        public static float PauseMenuScreen_effect(float effect)
        {

            effect = SoundHandler.effectsVolume;
            return (effect);
        }

        #endregion

        #region Handle Input

        public void MusicUpMenuEntrySelected(object sender, EventArgs e)
        {
            music = ((music/100) + 0.10f)*100;

            if (music > 100)
            {
                music = 100;
            }

            SoundHandler.musicVolume = music / 100;
        }

        public void MusicDownMenuEntrySelected(object sender, EventArgs e)
        {
            music = ((music/100) - 0.10f)*100;

            if (music <= 0)
            {
                music = 0;
            }

            SoundHandler.musicVolume = music / 100;
        }

        public void effectsUpMenuEntrySelected(object sender, EventArgs e)
        {
            effects = ((effects / 100) + 0.10f)*100;

            if (effects > 100)
            {
                effects = 100;
            }

            SoundHandler.effectsVolume = effects / 100; 
        }

        public void effectsDownMenuEntrySelected(object sender, EventArgs e)
        {
            effects = ((effects / 100) - 0.10f) * 100;

            if (effects <= 0)
            {
                effects = 0;
            }

            SoundHandler.effectsVolume = effects / 100;
        }

        ///<summary>
        ///Event handler for when the Quit Game menu entry is selected.
        ///</summary>
        void QuitGameMenuEntrySelected(object sender, EventArgs e)
        {
            MessageBoxScreen confirmQuitMessageBox =
                                    new MessageBoxScreen(Resources.ConfirmQuitGame);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox);
        }

        ///<summary>
        ///Event handler for when the user selects ok on the "are you sure
        ///you want to quit" message box. This uses the loading screen to
        ///transition from the game back to the main menu screen.
        ///</summary>
        void ConfirmQuitMessageBoxAccepted(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(),
                                                     new MainMenuScreen());
        }

        ///<summary>
        ///Event handler for when the Return to Lobby menu entry is selected.
        ///</summary>
        void ReturnToLobbyMenuEntrySelected(object sender, EventArgs e)
        {
            if (networkSession.SessionState == NetworkSessionState.Playing)
            {
                networkSession.EndGame();
            }
        }

        ///<summary>
        ///Event handler for when the End/Leave Session menu entry is selected.
        ///</summary>
        void LeaveSessionMenuEntrySelected(object sender, EventArgs e)
        {
            NetworkSessionComponent.LeaveSession(ScreenManager);
        }

        #endregion

        #region Draw

        ///<summary>
        ///Draws the pause menu screen. This darkens down the gameplay screen
        ///that is underneath us, and then chains to the base MenuScreen.Draw.
        ///</summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;
            Vector2 textPosition = new Vector2(100, 90);
            Vector2 textPosition2 = new Vector2(100, 90);
            Vector2 position = new Vector2(100, 150);

            //Make the menu slide into place during transitions, using a
            //power curve to make things look more interesting (this makes
            //the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            //Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, TransitionAlpha);

            spriteBatch.Begin();

            //Draw the message box text.
            spriteBatch.DrawString(font, message + music + " %", textPosition, color);

            //Draw the message box text.
            spriteBatch.DrawString(font, message2 + effects + " %", textPosition2, color);
           
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion
    }
}
