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
using System;
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

            //keep the nose pointing along the flight direction: the model
            //faces its matrix-backward direction (same convention as the
            //ship), so angle = atan2(-dirX, dirY) makes the nose track
            //direction — including while homing in on a target
            float angle = (float)Math.Atan2(-direction.X, direction.Y);
            rotation = Matrix.CreateRotationX(MathHelper.PiOver2)
                * Matrix.CreateRotationZ(angle);

            if (position.X > GameVariables.PlayfieldSizeX ||
                position.X < -GameVariables.PlayfieldSizeX ||
                position.Y > GameVariables.PlayfieldSizeY ||
                position.Y < -GameVariables.PlayfieldSizeY)
            {
                isActive = false;
            }
        }

        ///<summary>
        ///Steers the rocket towards the target like a missile: directional
        ///thrusters correct the heading at a limited turn rate, so the
        ///rocket arcs towards its target instead of snapping onto it.
        ///</summary>
        public void SteerToward(Vector3 targetPosition, float turnRate, float delta)
        {
            Vector3 toTarget = targetPosition - position;

            if (toTarget.LengthSquared() < 0.0001f)
            {
                return;
            }

            float targetAngle = (float)Math.Atan2(toTarget.Y, toTarget.X);
            float currentAngle = (float)Math.Atan2(direction.Y, direction.X);
            float diff = targetAngle - currentAngle;

            //wrap the difference to the shortest arc [-pi, pi]
            if (diff > MathHelper.Pi)
            {
                diff -= MathHelper.TwoPi;
            }
            else if (diff < -MathHelper.Pi)
            {
                diff += MathHelper.TwoPi;
            }

            //the thrusters can only correct the heading so fast
            float step = MathHelper.Clamp(diff, -turnRate * delta, turnRate * delta);

            direction.X = (float)Math.Cos(currentAngle + step);
            direction.Y = (float)Math.Sin(currentAngle + step);
            direction.Z = 0.0f;
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        #endregion
    }
}
