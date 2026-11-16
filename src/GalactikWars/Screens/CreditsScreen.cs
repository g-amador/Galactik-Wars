#region Using Statements
using GalactikWars.Handlers;
using GalactikWars.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace GalactikWars.Screens
{
    internal class CreditsScreen : GameScreen
    {
        #region Fields

        private readonly string message; //change here to put text on menu

        private Texture2D gradientTexture;

        #endregion

        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Initialization


        ///<summary>
        ///Constructor
        ///</summary>
        public CreditsScreen(string message)
            : this(message, true)
        {
        }


        ///<summary>
        ///Constructor
        ///</summary>
        public CreditsScreen(string message, bool includeUsageText)
        {
            if (includeUsageText)
            {
                this.message += message + "GALACTIK WARS\n\n    V0.69 Alpha\n\n    AUTHORS\n   Nuno Amador\nRicardo Alexandre";
            }
            else
            {
                this.message += message;
            }

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }


        ///<summary>
        ///Loads graphics content for this screen. This uses the shared ContentManager
        ///provided by the Game class, so the content will remain loaded forever.
        ///Whenever a subsequent MessageBoxScreen tries to load this same content,
        ///it will just get back another reference to the already loaded data.
        ///</summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            gradientTexture = content.Load<Texture2D>("ui/background");

            ScreenManager.SoundHandler.PlayTrack(SoundHandler.CreditsTrack);
        }


        #endregion

        #region Handle Input

        ///<summary>
        ///Responds to user input, accepting or cancelling the message box.
        ///</summary>
        public override void HandleInput(InputState input)
        {
            if (input.MenuSelect)
            {
                //Raise the accepted event, then exit the message box.
                Accepted?.Invoke(this, EventArgs.Empty);

                ExitScreen();
                LoadingScreen.Load(ScreenManager, false, new BackgroundScreen(),
                                                new MainMenuScreen());
            }
            else if (input.MenuCancel)
            {
                //Raise the cancelled event, then exit the message box.
                Cancelled?.Invoke(this, EventArgs.Empty);

                ExitScreen();
                LoadingScreen.Load(ScreenManager, false, new BackgroundScreen(),
                                                     new MainMenuScreen());
            }
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
            Vector2 viewportSize = new(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize) / 2;

            Rectangle backgroundRectangle = new(0, 0, viewport.Width, viewport.Height);


            //Fade the popup alpha during transitions.
            Color color = new((byte)255, (byte)255, (byte)255, TransitionAlpha);

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
