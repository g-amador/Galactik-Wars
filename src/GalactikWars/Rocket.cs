#region File Description
//-----------------------------------------------------------------------------
//Sound.cs
//
//Author: GNPA & STX
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace GalactikWars
{
    struct Rocket
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
                isActive = false;
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        #endregion
    }
}