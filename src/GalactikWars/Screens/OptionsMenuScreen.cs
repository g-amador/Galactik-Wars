#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Author: GNPA & STX
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GalactikWars.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion


namespace GalactikWars.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    internal class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        private int selectedEntry = 0;
        private readonly string menuTitle;

        private Texture2D gradientTexture;

        #endregion

        #region Initialization

        ///<summary>
        ///Constructor
        ///</summary>
        public OptionsMenuScreen(string message)
             : base(Resources.Options)
        {
            menuTitle = message;

            MenuEntry controlsMenuEntry = new(Resources.Controls);
            MenuEntry soundMenuEntry = new(Resources.Sound);
            MenuEntry backMenuEntry = new(Resources.Back);
            MenuEntry exitMenuEntry = new(Resources.Exit);

            controlsMenuEntry.Selected += controlsMenuEntrySelected;
            soundMenuEntry.Selected += soundMenuEntrySelected;
            backMenuEntry.Selected += backMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            //Add entries to the menu.
            MenuEntries.Add(controlsMenuEntry);
            MenuEntries.Add(soundMenuEntry);
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

            gradientTexture = content.Load<Texture2D>("ui/background");
        }
        #endregion

        #region Handle Input

        /////<summary>
        /////Handler for when the user has chosen a menu entry.
        /////</summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            MenuEntries[selectedEntry].OnSelectEntry();
        }

        private void controlsMenuEntrySelected(object sender, EventArgs e)
        {
            ControlsScreen controls = new(null);

            ScreenManager.AddScreen(controls);
            LoadingScreen.Load(ScreenManager, true, controls);
        }

        private void soundMenuEntrySelected(object sender, EventArgs e)
        {
            SoundScreen sound = new(null);

            ScreenManager.AddScreen(sound);
            LoadingScreen.Load(ScreenManager, true, sound);
        }

        private void backMenuEntrySelected(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(), new MainMenuScreen());
        }

        ///<summary>
        ///When the user cancels the main menu, ask if they want to exit the sample.
        ///</summary>
        protected override void OnCancel()
        {
            LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(),
                new MainMenuScreen());
        }

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
                {
                    selectedEntry = 2;
                }
            }

            //Move to the next menu entry?
            if (input.MenuDown)
            {
                selectedEntry++;

                if (selectedEntry > 2)
                {
                    selectedEntry = 0;
                }
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

            for (int i = 1; i < 3; i++)
            {
                MenuEntries[i].Update(this, true, gameTime);
            }

            //Update each nested MenuEntry object.
            for (int i = 0; i < 3; i++)
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

            Vector2 titlePosition = new(400, 60);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new((byte)192, (byte)192, (byte)192, TransitionAlpha);
            float titleScale = 1.50f;

            Vector2 position = new(100, 150);
            Color color = new((byte)255, (byte)255, (byte)255, TransitionAlpha);

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new(viewport.Width, viewport.Height);
            Rectangle backgroundRectangle = new(0, 0, viewport.Width, viewport.Height);

            //Make the menu slide into place during transitions, using a
            //power curve to make things look more interesting (this makes
            //the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
            {
                position.X -= transitionOffset * 256;
            }
            else
            {
                position.X += transitionOffset * 512;
            }

            spriteBatch.Begin();

            //Draw the background rectangle.
            spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

            //Draw the menu title.
            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            //Draw each menu entry in turn.
            for (int i = 0; i < 3; i++)
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
