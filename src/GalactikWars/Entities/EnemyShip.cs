#region File Description
//-----------------------------------------------------------------------------
//EnemyShip.cs
//
//Red enemy ships and blue ally ships for single-player (phase 3):
//enemies hunt the player, the ally escorts them.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GalactikWars.Entities
{
    internal class EnemyShip
    {
        #region Fields

        public Vector3 Position = Vector3.Zero;
        public Vector3 Velocity = Vector3.Zero;
        public Matrix RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);
        public bool IsActive = false;
        public bool IsAlly = false;   //blue allies escort, red enemies attack
        public Model Model = null;
        public Matrix[] Transforms = null;
        //the ship's team tint (ambient light): allies wear the player's
        //colors, enemies wear the opposite color
        public Vector3 Tint = new(0.70f, 0.12f, 0.12f);
        public Bullet[] bulletList = new Bullet[GameVariables.NumBullets];
        public int Shield = GameVariables.Shield;
        public float Radius = 1.0f;
        public float fireTimer = 0.0f;//seconds until the next shot

        private float rotation = 0.0f;

        #endregion

        #region Gets and Sets

        public float Rotation
        {
            get { return rotation; }
            set
            {
                //keep the angle in [0, 2PI), like Ship.Rotation, and rebuild
                //the matrix so the nose (matrix-backward) tracks the angle
                float newVal = value;

                while (newVal >= MathHelper.TwoPi)
                {
                    newVal -= MathHelper.TwoPi;
                }

                while (newVal < 0)
                {
                    newVal += MathHelper.TwoPi;
                }

                rotation = newVal;
                RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) *
                    Matrix.CreateRotationZ(rotation);
            }
        }

        #endregion

        #region Update and ShootBullet

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //add velocity to the current position, bleeding it off over time
            //(same motion model as the player's ship)
            Position += Velocity;
            Velocity *= 0.95f;

            //wrap around the playfield like the asteroids do
            if (Position.X > GameVariables.PlayfieldSizeX)
            {
                Position.X -= 2 * GameVariables.PlayfieldSizeX;
            }

            if (Position.X < -GameVariables.PlayfieldSizeX)
            {
                Position.X += 2 * GameVariables.PlayfieldSizeX;
            }

            if (Position.Y > GameVariables.PlayfieldSizeY)
            {
                Position.Y -= 2 * GameVariables.PlayfieldSizeY;
            }

            if (Position.Y < -GameVariables.PlayfieldSizeY)
            {
                Position.Y += 2 * GameVariables.PlayfieldSizeY;
            }

            for (int i = 0; i < GameVariables.NumBullets; i++)
            {
                if (bulletList[i].IsActive)
                {
                    bulletList[i].Update(delta);
                }
            }
        }

        public bool ShootBullet()
        {
            //fire from the nose along the facing direction (same rule as the
            //player's ship: the nose is RotationMatrix.Backward)
            for (int i = 0; i < GameVariables.NumBullets; i++)
            {
                if (!bulletList[i].IsActive)
                {
                    bulletList[i].direction = RotationMatrix.Backward;
                    bulletList[i].speed = GameVariables.BulletSpeedAdjustment;
                    bulletList[i].position = Position + (200 * bulletList[i].direction);
                    bulletList[i].IsActive = true;

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
