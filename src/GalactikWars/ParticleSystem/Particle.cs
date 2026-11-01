#region File Description
//-----------------------------------------------------------------------------
//Particle.cs
//
//Author: GNPA & STX
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace GalactikWars
{
    class Particle
    {
        #region Fields

        public Vector2 xyorg;// The original X,Y values of the particle. This is used in checking whether or not one virtual emitter has enough particles.
        public Vector2 xyoffset;// This is used to move the particle.
        public bool used = false;// Checks whether or not the particle is in use. This is so we can reuse old particles rather than waste memory creating and deleting new and old particles.
        public int clr;// The color of the particle.
        public int lifeSpan;// How long it lives.
        public float rot;// This rotates the particle.
        public int halfLife;// This variable will tell the particle to behave differently after it reaches a certain point in its lifespan. In this sample it will be used to simulate gravity.
        public int vspd;// The speed it goes either up or down.
        public int hspd;// The speed it goes either left or right.
        public int size;// The size of the particle.

        #endregion
    }
}
