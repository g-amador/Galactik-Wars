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
    class SoundScreen : MenuScreen
    {
        /// <summary>
        /// The controls screen is brought up over the top of the main menu
        /// screen, and gives the user a chance to configure the game
        /// in various hopefully useful ways.
        /// </summary>
        #region Fields

        int selectedEntry = 0;
        string message = "Music Volume: ";
        string message2 = "                         Effects Volume: ";
        float music = SoundHandler.musicVolume * 100, effects = SoundHandler.effectsVolume * 100;

        Texture2D gradientTexture;

        #endregion

        #region Initialization


        ///<summary>
        ///Constructor
        ///</summary>
        public SoundScreen(string message)
                : this(message, true)
            { }

        ///<summary>
        ///Constructor
        ///</summary>
        public SoundScreen(string menuTitle, bool a)
            : base(Resources.Sound)
        {
            MenuEntry MusicUpMenuEntry = new MenuEntry(Resources.MusicUp);
            MenuEntry MusicDownMenuEntry = new MenuEntry(Resources.MusicDown);
            MenuEntry effectsUpMenuEntry = new MenuEntry(Resources.EffectsUp);
            MenuEntry effectsDownMenuEntry = new MenuEntry(Resources.EffectsDown);
            MenuEntry backMenuEntry = new MenuEntry(Resources.Back);
            MenuEntry exitMenuEntry = new MenuEntry(Resources.Exit);

            MusicUpMenuEntry.Selected += MusicUpMenuEntrySelected;
            MusicDownMenuEntry.Selected += MusicDownMenuEntrySelected;
            effectsUpMenuEntry.Selected += effectsUpMenuEntrySelected;
            effectsDownMenuEntry.Selected += effectsDownMenuEntrySelected;
            backMenuEntry.Selected += backMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            MenuEntries.Add(MusicUpMenuEntry);
            MenuEntries.Add(MusicDownMenuEntry);
            MenuEntries.Add(effectsUpMenuEntry);
            MenuEntries.Add(effectsDownMenuEntry);
            MenuEntries.Add(backMenuEntry);
            MenuEntries.Add(exitMenuEntry);
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

        #region Handle Input

        ///<summary>
        ///Responds to user input, changing the selected entry and accepting
        ///or cancelling the menu.
        ///</summary>
        public override void HandleInput(InputState input)
        {
            //Move to the previous menu entry?
            if (input.MenuUp)
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = 4;
            }

            //Move to the next menu entry?
            if (input.MenuDown)
            {
                selectedEntry++;

                if (selectedEntry > 4)
                    selectedEntry = 0;
            }

            //Accept or cancel the menu?
            if (input.MenuSelect)
            {
                OnSelectEntry(selectedEntry);
            }
            else if (input.MenuCancel)
            {
                OnCancel();
            }
        }

        /////<summary>
        /////Handler for when the user has chosen a menu entry.
        /////</summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            MenuEntries[selectedEntry].OnSelectEntry();
        }

        void MusicUpMenuEntrySelected(object sender, EventArgs e)
        {
            music = ((music/100) + 0.10f)*100;

            if (music > 100)
            {
                music = 100;
            }

            SoundHandler.musicVolume = music / 100;
        }

        void MusicDownMenuEntrySelected(object sender, EventArgs e)
        {
            music = ((music/100) - 0.10f)*100;

            if (music <= 0)
            {
                music = 0;
            }

            SoundHandler.musicVolume = music / 100;
        }

        void effectsUpMenuEntrySelected(object sender, EventArgs e)
        {
            effects = ((effects / 100) + 0.10f)*100;

            if (effects > 100)
            {
                effects = 100;
            }

            SoundHandler.effectsVolume = effects / 100; 
        }

        void effectsDownMenuEntrySelected(object sender, EventArgs e)
        {
            effects = ((effects / 100) - 0.10f) * 100;

            if (effects <= 0)
            {
                effects = 0;
            }

            SoundHandler.effectsVolume = effects / 100;
        }

        void backMenuEntrySelected(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(), new OptionsMenuScreen(Resources.Options));
        }

        ///<summary>
        ///When the user cancels the main menu, ask if they want to exit the sample.
        ///</summary>
        protected override void OnCancel()
        {
            LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(), new OptionsMenuScreen(Resources.Options));
        }

        #endregion

        #region Update and draw

        ///<summary>
        ///Updates the menu.
        ///</summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            MenuEntries[0].Update(this, false, gameTime);

            for (int i = 1; i < 5; i++)
            {
                MenuEntries[i].Update(this, true, gameTime);
            }

            //Update each nested MenuEntry object.
            for (int i = 0; i < 5; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                MenuEntries[i].Update(this, isSelected, gameTime);
            }
        }


         ///<summary>
        ///Draws the menu.
        ///</summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 position = new Vector2(100, 150);

            //Make the menu slide into place during transitions, using a
            //power curve to make things look more interesting (this makes
            //the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            //Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = new Vector2(100,50);
            Vector2 textPosition2 = new Vector2(100,50);

            Rectangle backgroundRectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);

            //Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, TransitionAlpha);

            spriteBatch.Begin();

            //Draw the background rectangle.
            spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

            //Draw the message box text.
            spriteBatch.DrawString(font, message + music +" %", textPosition, color);
  
            //Draw the message box text.
            spriteBatch.DrawString(font, message2 + effects + " %", textPosition2, color);

            //Draw each menu entry in turn.
            for (int i = 0; i < 5; i++)
            {
                MenuEntry menuEntry = MenuEntries[i];

                bool isSelected = IsActive && (i == selectedEntry);

                menuEntry.Draw(this, position, isSelected, gameTime);

                position.Y += menuEntry.GetHeight(this);
            }
            spriteBatch.End();
        }

        #endregion
    }

}
