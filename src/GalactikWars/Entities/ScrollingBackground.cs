#region File Description
//-----------------------------------------------------------------------------
//ScrollingBackground.cs
//
//A looping starfield that scrolls continuously top -> down, the background of
//the Raptor-style level 2 (phase 3). The texture is sampled with linear
//wrapping, so one stretched quad tiles vertically with no double-draw math.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace GalactikWars.Entities
{
    public class ScrollingBackground
    {
        #region Fields

        //how fast the stars travel down the screen, in pixels per second
        private const float ScrollSpeed = 120.0f;

        private Texture2D texture;

        //scroll position in texture pixels, always kept in [0, Height)
        private float scrollY;

        #endregion

        #region Initialization

        public void LoadContent(GraphicsDevice device, Texture2D backgroundTexture)
        {
            texture = backgroundTexture;
            scrollY = 0.0f;
        }

        ///<summary>
        ///Releases the reference only: the gameplay screen owns (and disposes)
        ///the texture loaded through the content pipeline.
        ///</summary>
        public void UnLoadContent()
        {
            texture = null;
        }

        #endregion

        #region Update and Draw

        public void Update(GameTime gameTime)
        {
            if (texture == null)
            {
                return;
            }

            //walk backwards through the texture so the stars move down the
            //screen (the ship is flying up, Raptor-style)
            scrollY -= ScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (scrollY < 0.0f)
            {
                scrollY += texture.Height;
            }
        }

        ///<summary>
        ///Draws the starfield stretched over the whole viewport. The sprite
        ///batch must use <see cref="SamplerState.LinearWrap"/> so the source
        ///rectangle can run past the texture height and wrap seamlessly.
        ///</summary>
        public void Draw(SpriteBatch batch)
        {
            if (texture == null)
            {
                return;
            }

            Viewport viewport = batch.GraphicsDevice.Viewport;

            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                return;
            }

            //fit the texture to the window width, then take exactly one
            //screen-height slice of it (two slices when the window is taller
            //than it is wide)
            float scale = (float)viewport.Width / texture.Width;
            int sourceHeight = (int)Math.Ceiling(viewport.Height / scale);
            sourceHeight = Math.Max(sourceHeight, 1);

            batch.Draw(
                texture,
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                new Rectangle(0, (int)scrollY, texture.Width, sourceHeight),
                Color.White);
        }

        #endregion
    }
}
