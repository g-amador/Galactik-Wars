#region File Description
//-----------------------------------------------------------------------------
//GameplayScreenSinglep.cs
//
//Author: GNPA & STX based on Microsoft "Make a Game in 60 minutes" tutorial
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace GalactikWars
{
    ///<summary>
    ///This screen implements the actual game logic. It is just a
    ///placeholder to get the idea across: you'll probably want to
    ///put some more interesting gameplay in here!
    ///</summary>
    class GameplayScreenSinglePlayer : GameScreen
    {
        #region Fields

        ContentManager content = null;

        //Objects to do sound/keyboard/Camera/Collision handling
        CollisionsHandler collisionsHandler = new CollisionsHandler();
        SoundHandler soundHandler = new SoundHandler();
        CCamera camera = new CCamera();

        //asteroid variables
        Asteroid[] asteroid1List = new Asteroid[GameVariables.NumAsteroids1];
        Asteroid[] asteroid2List = new Asteroid[GameVariables.NumAsteroids2];
        int activeAsteroids = GameVariables.NumAsteroids1 + GameVariables.NumAsteroids2;
        int destroyedAsteroids = 0;
        int asteroidSpeed = 0;
        
        //Visual components
        ScrollingBackground myBackground;
        Texture2D space1 = null;
        Texture2D space2 = null;
        Texture2D space3 = null;
        Texture2D space4 = null;
        Texture2D space5 = null;
        Texture2D particlesTexture = null;

        Model rocketModel = null;
        Model bulletModel = null;
        Model shieldModel = null;
        Model[] asteroidModels = null;
        Matrix[] shieldTransforms = null;
        Matrix[] bulletTransforms = null;
        Matrix[] rocketTransforms = null;
        Matrix[] asteroid1Transforms = null;
        Matrix[] asteroid2Transforms = null;

        SpriteBatch spriteBatch = null;
        SpriteFont lucidaConsole = null;
        SpriteFont font = null;

        //Player
        Player p= new Player();
  
        Vector2 scorePosition = new Vector2(60, 110);
        Vector2 shieldPosition = new Vector2(60, 90);
        Vector2 remainingLivesPosition = new Vector2(60, 70);
        Vector2 remainingAtomBombsPosition = new Vector2(60, 50);
        Vector2 remainingAsteroidPosition = new Vector2(60, 30);
        Vector2 levelPosition = new Vector2(60, 10);

        Random random = new Random();

        //float shipTrailX, shipTrailY;
        float explosionX, explosionY;
        Particles particles = new Particles();
        //List<Particle> shipTrail = new List<Particle>();
        List<Particle> explosion = new List<Particle>();

        #endregion

        #region Initialization

        ///<summary>
        ///Constructor.
        ///</summary>
        public GameplayScreenSinglePlayer()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            camera.Initialize(new Vector3(0.0f, 0.0f, GameVariables.CameraHeight), Vector3.Zero);

            GameVariables.CurrentLevel = 0;

            asteroidModels = new Model[2];

            //show asteroids 
            ResetAsteroids();
        }

        private Matrix[] SetupEffectDefaults(Model myModel)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = camera.Projection;
                    effect.View = camera.View;
                }
            }
            return absoluteTransforms;
        }

        ///<summary>
        ///Load graphics content for the game.
        ///</summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            if (random.NextDouble() <= 0.17)
            {
                p.ship.Model = content.Load<Model>("Models/p1_pencil");
            }
            else if (random.NextDouble() <= 0.34)
            {
                p.ship.Model = content.Load<Model>("Models/p2_pencil");
            }
            else if (random.NextDouble() <= 0.51)
            {
                p.ship.Model = content.Load<Model>("Models/p1_wedge");
            }
            else if (random.NextDouble() <= 0.68)
            {
                p.ship.Model = content.Load<Model>("Models/p2_wedge");
            }
            else if (random.NextDouble() <= 0.85)
            {
                p.ship.Model = content.Load<Model>("Models/p1_saucer");
            }
            else
            {
                p.ship.Model = content.Load<Model>("Models/p2_saucer");
            }

            p.ship.Transforms = SetupEffectDefaults(p.ship.Model);
            p.ship.IsActive = true;

            asteroidModels[0] = content.Load<Model>("Models/asteroid1");
            asteroid1Transforms = SetupEffectDefaults(asteroidModels[0]);
            asteroidModels[1] = content.Load<Model>("Models/asteroid2");
            asteroid2Transforms = SetupEffectDefaults(asteroidModels[1]);
            bulletModel = content.Load<Model>("Models/pea_proj");
            bulletTransforms = SetupEffectDefaults(bulletModel);
            rocketModel = content.Load<Model>("Models/p1_rocket");
            rocketTransforms = SetupEffectDefaults(rocketModel);
            shieldModel = content.Load<Model>("Models/sphere");
            shieldTransforms = SetupEffectDefaults(shieldModel);

            //activate the static background or the dinamic "myBackground"
            if (GameVariables.ScrollingBackgroundIsActive)
            {
                myBackground = new ScrollingBackground();
                space1 = content.Load<Texture2D>("Textures/starfield");
                myBackground.LoadContent(ScreenManager.GraphicsDevice, space1);
            }
            else
            {
                space1 = content.Load<Texture2D>("Textures/galaxy");                
                space2 = content.Load<Texture2D>("Textures/aurora");
                space3 = content.Load<Texture2D>("Textures/nebula");
                space4 = content.Load<Texture2D>("Textures/nebula2");
                space5 = content.Load<Texture2D>("Textures/planets");
            }

            particlesTexture = content.Load<Texture2D>("Textures/multicolored");

            lucidaConsole = content.Load<SpriteFont>("Fonts/Lucida Console");
            spriteBatch = ScreenManager.SpriteBatch;
            font = ScreenManager.Font;
        }

        ///<summary>
        ///Unload graphics content used by the game.
        ///</summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            soundHandler.UnLoad();

            //Visual components
            if (GameVariables.ScrollingBackgroundIsActive)
                myBackground.UnLoadContent();

            space1.Dispose();
            space2.Dispose();
            space3.Dispose();
            space4.Dispose();
            space5.Dispose();
            particlesTexture.Dispose();

            content.Unload();
        }

        #endregion

        #region Auxiliar Functions

        private void ResetAsteroids()
        {
            float xStart;
            float yStart;

            if (GameVariables.NumAsteroids1 + GameVariables.NumAsteroids2 != 0)
                GameVariables.CurrentLevel += 1;

            for (int i = 0; i < GameVariables.NumAsteroids2; i++)
            {
                double angle = random.NextDouble() * 2 * Math.PI;

                if (i < GameVariables.NumAsteroids1)
                {
                    if (random.Next(2) == 0)
                    {
                        xStart = (float)-GameVariables.PlayfieldSizeX;
                    }
                    else
                    {
                        xStart = (float)GameVariables.PlayfieldSizeX;
                    }
                    yStart = (float)random.NextDouble() * GameVariables.PlayfieldSizeY;
                    asteroid1List[i].position = new Vector3(xStart, yStart, 0.0f);
                    asteroid1List[i].direction.X = -(float)Math.Sin(angle);
                    asteroid1List[i].direction.Y = (float)Math.Cos(angle);
                    asteroid1List[i].speed = GameVariables.AsteroidMinSpeed +
                       (float)random.NextDouble() * GameVariables.AsteroidMaxSpeed +
                       asteroidSpeed;
                    asteroid1List[i].isActive = true;
                }

                if (random.Next(2) == 0)
                {
                    xStart = (float)-GameVariables.PlayfieldSizeX;
                }
                else
                {
                    xStart = (float)GameVariables.PlayfieldSizeX;
                }
                yStart = (float)random.NextDouble() * GameVariables.PlayfieldSizeY;
                asteroid2List[i].position = new Vector3(xStart, yStart, 0.0f);
                asteroid2List[i].direction.X = -(float)Math.Sin(angle);
                asteroid2List[i].direction.Y = (float)Math.Cos(angle);
                asteroid2List[i].speed = GameVariables.AsteroidMinSpeed +
                   (float)random.NextDouble() * GameVariables.AsteroidMaxSpeed +
                   asteroidSpeed;
                asteroid2List[i].isActive = true;
            }
        }

        private void shipRadius(Player p, float factor)
        {
            if (p.Shield > 0)
            {
                p.Radius = shieldModel.Meshes[0].BoundingSphere.Radius * factor;
            }
            else
                p.Radius = p.ship.Model.Meshes[0].BoundingSphere.Radius
                        * GameVariables.shipBoundingSphereScale;
        }

        private void zoomCamera(float newCameraHeight)
        {
            if (GameVariables._3D)
            {
                camera.SetTranslation(new Vector3(0.0f, -newCameraHeight / 3,
                    newCameraHeight / 1000));
            }
            else
            {
                camera.SetTranslation(new Vector3(0.0f, 0.0f, newCameraHeight));
            }
        }

        #endregion

        #region Input Handling

        new bool IsActive
        {
            get
            {
                // Pause behavior.
                return base.IsActive;
            }
        }

        ///<summary>
        ///Lets the game respond to p input. Unlike the Update method,
        ///this will only be called when the gameplay screen is active.
        ///</summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.PauseGame)
            {
                soundHandler.PauseMusic();

                if (p.lives == 0)
                {
                    LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(),
                        new MainMenuScreen());
                }
                else
                {
                    //If they pressed pause, bring up the pause menu screen.
                    ScreenManager.AddScreen(new PauseMenuScreen(null));
                }
            }
            else
            {
                soundHandler.ResumeMusic();

                //did we activated the atom bomb?
                if (p.ship.IsActive && (input.IsNewKeyPress(Keys.C)
                    || input.IsMouseMiddleButtonPressed())
                    && (p.AtomBombs > 0))
                {
                    for (int i = 0; i < asteroid2List.Length; i++)
                    {
                        if (i < asteroid1List.Length)
                        {
                            if (asteroid1List[i].isActive)
                            {
                                asteroid1List[i].isActive = false;
                            }
                        }

                        if (asteroid2List[i].isActive)
                        {
                            asteroid2List[i].isActive = false;
                        }
                    }

                    activeAsteroids = 0;
                    p.AtomBombs -= 1;

                    //The x placement of the emitter.
                    explosionX = 400;

                    //The y placement of the emitter
                    explosionY = 300;

                    particles.particlesCreate(explosion, explosionX, explosionY, 
                        random.Next(50, 75), 2); // Make the particles.*/

                    // Make a sound when we fire.
                    soundHandler.PlayCue("explosion4");
                }

                particles.particlesUpdate(explosion, 2);

                //Move to object back to original state if Q key pressed
                if (!p.ship.IsActive && input.IsNewKeyPress(Keys.Q))
                {
                    if (p.lives != 0)
                    {
                        p.WarpToCenter();

                        //Make a sound when we warp.
                        soundHandler.PlayCue("hyperspace_activate");
                    }
                }

                //are we shooting bullets?
                if (p.ship.IsActive && (input.IsNewKeyPress(Keys.Z)
                    || input.IsMouseLeftButtonPressed()))
                {
                    if (p.ShootBullet())
                    {
                        // Make a sound when we fire.
                        soundHandler.PlayCue("tx0_fire1");
                    }
                }

                //are we shooting rockets?
                if (p.ship.IsActive && (input.IsNewKeyPress(Keys.X)
                    || input.IsMouseRightButtonPressed()))
                {
                    if (p.ShootRocket())
                    {
                        // Make a sound when we fire.
                        soundHandler.PlayCue("hax2_fire");
                    }
                }

                //Rotate to the viwer left while the Left key down
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.Left,
                    PlayerIndex.One))
                {
                    p.ship.Rotation += 0.10f;
                    soundHandler.PlayMyEngineSound();
                }

                //Rotate to the viewer right while the Right key down
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.Right,
                    PlayerIndex.One))
                {
                    p.ship.Rotation -= 0.10f;
                    soundHandler.PlayMyEngineSound();
                }

                //Move Up while the Up key is pressed
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.Up,
                    PlayerIndex.One))
                {
                    //Create some velocity if the Up key is down.
                    Vector3 modelVelocityAdd = Vector3.Zero;

                    //Find out what direction we should be thrusting, using rotation.
                    modelVelocityAdd.X = -(float)Math.Sin(p.ship.Rotation);
                    modelVelocityAdd.Y = +(float)Math.Cos(p.ship.Rotation);

                    //Now scale our direction by pressing this key
                    modelVelocityAdd *= 6;

                    //Finally, add this vector to our velocity.
                    p.ship.Update(modelVelocityAdd);

                    soundHandler.PlayMyEngineSound();
                }

                //Move Down while the Down key is pressed
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.Down,
                    PlayerIndex.One))
                {
                    //Create some velocity if the Down key is down.
                    Vector3 modelVelocityAdd = Vector3.Zero;

                    //Find out what direction we should be thrusting, using rotation.
                    modelVelocityAdd.X = +(float)Math.Sin(p.ship.Rotation);
                    modelVelocityAdd.Y = -(float)Math.Cos(p.ship.Rotation);

                    //Now scale our direction by pressing this key
                    modelVelocityAdd *= 6;

                    //Finally, add this vector to our velocity.
                    p.ship.Update(modelVelocityAdd);

                    soundHandler.PlayMyEngineSound();
                }

                if (p.ship.IsActive && ((input.VariationX() != 0) || 
                    (input.VariationY() != 0)))
                {
                    if (input.MouseMoved)
                    {
                        Vector3 modelVelocityAdd = Vector3.Zero;

                        modelVelocityAdd.X = -input.VariationX() * 3.1f;
                        modelVelocityAdd.Y = input.VariationY() * 3.1f;

                        p.ship.Update(modelVelocityAdd);
                        soundHandler.PlayMyEngineSound();
                    }
                    else
                        input.MouseMoved = true;
                }

                if (!input.IsNewKeyStillPress(Keys.Up, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.Down, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.Left, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.Right, PlayerIndex.One)
                    && input.VariationX() == 0
                    && input.VariationY() == 0)
                {
                    ////The x placement of the emitter.
                    //shipTrailX = (((p.ship.Position.X + (GameVariables.PlayfieldSizeX / 
                    //    2.0f)) / (GameVariables.PlayfieldSizeX)) * (10.0f + 
                    //    ((float)GraphicsDeviceManager.DefaultBackBufferWidth / 2.0f))) + 
                    //    195.0f;

                    ////The y placement of the emitter.
                    //shipTrailY = (((60.0f + 
                    //    ((float)GraphicsDeviceManager.DefaultBackBufferHeight / 2.0f)) -
                    //   (((p.ship.Position.Y + (GameVariables.PlayfieldSizeY / 2.0f)) /
                    //   (GameVariables.PlayfieldSizeY)) * (60.0f + 
                    //   ((float)GraphicsDeviceManager.DefaultBackBufferHeight / 2.0f))))) + 
                    //   145.0f;
                    
                    //particles.particlesCreate(shipTrail, shipTrailX, shipTrailY, 
                    //    random.Next(1, 2), 1);// Make the particles.
                    
                    soundHandler.PauseMyEngineSound();
                }

                //particles.particlesUpdate(shipTrail, 1);

                if (input.IsNewKeyPress(Keys.F1))
                {
                    camera.Position = new Vector3(0.0f, 0.0f, GameVariables.CameraHeight);

                    GameVariables._3D = false;
                }

                if (input.IsNewKeyPress(Keys.F2))
                {
                    camera.Position = new Vector3(0.0f, -GameVariables.CameraHeight / 3,
                        GameVariables.CameraHeight / 10000);

                    GameVariables._3D = true;
                }

                //ZOOM IN if E key is pressed
                if (input.IsNewKeyStillPress(Keys.E, PlayerIndex.One))
                {
                    GameVariables.PlayfieldSizeX -= 200;
                    GameVariables.PlayfieldSizeY -= 200;

                    zoomCamera(-500);
                }

                //ZOOM OUT if W key is pressed
                if (input.IsNewKeyStillPress(Keys.W, PlayerIndex.One))
                {
                    GameVariables.PlayfieldSizeX += 200;
                    GameVariables.PlayfieldSizeY += 200;

                    zoomCamera(+500);
                }
            }
        }

        #endregion

        #region Update and Draw

        //<summary>
        //Updates the state of the game.
        //</summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                p.Update(gameTime);

                //Play gameOver if we are 100% dead
                if (p.lives == 0)
                {
                    soundHandler.PlayGameOver();
                }
                else
                {
                    soundHandler.Update(gameTime);
                }

                if (GameVariables.ScrollingBackgroundIsActive)
                    myBackground.Update(gameTime);

                for (int i = 0; i < GameVariables.NumAsteroids2; i++)
                {
                    if (i < GameVariables.NumAsteroids1)
                    {
                        asteroid1List[i].Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    }

                    asteroid2List[i].Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }

                if ((collisionsHandler.CheckForBulletAsteroidCollision(
                    bulletModel.Meshes[0].BoundingSphere.Radius,
                    asteroidModels[0].Meshes[0].BoundingSphere.Radius,
                    p, asteroid1List)) || 
                    (collisionsHandler.CheckForBulletAsteroidCollision(
                    bulletModel.Meshes[0].BoundingSphere.Radius,
                    asteroidModels[1].Meshes[0].BoundingSphere.Radius,
                    p, asteroid2List)))
                {
                    activeAsteroids -= 1;

                    //The x placement of the emitter.
                    explosionX = (((collisionsHandler.explosionPosition.X +
                        (GameVariables.PlayfieldSizeX / 2.0f)) /
                        (GameVariables.PlayfieldSizeX)) * (10.0f +
                        ((float)GraphicsDeviceManager.DefaultBackBufferWidth / 2.0f))) +
                        175.0f;

                    //The y placement of the emitter.
                    explosionY = (((60.0f +
                        ((float)GraphicsDeviceManager.DefaultBackBufferHeight / 2.0f)) -
                        (((collisionsHandler.explosionPosition.Y
                        + (GameVariables.PlayfieldSizeY / 2.0f)) /
                        (GameVariables.PlayfieldSizeY)) *
                        (60.0f + ((float)GraphicsDeviceManager.DefaultBackBufferHeight /
                        2.0f))))) + 120.0f;

                    particles.particlesCreate(explosion, explosionX, explosionY,
                        random.Next(5, 8), 2);

                    soundHandler.PlayCue("explosion2");
                }

                destroyedAsteroids += collisionsHandler.CheckForRocketAsteroidCollision(
                    rocketModel.Meshes[0].BoundingSphere.Radius,
                    asteroidModels[0].Meshes[0].BoundingSphere.Radius,
                    asteroidModels[1].Meshes[0].BoundingSphere.Radius,
                    p, asteroid1List, asteroid2List);

                destroyedAsteroids += collisionsHandler.CheckForRocketAsteroidCollision(
                    rocketModel.Meshes[0].BoundingSphere.Radius,
                    asteroidModels[1].Meshes[0].BoundingSphere.Radius,
                    asteroidModels[0].Meshes[0].BoundingSphere.Radius,
                    p, asteroid2List, asteroid1List);

                if (destroyedAsteroids != 0)
                {
                    activeAsteroids -= destroyedAsteroids;

                    //The x placement of the emitter.
                    explosionX = (((collisionsHandler.explosionPosition.X + 
                        (GameVariables.PlayfieldSizeX / 2.0f)) / 
                        (GameVariables.PlayfieldSizeX)) * (30.0f + 
                        ((float)GraphicsDeviceManager.DefaultBackBufferWidth / 2.0f))) + 
                        175.0f;

                    //The y placement of the emitter.
                    explosionY = (((65.0f + 
                        ((float)GraphicsDeviceManager.DefaultBackBufferHeight / 2.0f)) -
                       (((collisionsHandler.explosionPosition.Y + 
                       (GameVariables.PlayfieldSizeY / 2.0f)) /
                       (GameVariables.PlayfieldSizeY)) *
                       (65.0f + ((float)GraphicsDeviceManager.DefaultBackBufferHeight / 
                       2.0f))))) + 120.0f;

                    particles.particlesCreate(explosion, explosionX, explosionY, 
                        random.Next(10, 15), 2);

                    soundHandler.PlayCue("explosion2");
                }

                particles.particlesUpdate(explosion, 2);

                destroyedAsteroids = 0;

                //recalculate the ship radius
                shipRadius(p, 80);

                if (collisionsHandler.CheckForShipAsteroidCollision(
                    asteroidModels[0].Meshes[0].BoundingSphere.Radius,
                    asteroidModels[1].Meshes[0].BoundingSphere.Radius,
                    asteroid1List, asteroid2List, p))
                {
                    activeAsteroids -= 1;

                    if (p.Shield < 0)
                    {
                        //The x placement of the emitter.
                        explosionX = (((p.ship.Position.X + (GameVariables.PlayfieldSizeX / 2.0f)) /
                            (GameVariables.PlayfieldSizeX)) * (10.0f +
                            ((float)GraphicsDeviceManager.DefaultBackBufferWidth / 2.0f))) +
                            195.0f;

                        //The y placement of the emitter.
                        explosionY = (((60.0f +
                            ((float)GraphicsDeviceManager.DefaultBackBufferHeight / 2.0f)) -
                            (((p.ship.Position.Y + (GameVariables.PlayfieldSizeY / 2.0f)) /
                           (GameVariables.PlayfieldSizeY)) * (60.0f +
                           ((float)GraphicsDeviceManager.DefaultBackBufferHeight / 2.0f))))) + 145.0f;

                        particles.particlesCreate(explosion, explosionX, explosionY,
                            random.Next(16, 18), 2);

                        soundHandler.PlayCue("explosion3");
                    }
                    else
                        soundHandler.PlayCue("phase_collide_ship1");
                }

                particles.particlesUpdate(explosion, 2);

                collisionsHandler.CheckForAsteroidCollision(
                    asteroidModels[0].Meshes[0].BoundingSphere.Radius, asteroid1List,
                    asteroidModels[1].Meshes[0].BoundingSphere.Radius, asteroid2List);

                if (activeAsteroids == 0)
                {
                    activeAsteroids = GameVariables.NumAsteroids1 +
                        GameVariables.NumAsteroids2;
                    asteroidSpeed += GameVariables.AsteroidSpeedIncrement;
                    ResetAsteroids();
                }
            }

            camera.Update();

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region Draw, Draw Player, Draw Shooting and Draw Model

        public override void Draw(GameTime gameTime)
        {
            Vector2 titlePosition = new Vector2(400, 300);
            Vector2 titleOrigin = font.MeasureString(Resources.GameOver) / 2;
            Color titleColor = new Color(139, 0, 0, TransitionAlpha);

            float titleScale = 2.0f;

            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, 
                SaveStateMode.None);

            if (GameVariables.ScrollingBackgroundIsActive)
                myBackground.Draw(spriteBatch);
            else
            {
                Vector2 position = new Vector2(
                    (p.ship.Position.X + GameVariables.PlayfieldSizeX / 2) /
                    GameVariables.PlayfieldSizeX + 1,
                    (p.ship.Position.Y + GameVariables.PlayfieldSizeY / 2) /
                    GameVariables.PlayfieldSizeY - 2);

                position.X *= -100;
                position.Y *= 75;

                if (GameVariables.CurrentLevel == 1)
                {
                    spriteBatch.Draw(space1, position, Color.White);
                }

                if (GameVariables.CurrentLevel == 2)
                {
                    spriteBatch.Draw(space2, position, Color.White);
                }

                if (GameVariables.CurrentLevel == 3)
                {
                    spriteBatch.Draw(space3, position, Color.White);
                }

                if (GameVariables.CurrentLevel == 4)
                {
                    spriteBatch.Draw(space4, position, Color.White);
                }

                if (GameVariables.CurrentLevel > 4)
                {
                    spriteBatch.Draw(space5, position, Color.White);
                }
            }

            spriteBatch.End();

            for (int i = 0; i < GameVariables.NumAsteroids2; i++)
            {
                Matrix asteroidTransform;

                if (i < GameVariables.NumAsteroids1)
                {
                    asteroidTransform = Matrix.CreateTranslation(asteroid1List[i].position);
                    if (asteroid1List[i].isActive)
                    {
                        DrawModel(asteroidModels[0], asteroidTransform, asteroid1Transforms);
                    }
                }

                asteroidTransform = Matrix.CreateTranslation(asteroid2List[i].position);
                if (asteroid2List[i].isActive)
                {
                    DrawModel(asteroidModels[1], asteroidTransform, asteroid2Transforms);
                }
            }

            DrawShooting(p);
            DrawPlayer(p);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

            spriteBatch.DrawString(lucidaConsole, "Score: " + p.Score, scorePosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "Level: " + GameVariables.CurrentLevel, levelPosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "Lives: " + p.lives, remainingLivesPosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "Asteroids: " + activeAsteroids, remainingAsteroidPosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "AtomBombs: " + p.AtomBombs, remainingAtomBombsPosition, Color.LightGreen);

            if (p.Shield < 0)
                spriteBatch.DrawString(lucidaConsole, "Shield: 0%", shieldPosition, Color.LightGreen);
            else
                spriteBatch.DrawString(lucidaConsole, "Shield: " + p.Shield + "%", shieldPosition, Color.LightGreen);

            spriteBatch.End();

            spriteBatch.Begin();

            if (p.lives == 0)
            {
                //Make the menu slide into place during transitions, using a
                //power curve to make things look more interesting (this makes
                //the movement slow down as it nears the end).
                float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

                //Draw the menu title.
                titlePosition.Y -= transitionOffset * 100;

                spriteBatch.DrawString(font, Resources.GameOver, titlePosition, 
                    titleColor, 0, titleOrigin, titleScale, SpriteEffects.None, 0);
            }



            /*ship trail*/
            //particles.drawParticles(gameTime, ScreenManager.SpriteBatch, rasto, 1, 
            //particlesTexture);
            /*Explosions*/
            particles.drawParticles(gameTime, ScreenManager.SpriteBatch, explosion, 2, 
                particlesTexture);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawPlayer(Player p)
        {
            Matrix shipTransformMatrix = p.ship.RotationMatrix
                * Matrix.CreateTranslation(p.ship.Position);
            Matrix shieldTransform = shipTransformMatrix;
            
            if (p.ship.IsActive)
            {
                spriteBatch.Begin(SpriteBlendMode.None,
                        SpriteSortMode.Immediate, SaveStateMode.None);
                DrawModel(p.ship.Model, shipTransformMatrix, p.ship.Transforms);
                spriteBatch.End();

                if (p.Shield > 0)
                {
                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend,
                        SpriteSortMode.Immediate, SaveStateMode.None);
                    DrawModel(shieldModel, shieldTransform, shieldTransforms);
                    spriteBatch.End();
                }
            }
        }

        public void DrawShooting(Player p)
        {
            for (int i = 0; i < GameVariables.NumBullets; i++)
            {
                if (p.bulletList[i].IsActive)
                {
                    Matrix bulletTransform = Matrix.CreateTranslation(p.bulletList[i].position);
                    DrawModel(bulletModel, bulletTransform, bulletTransforms);
                }
            }

            for (int i = 0; i < GameVariables.NumRockets; i++)
            {
                Matrix rocketTransform =
                    p.rocketList[i].rotation *
                    Matrix.CreateRotationZ(MathHelper.Pi) *
                    Matrix.CreateTranslation(p.rocketList[i].position);

                if (p.rocketList[i].IsActive)
                {
                    DrawModel(rocketModel, rocketTransform, rocketTransforms);
                }
            }
        }

        public void DrawModel(Model model, Matrix modelTransform, Matrix[] absoluteBoneTransforms)
        {
            //Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * modelTransform;
                    effect.View = camera.View;
                    camera.SetProjection(GameVariables.CameraHeight * 100.0f);
                    effect.Projection = camera.Projection;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        #endregion

        #endregion
    }
}