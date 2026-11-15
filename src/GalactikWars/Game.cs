#region File Description
//-----------------------------------------------------------------------------
//Game.cs
//
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GalactikWars.ScreenManager;
using GalactikWars.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace GalactikWars
{

    ///<summary>
    ///Sample showing how to manage the different game states involved in
    ///implementing a networked game, with menus for creating, searching,
    ///and joining sessions, a lobby screen, and the game itself. This main
    ///game class is extremely simple: all the interesting stuff happens
    ///in the ScreenManager component.
    ///</summary>
    public class NetworkStateManagementGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        private readonly GraphicsDeviceManager graphics;
        private readonly GalactikWars.ScreenManager.ScreenManager screenManager;

        /// <summary>
        /// Keyboard state from the previous frame, used to detect F11 presses.
        /// </summary>
        private KeyboardState previousKeyboard;

        #endregion

        #region Initialization


        ///<summary>
        ///The main game constructor.
        ///</summary>
        public NetworkStateManagementGame()
        {
            Content.RootDirectory = "assets";

            graphics = new GraphicsDeviceManager(this)
            {
                //A default window large enough to show every control screen entry,
                //and resizable (with maximize button) so users can adjust it.
                PreferredBackBufferWidth = GameVariables.ScreenWidth,
                PreferredBackBufferHeight = GameVariables.ScreenHeight,
                IsFullScreen = GameVariables.FullScreen
            };
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

            this.IsMouseVisible = GameVariables.MouseVisible;

            //Create components.
            screenManager = new GalactikWars.ScreenManager.ScreenManager(this);

            Components.Add(screenManager);
            Components.Add(new MessageDisplayComponent(this));

            //Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());
        }

        #endregion

        #region Update


        ///<summary>
        ///Handles the global fullscreen hotkey (F11). Everything else is
        ///delegated to the ScreenManager component.
        ///</summary>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.F11) &&
                previousKeyboard.IsKeyUp(Keys.F11))
            {
                graphics.ToggleFullScreen();
            }

            previousKeyboard = keyboard;

            base.Update(gameTime);
        }


        #endregion

        #region Draw


        ///<summary>
        ///This is called when the game should draw itself.
        ///</summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            //The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }


        #endregion
    }


    #region Entry Point

    ///<summary>
    ///The main entry point for the application.
    ///</summary>
    internal static class Program
    {
        private static void Main()
        {
            using (NetworkStateManagementGame game = new())
            {
                game.Run();
            }
        }
    }

    #endregion
}
