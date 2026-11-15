#region File Description
//-----------------------------------------------------------------------------
//GameplayScreen.cs
//
//Author: GNPA & STX
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace GalactikWars
{
    class Player
    {
        #region Fields

        internal Ship ship = new Ship();
        internal Bullet[] bulletList = new Bullet[GameVariables.NumBullets];
        internal Rocket[] rocketList = new Rocket[GameVariables.NumRockets];

        internal int score = 0;
        internal float radius = 1;
        internal int lives = GameVariables.NumLives;
        internal int shield = GameVariables.Shield;
        internal int atomBombs = GameVariables.NumAtomBombs;

        #endregion

        #region Gets and Sets

        public int AtomBombs
        {
            get { return atomBombs; }
            set { atomBombs = value; }
        }

        public int Shield
        {
            get { return shield; }
            set { shield = value; }
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        #endregion

        #region Update, ShootRocket, ShootBullet and Warp To Center

        internal void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Add velocity to the current position.
            ship.Position += ship.Velocity;

            // Bleed off velocity over time.
            ship.Velocity *= 0.95f;

            for (int i = 0; i < GameVariables.NumBullets; i++)
            {
                if (bulletList[i].IsActive)
                {
                    bulletList[i].Update(timeDelta);
                }
            }

            for (int i = 0; i < GameVariables.NumRockets; i++)
            {
                if (rocketList[i].IsActive)
                {
                    rocketList[i].Update(timeDelta);
                }
            }
        }

        internal void WarpToCenter()
        {
            shield = GameVariables.Shield;
            atomBombs = GameVariables.NumAtomBombs;
            ship.IsActive = true;
            ship.Position = Vector3.Zero;
            ship.Velocity = Vector3.Zero;
            ship.Rotation = 0.0f;
            if (score - GameVariables.WarpPenalty < 0)
                score = 0;
            else
                score -= GameVariables.WarpPenalty;
        }

        internal bool ShootBullet()
        {
            //add another bullet.  Find an inactive bullet slot and use it
            //if all bullets slots are used, ignore the user input
            for (int i = 0; i < GameVariables.NumBullets; i++)
            {
                if (!bulletList[i].IsActive)
                {
                    bulletList[i].direction = ship.RotationMatrix.Forward;
                    bulletList[i].speed = GameVariables.BulletSpeedAdjustment;
                    bulletList[i].position = ship.Position + (200 * bulletList[i].direction);
                    bulletList[i].IsActive = true;
                    if (score - GameVariables.ShotPenalty < 0)
                        score = 0;
                    else
                        score -= GameVariables.ShotPenalty;
                    return true; //exit the loop     
                }
            }

            return false;
        }

        internal bool ShootRocket()
        {
            //add another rocket.  Find an inactive rocket slot and use it
            //if all rocket slots are used, ignore the user input
            for (int i = 0; i < GameVariables.NumRockets; i++)
            {
                if (!rocketList[i].IsActive)
                {
                    rocketList[i].rotation = ship.RotationMatrix;
                    rocketList[i].direction = rocketList[i].rotation.Forward;
                    rocketList[i].speed = GameVariables.RocketSpeedAdjustment;
                    rocketList[i].position = ship.Position + (200 * rocketList[i].direction);
                    rocketList[i].IsActive = true;
                    if (score - GameVariables.ShotPenalty < 0)
                        score = 0;
                    else
                        score -= GameVariables.ShotPenalty;
                    return true; //exit the loop     
                }
            }
            return false;
        }

        #endregion
    }
}
