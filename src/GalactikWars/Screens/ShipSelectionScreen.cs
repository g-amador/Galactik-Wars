#region File Description
//-----------------------------------------------------------------------------
//ShipSelectionScreen.cs
//
//Single-player ship choice (phase 3): the pilot picks one of three hulls
//(Pencil, Wedge or Saucer) and then, with left/right, the paint scheme that
//decides the team color (p1 hulls are blue, p2 hulls are red). A rotating 3D
//preview shows the actual model before the match starts.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GalactikWars.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace GalactikWars.Screens
{
    ///<summary>
    ///Lets the pilot pick the hull and the team color for this single-player
    ///run: up/down changes the hull, left/right changes the paint scheme.
    ///</summary>
    internal class ShipSelectionScreen : MenuScreen
    {
        #region Fields

        private ModelPreviewRenderer preview;
        private float previewRotation;

        //true while the pilot is looking at the blue (p1) paint scheme
        private bool blueTeam = true;

        #endregion

        #region Initialization

        public ShipSelectionScreen()
            : base("Choose Your Ship")
        {
            for (int i = 0; i < ShipProfile.HullCount; i++)
            {
                MenuEntry entry = new(ShipProfile.Create(i, true).Name);
                MenuEntries.Add(entry);
            }
        }

        ///<summary>
        ///Loads the preview renderer and every hull in both paint schemes up
        ///front, so switching hull or colour never stalls on a model load.
        ///</summary>
        public override void LoadContent()
        {
            preview = new ModelPreviewRenderer(ScreenManager.Game.Content,
                ScreenManager.GraphicsDevice);

            for (int i = 0; i < ShipProfile.HullCount; i++)
            {
                preview.LoadModel(ShipProfile.Create(i, true).ModelName);
                preview.LoadModel(ShipProfile.Create(i, false).ModelName);
            }
        }

        #endregion

        #region Handle Input

        ///<summary>
        ///Moves on to the escort pick, which also previews the pilot's hull
        ///and the escort in their team color.
        ///</summary>
        private void StartRun(ShipProfile ship)
        {
            ScreenManager.AddScreen(new EscortSelectionScreen(ship));
        }

        ///<summary>
        ///The highlighted hull is accepted with its current paint scheme.
        ///</summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            StartRun(CurrentShip);
        }

        ///<summary>
        ///Left and right flip between the blue and red paint schemes.
        ///</summary>
        protected override void OnMenuLeft()
        {
            blueTeam = !blueTeam;
        }

        protected override void OnMenuRight()
        {
            blueTeam = !blueTeam;
        }

        ///<summary>
        ///Cancelling returns to the main menu (this screen is pushed on top of
        ///it, so it is still in the screen stack).
        ///</summary>
        protected override void OnCancel()
        {
            ExitScreen();
        }

        ///<summary>The hull and paint scheme the pilot is looking at now.</summary>
        private ShipProfile CurrentShip => ShipProfile.Create(SelectedEntry, blueTeam);

        #endregion

        #region Update and Draw

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            previewRotation +=
                (float)gameTime.ElapsedGameTime.TotalSeconds * 0.6f;

            if (previewRotation >= MathHelper.TwoPi)
            {
                previewRotation -= MathHelper.TwoPi;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ShipProfile ship = CurrentShip;
            Rectangle bounds = PreviewBounds;

            preview?.Draw(ship.ModelName, bounds, previewRotation, null);

            base.Draw(gameTime);

            DrawPreviewLabels(ship, bounds);
        }

        ///<summary>
        ///The preview sits in the right half, leaving the menu column free.
        ///</summary>
        private Rectangle PreviewBounds
        {
            get
            {
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                int width = Math.Max(180,
                    Math.Min(480, viewport.Width / 2 - 60));
                int height = Math.Max(140,
                    Math.Min(410, viewport.Height - 230));

                return new Rectangle(viewport.Width - width - 60, 130,
                    width, height);
            }
        }

        private void DrawPreviewLabels(ShipProfile ship, Rectangle bounds)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Color textColor = new((byte)200, (byte)200, (byte)200,
                TransitionAlpha);
            Color teamColor = new(ship.TeamLabelColor.R,
                ship.TeamLabelColor.G, ship.TeamLabelColor.B, TransitionAlpha);

            string title = ship.Name;
            string team = "Team: " + ship.TeamName;
            string hint = "Left / Right changes the colour.";

            Vector2 anchor = new(bounds.Center.X, bounds.Bottom + 12);
            float y = anchor.Y;

            spriteBatch.Begin();

            DrawCentered(spriteBatch, font, title, anchor.X, ref y, textColor);

            DrawCentered(spriteBatch, font, team, anchor.X, ref y, teamColor);

            DrawCentered(spriteBatch, font, hint, anchor.X, ref y, textColor);

            spriteBatch.End();
        }

        ///<summary>
        ///Draws a line centered on <paramref name="x"/> and advances
        ///<paramref name="y"/> to the next line.
        ///</summary>
        private static void DrawCentered(SpriteBatch spriteBatch, SpriteFont font,
            string text, float x, ref float y, Color color)
        {
            Vector2 size = font.MeasureString(text);

            spriteBatch.DrawString(font, text,
                new Vector2(x - size.X / 2, y), color);

            y += font.LineSpacing;
        }

        #endregion
    }
}
