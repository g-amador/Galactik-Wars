using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GalactikWars
{
    class Ship
    {
        public Model Model;
        public Matrix[] Transforms;

        //Position of the model in world space
        public Vector3 Position = Vector3.Zero;

        //Velocity of the model, applied each frame to the model's position
        public Vector3 Velocity = Vector3.Zero;

        private const float VelocityScale = 5.0f; //amplifies controller speed input
        private bool isActive = false;

        //public Matrix RotationMatrix = Matrix.Identity;
        public Matrix RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);
        private float rotation = 0.0f;

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set
            {
                float newVal = value;
                while (newVal >= MathHelper.TwoPi)
                {
                    newVal -= MathHelper.TwoPi;
                }
                while (newVal < 0)
                {
                    newVal += MathHelper.TwoPi;
                }

                if (rotation != newVal)
                {
                    rotation = newVal;
                    RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) *
                        Matrix.CreateRotationZ(rotation);
                }

                if (rotation != value)
                {
                    rotation = value;
                    RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) *
                        Matrix.CreateRotationZ(rotation);
                }
            }
        }

        public void Update(Vector3 modelVelocityAdd)
        {   
            Velocity += modelVelocityAdd;

            if (Position.X > GameVariables.PlayfieldSizeX)
                Position.X -= 2 * GameVariables.PlayfieldSizeX;
            if (Position.X < -GameVariables.PlayfieldSizeX)
                Position.X += 2 * GameVariables.PlayfieldSizeX;
            if (Position.Y > GameVariables.PlayfieldSizeY)
                Position.Y -= 2 * GameVariables.PlayfieldSizeY;
            if (Position.Y < -GameVariables.PlayfieldSizeY)
                Position.Y += 2 * GameVariables.PlayfieldSizeY;
        }
    }
}
