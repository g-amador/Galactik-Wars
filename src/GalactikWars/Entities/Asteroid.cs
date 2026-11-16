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
    internal struct Asteroid
    {
        #region Fields

        public Vector3 position;
        public Vector3 direction;
        public float speed;
        public bool isActive;

        #endregion

        #region Update

        public void Update(float delta)
        {
            position += direction * speed * GameVariables.AsteroidSpeedAdjustment * delta;

            if (position.X > GameVariables.PlayfieldSizeX)
            {
                position.X -= 2 * GameVariables.PlayfieldSizeX;
            }

            if (position.X < -GameVariables.PlayfieldSizeX)
            {
                position.X += 2 * GameVariables.PlayfieldSizeX;
            }

            if (position.Y > GameVariables.PlayfieldSizeY)
            {
                position.Y -= 2 * GameVariables.PlayfieldSizeY;
            }

            if (position.Y < -GameVariables.PlayfieldSizeY)
            {
                position.Y += 2 * GameVariables.PlayfieldSizeY;
            }
        }

        #endregion
    }
}
