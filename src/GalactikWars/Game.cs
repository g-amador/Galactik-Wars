#region File Description
//-----------------------------------------------------------------------------
//Game.cs
//
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
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
        
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        #endregion

        #region Initialization


        ///<summary>
        ///The main game constructor.
        ///</summary>
        public NetworkStateManagementGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
            graphics.PreferredBackBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
            graphics.IsFullScreen = GameVariables.FullScreen;
            graphics.ApplyChanges();

            this.IsMouseVisible = GameVariables.MouseVisible;

            //Create components.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);
            Components.Add(new MessageDisplayComponent(this));
            Components.Add(new GamerServicesComponent(this));

            //Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());            
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
    static class Program
    {
        static void Main()
        {
            using (NetworkStateManagementGame game = new NetworkStateManagementGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
