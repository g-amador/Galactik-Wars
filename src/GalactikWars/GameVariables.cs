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
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace GalactikWars
{
    class GameVariables
    {
        #region Fields

        //camera variables
        public static float CameraHeight = 50000.0f;

        //asteroid variables
        public const int NumAsteroids1 = 10;
        public const int NumAsteroids2 = NumAsteroids1 * 2;
        public const float AsteroidMinSpeed = 100.0f;
        public const float AsteroidMaxSpeed = 300.0f;
        public const int AsteroidSpeedIncrement = 500;
        public const float AsteroidSpeedAdjustment = 5.0f;
        public const float AsteroidBoundingSphereScale = 0.95f;  //95% size
        public const int Asteroid1Damage = 25;
        public const int Asteroid2Damage = 10;

        //players constants
        public const int Shield = 100;
        public const int NumLives = 3;
        public const int NumRockets = 5;
        public const int NumBullets = 10;
        public const int NumAtomBombs = 2;
        public const int BulletDamage = 10;
        public const int RocketDamage = 20;
        public const float BulletSpeedAdjustment = 100.0f;
        public const float RocketSpeedAdjustment = 50.0f;
        public const float shipBoundingSphereScale = 0.5f;  //50% size
        public static int CurrentLevel = 0;

        //penalties/bonus constants
        public const int ShotPenalty = 1;
        public const int DeathPenalty = 100;
        public const int WarpPenalty = 50;
        public const int KillAsteroidBonus = 25;
        public const int KillShipBonus = 5000;

        //playfield variables
        public static float PlayfieldSizeX = 35000f;
        public static float PlayfieldSizeY = 25000f;

        //Volume Variables
        public const float defaultMusicVolume = 0.0f;
        public const float defaultEffectsVolume = 1.0f;

        //General Aspect Options
        public static bool ScrollingBackgroundIsActive = false;
        public const bool FlawToleranceOn = true;
        public const bool MouseVisible = false;
        public const bool MouseActive = false;
        public const bool FullScreen = false;
        public static bool _3D = false;

        #endregion
    }
}
