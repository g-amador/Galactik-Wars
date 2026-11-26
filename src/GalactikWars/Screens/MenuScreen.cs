#region File Description
//-----------------------------------------------------------------------------
//MenuScreen.cs
//
//XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GalactikWars.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
#endregion

namespace GalactikWars.Screens
{
    ///<summary>
    ///Base class for screens that contain a menu of options. The user can
    ///move up and down to select an entry, or cancel to back out of the screen.
    ///</summary>
    internal abstract class MenuScreen : GameScreen
    {
        #region Fields

        private readonly List<MenuEntry> menuEntries = [];
        private int selectedEntry = 0;
        private readonly string menuTitle;

        #endregion

        #region Properties


        ///<summary>
        ///Gets the list of menu entries, so derived classes can add
        ///or change the menu contents.
        ///</summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }


        ///<summary>
        ///Gets the index of the entry the user has highlighted. Preview
        ///screens use it to show the model for the current entry.
        ///</summary>
        protected int SelectedEntry
        {
            get { return selectedEntry; }
        }


        #endregion

        #region Initialization


        ///<summary>
        ///Constructor.
        ///</summary>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
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
                {
                    selectedEntry = menuEntries.Count - 1;
                }
            }

            //Move to the next menu entry?
            if (input.MenuDown)
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
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
            else
            {
                //optional horizontal actions (the ship picker uses them to
                //change the paint scheme)
                if (input.MenuLeft)
                {
                    OnMenuLeft();
                }

                if (input.MenuRight)
                {
                    OnMenuRight();
                }
            }
        }


        ///<summary>
        ///Handler for a horizontal "left" action. Menus that do not use it
        ///simply ignore it.
        ///</summary>
        protected virtual void OnMenuLeft()
        {
        }


        ///<summary>
        ///Handler for a horizontal "right" action. Menus that do not use it
        ///simply ignore it.
        ///</summary>
        protected virtual void OnMenuRight()
        {
        }


        ///<summary>
        ///Handler for when the user has chosen a menu entry.
        ///</summary>
        protected virtual void OnSelectEntry(int entryIndex)
        {
            menuEntries[selectedEntry].OnSelectEntry();
        }


        ///<summary>
        ///Handler for when the user has cancelled the menu.
        ///</summary>
        protected virtual void OnCancel()
        {
            ExitScreen();
        }


        ///<summary>
        ///Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        ///</summary>
        protected void OnCancel(object sender, EventArgs e)
        {
            OnCancel();
        }


        #endregion

        #region Update and Draw


        ///<summary>
        ///Updates the menu.
        ///</summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            //Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                menuEntries[i].Update(this, isSelected, gameTime);
            }
        }


        ///<summary>
        ///Draws the menu.
        ///</summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 position = new(100, 150);

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

            //Draw each menu entry in turn.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];

                bool isSelected = IsActive && (i == selectedEntry);

                menuEntry.Draw(this, position, isSelected, gameTime);

                position.Y += menuEntry.GetHeight(this);
            }

            //Draw the menu title.
            Vector2 titlePosition = new(400, 60);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new((byte)192, (byte)192, (byte)192, TransitionAlpha);
            float titleScale = 1.50f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion
    }
}
