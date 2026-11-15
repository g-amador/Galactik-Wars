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
    internal struct Bullet
    {
        #region Fields

        public Vector3 position;
        public Vector3 direction;
        public float speed;
        public bool isActive;

        #endregion

        #region Update and IsActive

        public void Update(float delta)
        {
            position += direction * speed * GameVariables.BulletSpeedAdjustment * delta * 8;
            if (position.X > GameVariables.PlayfieldSizeX ||
                position.X < -GameVariables.PlayfieldSizeX ||
                position.Y > GameVariables.PlayfieldSizeY ||
                position.Y < -GameVariables.PlayfieldSizeY)
            {
                isActive = false;
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
