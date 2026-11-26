#region File Description
//-----------------------------------------------------------------------------
//EscortSelectionScreen.cs
//
//Single-player loadout choice (phase 3): each escort grants the pilot one
//advantage (shield, thrust, rockets, atom bombs or fire rate). The pick is
//passed to the gameplay screen, which applies it to the player's loadout.
//The preview shows the pilot's hull and the escort in the pilot's team color,
//so the side is known before the match starts.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GalactikWars.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
#endregion

namespace GalactikWars.Screens
{
    ///<summary>
    ///Lets the pilot pick the escort that will fly alongside them for this
    ///single-player run.
    ///</summary>
    internal class EscortSelectionScreen : MenuScreen
    {
        #region Fields

        //the hull (and team color) picked on the ship screen
        private readonly ShipProfile ship;

        private ModelPreviewRenderer preview;
        private float previewRotation;

        #endregion

        #region Initialization

        public EscortSelectionScreen(ShipProfile ship)
            : base("Choose Your Escort")
        {
            this.ship = ship ?? ShipProfile.Default;

            foreach (EscortProfile escort in EscortProfile.All)
            {
                EscortProfile picked = escort;

                //stick to ASCII: the menu spritefont has no em dash glyph
                MenuEntry entry = new(escort.Name + " - " + escort.Advantage);
                entry.Selected += (sender, e) => StartRun(picked);

                MenuEntries.Add(entry);
            }
        }

        ///<summary>
        ///Loads the preview renderer plus every hull the screen can show: the
        ///pilot's ship and each escort swapped into the pilot's team family.
        ///</summary>
        public override void LoadContent()
        {
            preview = new ModelPreviewRenderer(ScreenManager.Game.Content,
                ScreenManager.GraphicsDevice);

            preview.LoadModel(ship.ModelName);

            HashSet<string> escortModels = [];

            foreach (EscortProfile escort in EscortProfile.All)
            {
                string modelName = EscortModelName(escort);

                if (escortModels.Add(modelName))
                {
                    preview.LoadModel(modelName);
                }
            }
        }

        #endregion

        #region Handle Input

        ///<summary>
        ///Loads the single-player arena with the chosen hull and escort. The
        ///loading screen replaces this menu and the screens behind it.
        ///</summary>
        private void StartRun(EscortProfile escort)
        {
            LoadingScreen.Load(ScreenManager, true,
                new GameplayScreenSinglePlayer(ship, escort));
        }

        ///<summary>
        ///Cancelling returns to the ship picker (this screen is pushed on top
        ///of it, so it is still in the screen stack).
        ///</summary>
        protected override void OnCancel()
        {
            ExitScreen();
        }

        ///<summary>
        ///The escort hull is catalogued as a p1 model; swap it to the p2 family
        ///when the pilot flies red so the texture matches the team color.
        ///</summary>
        private string EscortModelName(EscortProfile escort)
        {
            return ShipProfile.ForTeam(escort.AllyModelName, ship.IsBlueTeam);
        }

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
            EscortProfile escort = EscortProfile.All[SelectedEntry];

            Rectangle shipBounds = ShipPreviewBounds;
            int gap = 24;
            Rectangle escortBounds = new(
                shipBounds.Right + gap, shipBounds.Y,
                shipBounds.Width, shipBounds.Height);

            //the pilot's own hull is drawn untinted (its texture carries the
            //team color); the escort wears the team tint, exactly as in game
            preview?.Draw(ship.ModelName, shipBounds, previewRotation, null);
            preview?.Draw(EscortModelName(escort), escortBounds, previewRotation,
                ship.TeamTint);

            base.Draw(gameTime);

            DrawPreviewLabels(escort, shipBounds, escortBounds);
        }

        ///<summary>
        ///The two previews sit on the right, leaving the menu column free.
        ///</summary>
        private Rectangle ShipPreviewBounds
        {
            get
            {
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                int width = Math.Max(110,
                    Math.Min(230, viewport.Width / 4 - 40));
                int height = Math.Max(140,
                    Math.Min(360, viewport.Height - 260));

                //the escort box sits to the right of this one; anchor the pair
                //to the right edge of the window
                return new Rectangle(
                    viewport.Width - 60 - 2 * width - 24, 150, width, height);
            }
        }

        private void DrawPreviewLabels(EscortProfile escort, Rectangle shipBounds,
            Rectangle escortBounds)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Color textColor = new((byte)200, (byte)200, (byte)200,
                TransitionAlpha);
            Color teamColor = new(ship.TeamLabelColor.R,
                ship.TeamLabelColor.G, ship.TeamLabelColor.B, TransitionAlpha);

            float y = shipBounds.Bottom + 12;

            spriteBatch.Begin();

            DrawCentered(spriteBatch, font,
                ship.Name + " (" + ship.TeamName + ")",
                shipBounds.Center.X, y, textColor);

            DrawCentered(spriteBatch, font, escort.Name,
                escortBounds.Center.X, y, textColor);

            y += font.LineSpacing + 8;

            DrawCentered(spriteBatch, font, "Team: " + ship.TeamName,
                (shipBounds.Center.X + escortBounds.Center.X) / 2, y,
                teamColor);

            y += font.LineSpacing;

            DrawCentered(spriteBatch, font,
                "Your hull and escort fly this colour.",
                (shipBounds.Center.X + escortBounds.Center.X) / 2, y,
                textColor);

            spriteBatch.End();
        }

        ///<summary>
        ///Draws a line centered on <paramref name="x"/>.
        ///</summary>
        private static void DrawCentered(SpriteBatch spriteBatch, SpriteFont font,
            string text, float x, float y, Color color)
        {
            Vector2 size = font.MeasureString(text);

            spriteBatch.DrawString(font, text,
                new Vector2(x - size.X / 2, y), color);
        }

        #endregion
    }
}
