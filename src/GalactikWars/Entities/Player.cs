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
using Microsoft.Xna.Framework;
#endregion

namespace GalactikWars.Entities
{
    internal class Player
    {
        #region Fields

        internal Ship ship = new();
        internal Bullet[] bulletList = new Bullet[GameVariables.NumBullets];
        internal Rocket[] rocketList = new Rocket[GameVariables.NumRockets];

        internal int score = 0;
        internal float radius = 1;
        internal int lives = GameVariables.NumLives;
        internal int shield = GameVariables.Shield;
        internal int atomBombs = GameVariables.NumAtomBombs;
        internal int rockets = GameVariables.RocketAmmo;

        //the pilot's escort decides the loadout granted on start and on every
        //respawn (phase 3)
        internal EscortProfile Escort = EscortProfile.Standard;

        #endregion

        #region Gets and Sets

        public int AtomBombs
        {
            get { return atomBombs; }
            set { atomBombs = value; }
        }

        public int Rockets
        {
            get { return rockets; }
            set { rockets = value; }
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

        ///<summary>
        ///Applies the escort's advantage to the pilot's loadout. Called when
        ///the gameplay screen loads.
        ///</summary>
        internal void ApplyEscort(EscortProfile escort)
        {
            Escort = escort;

            shield = escort.Shield;
            rockets = escort.Rockets;
            atomBombs = escort.AtomBombs;
        }

        internal void WarpToCenter()
        {
            //keep the escort's loadout on every respawn
            shield = Escort.Shield;
            atomBombs = Escort.AtomBombs;
            rockets = Escort.Rockets;
            ship.IsActive = true;
            ship.Position = Vector3.Zero;
            ship.Velocity = Vector3.Zero;
            ship.Rotation = 0.0f;
            if (score - GameVariables.WarpPenalty < 0)
            {
                score = 0;
            }
            else
            {
                score -= GameVariables.WarpPenalty;
            }
        }

        internal bool ShootBullet()
        {
            //add another bullet.  Find an inactive bullet slot and use it
            //if all bullets slots are used, ignore the user input
            for (int i = 0; i < GameVariables.NumBullets; i++)
            {
                if (!bulletList[i].IsActive)
                {
                    //the ship model faces its matrix-backward direction (the
                    //RotationMatrix is pre-multiplied with CreateRotationX
                    //(PiOver2)), so the nose is Backward — using Forward would
                    //spawn the bullet at the tail and fly it backwards
                    bulletList[i].direction = ship.RotationMatrix.Backward;
                    bulletList[i].speed = GameVariables.BulletSpeedAdjustment;
                    bulletList[i].position = ship.Position + (200 * bulletList[i].direction);
                    bulletList[i].IsActive = true;
                    if (score - GameVariables.ShotPenalty < 0)
                    {
                        score = 0;
                    }
                    else
                    {
                        score -= GameVariables.ShotPenalty;
                    }

                    return true; //exit the loop     
                }
            }

            return false;
        }

        internal bool ShootRocket()
        {
            //rockets are a limited resource: no ammo, no rocket
            if (rockets <= 0)
            {
                return false;
            }

            //add another rocket.  Find an inactive rocket slot and use it
            //if all rocket slots are used, ignore the user input
            for (int i = 0; i < GameVariables.NumRockets; i++)
            {
                if (!rocketList[i].IsActive)
                {
                    rocketList[i].rotation = ship.RotationMatrix;
                    //see ShootBullet: the nose of the ship model is the
                    //matrix-backward direction, so rockets exit from the front
                    rocketList[i].direction = rocketList[i].rotation.Backward;
                    rocketList[i].speed = GameVariables.RocketSpeedAdjustment;
                    rocketList[i].position = ship.Position + (200 * rocketList[i].direction);
                    rocketList[i].IsActive = true;
                    rockets -= 1;
                    if (score - GameVariables.ShotPenalty < 0)
                    {
                        score = 0;
                    }
                    else
                    {
                        score -= GameVariables.ShotPenalty;
                    }

                    return true; //exit the loop     
                }
            }
            return false;
        }

        #endregion
    }
}
