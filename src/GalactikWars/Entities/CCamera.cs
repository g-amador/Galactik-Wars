#region File Description
//-----------------------------------------------------------------------------
//CCamera.cs
//
//Author: GNPA & STX
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion


namespace GalactikWars
{
    class CCamera
    {
        #region Fields

        private Matrix _view; 
        private Matrix _projection;
        private Vector3 _position; 
        private Vector3 _target;
        private float _aspectRatio;

        #endregion

        #region Initialize ans Update

        public void Initialize(Vector3 position, Vector3 target)
        {
            _aspectRatio = (float)GraphicsDeviceManager.DefaultBackBufferWidth / GraphicsDeviceManager.DefaultBackBufferHeight;
            
            // Create view and projection matrix
            _view = Matrix.CreateLookAt(position, target, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, _aspectRatio, 
                GameVariables.CameraHeight - 1000.0f, 
                GameVariables.CameraHeight);
            
            // Store camera position
            _position = position;
            _target = target;
        }

        public void Update()
        {
            _view = Matrix.CreateLookAt(_position, _target, Vector3.Up);
        }

        #endregion

        #region  Acess to camera characteristics

        public Matrix View
        {
            get { return _view; }
        }

        public Vector3 Position
        {
            set { _position = value; }
            get { return _position; }
        }

        public Matrix Projection
        {
            get { return _projection; }
        }

        #endregion

        #region methods set projection, translation, and rotation of the camera

        public void SetProjection(float newCameraHeight)
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                _aspectRatio, 1.0f, newCameraHeight * 100.0f);
        }

        public void SetTranslation(Vector3 position)
        {
            _position += position; 
            _target += position;
            _view = Matrix.CreateLookAt(_position, _target, Vector3.Up);
        }

        public void SetRotationX(float radians)
        {
            _view *= Matrix.CreateRotationX(radians); 
        }

        public void SetRotationY(float radians)
        {
            _view *= Matrix.CreateRotationY(radians); 
        }

        public void SetRotationZ(float radians)
        {
            _view *= Matrix.CreateRotationZ(radians);
        }

        #endregion
    }
}
