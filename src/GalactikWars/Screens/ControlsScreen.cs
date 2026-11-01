#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

/*adicionei esta classe*/
namespace GalactikWars
{
    class ControlsScreen : MenuScreen
    {
        /// <summary>
        /// The controls screen is brought up over the top of the main menu
        /// screen, and gives the user a chance to configure the game
        /// in various hopefully useful ways.
        /// </summary>
            #region Fields

            string message = "                CONTROLS KEYBOARD\n MOVE UP: Up                  MOVE DOWN: Down\n ROTATE LEFT: Left          ROTATE RIGHT: Right\n SHOOT BULLETS: z          RESPAWN: q\n SHOOT ROCKETS: x         ATOM BOMB: c\n 2D VIEW: F1                  3D VIEW: F2 \n ZOOM-IN: e                  ZOOM-OUT: w\n\n                 CONTROLS MOUSE\n MOVE UP: Up                 MOVE DOWN: Down\n MOVE RIGHT: Right         MOVE LEFT: Left\n SHOOT BULLETS: Left Button\n SHOOT ROCKETS: Right Button\n ATOM BOMB: Middle Button"; //change here to put text on menu
            Texture2D gradientTexture;

            #endregion

            #region Initialization

            ///<summary>
            ///Constructor
            ///</summary>
            public ControlsScreen(string message)
                : this(message, true)
            { }

            ///<summary>
            ///Constructor
            ///</summary>
            public ControlsScreen(string message, bool a)
                : base(Resources.Controls)
            {
                MenuEntry exitMenuEntry = new MenuEntry(Resources.Exit);

                exitMenuEntry.Selected += OnCancel;

                MenuEntries.Add(exitMenuEntry);    
            }

            #endregion

            #region Handle Input
  
            ///<summary>
            ///When the user cancels the main menu, ask if they want to exit the sample.
            ///</summary>
            protected override void OnCancel()
            {
                LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(), new OptionsMenuScreen(Resources.Options));
            }
            #endregion

            #region load
            ///<summary>
            ///Loads graphics content for this screen. This uses the shared ContentManager
            ///provided by the Game class, so the content will remain loaded forever.
            ///Whenever a subsequent MessageBoxScreen tries to load this same content,
            ///it will just get back another reference to the already loaded data.
            ///</summary>
            public override void LoadContent()
            {
                ContentManager content = ScreenManager.Game.Content;

                gradientTexture = content.Load<Texture2D>("background");
            }
            #endregion

            #region Draw

            ///<summary>
            ///Draws the message box.
            ///</summary>
            public override void Draw(GameTime gameTime)
            {
                SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
                SpriteFont font = ScreenManager.Font;

                //Darken down any other screens that were drawn beneath the popup.
                ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

                //Center the message text in the viewport.
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
                Vector2 textSize = font.MeasureString(message);
                Vector2 textPosition = (viewportSize - textSize) / 2;

                Rectangle backgroundRectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);

                //Fade the popup alpha during transitions.
                Color color = new Color(255, 255, 255, TransitionAlpha);

                spriteBatch.Begin();

                //Draw the background rectangle.
                spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

                //Draw the message box text.
                spriteBatch.DrawString(font, message, textPosition, color);

                spriteBatch.End();
            }

            #endregion
        }
    
}
