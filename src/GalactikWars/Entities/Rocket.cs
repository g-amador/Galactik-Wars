#region File Description
//-----------------------------------------------------------------------------
//Sound.cs
//
//Author: GNPA & STX
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace GalactikWars.Entities
{
    internal struct Rocket
    {
        #region Fields

        public Vector3 position;
        public Vector3 direction;
        public float speed;
        public bool isActive;
        public Matrix rotation;

        #endregion

        #region Update and IsActive 

        public void Update(float delta)
        {
            position += direction * speed * GameVariables.RocketSpeedAdjustment * delta * 8;
            if (position.X > GameVariables.PlayfieldSizeX ||
                position.X < -GameVariables.PlayfieldSizeX ||
                position.Y > GameVariables.PlayfieldSizeY ||
                position.Y < -GameVariables.PlayfieldSizeY)
            {
                isActive = false;
            }
        }

        ///<summary>
        ///Steers the rocket a little towards the target position, so it
        ///homes in on the nearest asteroid over a few frames.
        ///</summary>
        public void SteerToward(Vector3 targetPosition, float turnRate, float delta)
        {
            Vector3 desired = targetPosition - position;

            if (desired.LengthSquared() < 0.0001f)
            {
                return;
            }

            desired.Normalize();

            float blend = MathHelper.Clamp(turnRate * delta, 0.0f, 1.0f);
            direction = Vector3.Lerp(direction, desired, blend);

            if (direction.LengthSquared() > 0.0001f)
            {
                direction.Normalize();
            }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        #endregion
    }
}
