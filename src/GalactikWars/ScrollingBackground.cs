#region File Description
//-----------------------------------------------------------------------------
//ScrollingBackground.cs
//
//Author: GNPA & STX
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace GalactikWars
{
    public class ScrollingBackground
    {
        #region Fields

        //class ScrollingBackground
        private Vector2 screenpos, origin, texturesize;
        private Texture2D mytexture;
        private int screenheight;
        public bool isActive = GameVariables.ScrollingBackgroundIsActive;

        #endregion

        #region Initialization

        public void LoadContent( GraphicsDevice device, Texture2D backgroundTexture )
        {
            mytexture = backgroundTexture;
            screenheight = device.Viewport.Height;
            int screenwidth = device.Viewport.Width;
            //Set the origin so that we're drawing from the 
            //center of the top edge.
            origin = new Vector2( mytexture.Width / 2, 0 );
            //Set the screen position to the center of the screen.
            screenpos = new Vector2( screenwidth / 2, screenheight / 2 );
            //Offset to draw the second texture, when necessary.
            texturesize = new Vector2( 0, mytexture.Height );
        }

        public void UnLoadContent()
        {
            mytexture.Dispose();
        }

        #endregion

        #region Update and Draw

        //ScrollingBackground.Update
        public void Update( GameTime gameTime )
        {
            float deltaY = (float)gameTime.ElapsedGameTime.TotalSeconds * 100;
            screenpos.Y += deltaY;
            screenpos.Y = screenpos.Y % mytexture.Height;
        }

        //ScrollingBackground.Draw
        public void Draw( SpriteBatch batch )
        {
            //Draw the texture, if it is still onscreen.
            if (screenpos.Y < screenheight)
            {
                batch.Draw( mytexture, screenpos, null,
                     Color.White, 0, origin, 1, SpriteEffects.None, 0f );
            }
            //Draw the texture a second time, behind the first,
            //to create the scrolling illusion.
            batch.Draw( mytexture, screenpos - texturesize, null,
                 Color.White, 0, origin, 1, SpriteEffects.None, 0f );
        }

        #endregion
    }
}
