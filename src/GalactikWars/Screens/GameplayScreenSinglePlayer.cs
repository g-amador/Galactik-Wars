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
using GalactikWars.Entities;
using GalactikWars.Handlers;
using GalactikWars.Particles;
using GalactikWars.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
#endregion

namespace GalactikWars.Screens
{
    ///<summary>
    ///This screen implements the actual game logic. It is just a
    ///placeholder to get the idea across: you'll probably want to
    ///put some more interesting gameplay in here!
    ///</summary>
    internal class GameplayScreenSinglePlayer : GameScreen
    {
        #region Fields

        private ContentManager content = null;

        //Objects to do sound/keyboard/Camera/Collision handling
        private readonly CollisionsHandler collisionsHandler = new();
        private SoundHandler soundHandler => ScreenManager.SoundHandler;
        private readonly CCamera camera = new();

        //asteroid variables
        private readonly Asteroid[] asteroid1List = new Asteroid[GameVariables.NumAsteroids1];
        private readonly Asteroid[] asteroid2List = new Asteroid[GameVariables.NumAsteroid2Slots];
        private int activeAsteroids = GameVariables.NumAsteroids1 + GameVariables.NumAsteroids2;
        private int asteroidSpeed = 0;

        //death/respawn timers
        private float respawnTimer = 0.0f;
        private float gameOverTimer = 0.0f;
        private bool returnedToMenu = false;

        //Visual components
        private ScrollingBackground myBackground;
        private Texture2D space1 = null;
        private Texture2D space2 = null;
        private Texture2D space3 = null;
        private Texture2D space4 = null;
        private Texture2D space5 = null;
        private Texture2D particlesTexture = null;

        private Model rocketModel = null;
        private Model bulletModel = null;
        private Model shieldModel = null;
        private readonly Model[] asteroidModels = null;
        private Matrix[] shieldTransforms = null;
        private Matrix[] bulletTransforms = null;
        private Matrix[] rocketTransforms = null;
        private Matrix[] asteroid1Transforms = null;
        private Matrix[] asteroid2Transforms = null;

        private SpriteBatch spriteBatch = null;
        private SpriteFont lucidaConsole = null;
        private SpriteFont font = null;

        //Player
        private readonly Player p = new();

        //red enemy ships and the blue ally escort (phase 3)
        private readonly List<EnemyShip> enemyShips = [];
        private readonly EnemyShip allyShip = new();
        private Model enemyShipModel = null;
        private Model allyShipModel = null;
        private Matrix[] enemyShipTransforms = null;
        private Matrix[] allyShipTransforms = null;
        private float enemyRespawnTimer = 0.0f;
        private float allyRespawnTimer = 0.0f;

        //team colors: the ally wears the player's colors, the enemies the
        //opposite color (set from the player's hull in LoadContent)
        private Vector3 playerTeamColor = new(0.15f, 0.30f, 0.70f);
        private Vector3 enemyTeamColor = new(0.70f, 0.12f, 0.12f);

        private Vector2 scorePosition = new(60, 110);
        private Vector2 shieldPosition = new(60, 90);
        private Vector2 remainingLivesPosition = new(60, 70);
        private Vector2 remainingAtomBombsPosition = new(60, 50);
        private Vector2 remainingAsteroidPosition = new(60, 30);
        private Vector2 levelPosition = new(60, 10);
        private Vector2 remainingRocketsPosition = new(60, 130);
        private Vector2 enemiesPosition = new(60, 150);

        private readonly Random random = new();

        private float shipTrailX, shipTrailY;
        private float explosionX, explosionY;
        private readonly ParticleSystem particles = new();
        private readonly List<Particle> shipTrail = [];
        private readonly List<Particle> explosion = [];

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
                    //brighter ambient light so the models are not so dark
                    effect.AmbientLightColor = new Vector3(0.55f, 0.55f, 0.55f);
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
            content ??= new ContentManager(ScreenManager.Game.Services, "assets");

            soundHandler.Load(content);
            soundHandler.PlayTrack(SoundHandler.BattleTrack);

            string shipModelName;

            if (random.NextDouble() <= 0.17)
            {
                shipModelName = "models/p1_pencil";
            }
            else if (random.NextDouble() <= 0.34)
            {
                shipModelName = "models/p2_pencil";
            }
            else if (random.NextDouble() <= 0.51)
            {
                shipModelName = "models/p1_wedge";
            }
            else if (random.NextDouble() <= 0.68)
            {
                shipModelName = "models/p2_wedge";
            }
            else if (random.NextDouble() <= 0.85)
            {
                shipModelName = "models/p1_saucer";
            }
            else
            {
                shipModelName = "models/p2_saucer";
            }

            p.ship.Model = content.Load<Model>(shipModelName);

            //team colors: the p1 hulls are the blue team, the p2 hulls the
            //red team — the ally wears the player's colors and the enemies
            //the opposite color
            if (shipModelName.Contains("p1", StringComparison.Ordinal))
            {
                playerTeamColor = new Vector3(0.15f, 0.30f, 0.70f);   //blue
                enemyTeamColor = new Vector3(0.70f, 0.12f, 0.12f);    //red
            }
            else
            {
                playerTeamColor = new Vector3(0.70f, 0.12f, 0.12f);   //red
                enemyTeamColor = new Vector3(0.15f, 0.30f, 0.70f);    //blue
            }

            p.ship.Transforms = SetupEffectDefaults(p.ship.Model);
            p.ship.IsActive = true;

            asteroidModels[0] = content.Load<Model>("models/asteroid1");
            asteroid1Transforms = SetupEffectDefaults(asteroidModels[0]);
            asteroidModels[1] = content.Load<Model>("models/asteroid2");
            asteroid2Transforms = SetupEffectDefaults(asteroidModels[1]);
            bulletModel = content.Load<Model>("models/pea_proj");
            bulletTransforms = SetupEffectDefaults(bulletModel);
            rocketModel = content.Load<Model>("models/p1_rocket");
            rocketTransforms = SetupEffectDefaults(rocketModel);
            shieldModel = content.Load<Model>("models/sphere");
            shieldTransforms = SetupEffectDefaults(shieldModel);

            //activate the static background or the dinamic "myBackground"
            if (GameVariables.ScrollingBackgroundIsActive)
            {
                myBackground = new ScrollingBackground();
                space1 = content.Load<Texture2D>("textures/starfield");
                myBackground.LoadContent(ScreenManager.GraphicsDevice, space1);
            }
            else
            {
                space1 = content.Load<Texture2D>("textures/galaxy");
                space2 = content.Load<Texture2D>("textures/aurora");
                space3 = content.Load<Texture2D>("textures/nebula");
                space4 = content.Load<Texture2D>("textures/nebula2");
                space5 = content.Load<Texture2D>("textures/planets");
            }

            particlesTexture = content.Load<Texture2D>("textures/multicolored");

            lucidaConsole = content.Load<SpriteFont>("fonts/LucidaConsole");
            spriteBatch = ScreenManager.SpriteBatch;
            font = ScreenManager.Font;

            //red enemy ships and the blue ally escort (phase 3)
            enemyShipModel = content.Load<Model>("models/p1_pencil");
            enemyShipTransforms = SetupEffectDefaults(enemyShipModel);
            allyShipModel = content.Load<Model>("models/p1_saucer");
            allyShipTransforms = SetupEffectDefaults(allyShipModel);

            //the starting red squadron and the blue escort
            for (int i = 0; i < GameVariables.NumEnemyShips; i++)
            {
                SpawnEnemyShip();
            }

            SpawnAllyShip();
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
            {
                myBackground.UnLoadContent();
            }

            if (space1 != null)
            {
                space1.Dispose();
            }

            if (space2 != null)
            {
                space2.Dispose();
            }

            if (space3 != null)
            {
                space3.Dispose();
            }

            if (space4 != null)
            {
                space4.Dispose();
            }

            if (space5 != null)
            {
                space5.Dispose();
            }

            if (particlesTexture != null)
            {
                particlesTexture.Dispose();
            }

            content.Unload();
        }

        #endregion

        #region Auxiliar Functions

        private void ResetAsteroids()
        {
            float xStart;
            float yStart;

            if (GameVariables.NumAsteroids1 + GameVariables.NumAsteroids2 != 0)
            {
                GameVariables.CurrentLevel += 1;
            }

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
            {
                p.Radius = p.ship.Model.Meshes[0].BoundingSphere.Radius
                        * GameVariables.shipBoundingSphereScale;
            }
        }

        ///<summary>
        ///Spawns a red enemy ship at a random spot comfortably away from the
        ///player.
        ///</summary>
        private void SpawnEnemyShip()
        {
            EnemyShip enemy = new()
            {
                IsAlly = false,
                IsActive = true,
                Model = enemyShipModel,
                Transforms = enemyShipTransforms,
                Shield = GameVariables.Shield,
                //the enemies wear the opposite color of the player's team
                Tint = enemyTeamColor,
                //stagger the opening shots between ships
                fireTimer = 1.5f + (float)random.NextDouble()
            };

            do
            {
                enemy.Position = new Vector3(
                    (float)(random.NextDouble() * 2 - 1)
                        * GameVariables.PlayfieldSizeX * 0.9f,
                    (float)(random.NextDouble() * 2 - 1)
                        * GameVariables.PlayfieldSizeY * 0.9f,
                    0.0f);
            }
            while (Vector3.DistanceSquared(enemy.Position, p.ship.Position)
                < GameVariables.EnemySpawnDistance
                    * GameVariables.EnemySpawnDistance);

            enemy.Radius = enemyShipModel.Meshes[0].BoundingSphere.Radius
                * GameVariables.shipBoundingSphereScale * 2;

            enemyShips.Add(enemy);
        }

        ///<summary>
        ///Spawns (or respawns) the blue ally escort near the player.
        ///</summary>
        private void SpawnAllyShip()
        {
            allyShip.IsAlly = true;
            allyShip.IsActive = true;
            allyShip.Model = allyShipModel;
            allyShip.Transforms = allyShipTransforms;
            allyShip.Shield = GameVariables.Shield;
            //the ally wears the player's team colors
            allyShip.Tint = playerTeamColor;
            allyShip.fireTimer = 1.0f;
            allyShip.Radius = allyShipModel.Meshes[0].BoundingSphere.Radius
                * GameVariables.shipBoundingSphereScale * 2;
            allyShip.Velocity = Vector3.Zero;
            allyShip.Position = p.ship.Position + new Vector3(3000.0f, -3000.0f, 0.0f);
            allyShip.Rotation = p.ship.Rotation;

            allyRespawnTimer = 0.0f;
        }

        ///<summary>
        ///Steers the red enemies and the blue ally: enemies close in on the
        ///player with their guns locked on, while the ally escorts the
        ///player and shoots at the nearest enemy.
        ///</summary>
        private void SteerShips(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (EnemyShip enemy in enemyShips)
            {
                if (!enemy.IsActive || enemy.IsAlly)
                {
                    continue;
                }

                if (!p.ship.IsActive)
                {
                    continue;   //the player is down: the squadron drifts
                }

                Vector3 toPlayer = p.ship.Position - enemy.Position;
                float distance = toPlayer.Length();

                if (distance < 0.001f)
                {
                    continue;
                }

                //turn the nose towards the player (the nose is the
                //matrix-backward direction, like the player's ship)
                float desired = (float)Math.Atan2(toPlayer.X, -toPlayer.Y);
                float diff = desired - enemy.Rotation;

                while (diff > MathHelper.Pi)
                {
                    diff -= MathHelper.TwoPi;
                }

                while (diff < -MathHelper.Pi)
                {
                    diff += MathHelper.TwoPi;
                }

                enemy.Rotation += MathHelper.Clamp(diff,
                    -GameVariables.EnemyTurnRate * delta,
                    GameVariables.EnemyTurnRate * delta);

                //close in from afar, ease off inside the standoff ring
                if (distance > GameVariables.EnemyStandoff)
                {
                    enemy.Velocity += Vector3.Normalize(toPlayer)
                        * GameVariables.EnemyThrust * delta;
                }

                //fire along the nose when roughly on target and in range
                if (enemy.fireTimer <= 0.0f && Math.Abs(diff) < 0.35f
                    && distance < GameVariables.EnemyFireRange)
                {
                    if (enemy.ShootBullet())
                    {
                        soundHandler.PlayCue("tx0_fire1");
                    }

                    enemy.fireTimer = 0.9f + (float)random.NextDouble();
                }
            }

            //the blue ally escorts: it flies with the player, guns on enemies
            if (allyShip.IsActive)
            {
                Vector3 toPlayer = p.ship.Position - allyShip.Position;
                float distance = toPlayer.Length();

                if (p.ship.IsActive && distance > GameVariables.AllyStandoff)
                {
                    allyShip.Velocity += Vector3.Normalize(toPlayer)
                        * GameVariables.AllyThrust * delta;

                    //the escort gets the same twin frontal bursts as the
                    //player's ship while it thrusts
                    EmitForwardThrusters(allyShip.Position,
                        allyShip.RotationMatrix,
                        allyShip.Model.Meshes[0].BoundingSphere.Radius);
                }

                //aim at the nearest active enemy and fire when aligned
                float bestDistanceSq = GameVariables.AllyFireRange
                    * GameVariables.AllyFireRange;
                EnemyShip bestTarget = null;

                foreach (EnemyShip enemy in enemyShips)
                {
                    if (enemy.IsActive && !enemy.IsAlly)
                    {
                        float distanceSq = Vector3.DistanceSquared(
                            allyShip.Position, enemy.Position);

                        if (distanceSq < bestDistanceSq)
                        {
                            bestDistanceSq = distanceSq;
                            bestTarget = enemy;
                        }
                    }
                }

                Vector3 aimTarget = (bestTarget != null)
                    ? bestTarget.Position
                    : p.ship.Position;

                Vector3 toAim = aimTarget - allyShip.Position;

                if (toAim.LengthSquared() > 0.001f)
                {
                    float desired = (float)Math.Atan2(toAim.X, -toAim.Y);
                    float diff = desired - allyShip.Rotation;

                    while (diff > MathHelper.Pi)
                    {
                        diff -= MathHelper.TwoPi;
                    }

                    while (diff < -MathHelper.Pi)
                    {
                        diff += MathHelper.TwoPi;
                    }

                    allyShip.Rotation += MathHelper.Clamp(diff,
                        -GameVariables.EnemyTurnRate * delta,
                        GameVariables.EnemyTurnRate * delta);

                    if (bestTarget != null && allyShip.fireTimer <= 0.0f
                        && Math.Abs(diff) < 0.35f)
                    {
                        if (allyShip.ShootBullet())
                        {
                            soundHandler.PlayCue("tx0_fire1");
                        }

                        allyShip.fireTimer = 0.8f + (float)random.NextDouble();
                    }
                }
            }
        }

        ///<summary>
        ///Projects a world position to screen pixels, so particle emitters
        ///land exactly on the object they belong to at any resolution or
        ///zoom level (the old 800x600-era screen math broke on other sizes).
        ///</summary>
        private Vector2 WorldToScreen(Vector3 worldPosition)
        {
            Vector3 screen = ScreenManager.GraphicsDevice.Viewport.Project(
                worldPosition, camera.Projection, camera.View, Matrix.Identity);

            return new Vector2(screen.X, screen.Y);
        }

        ///<summary>
        ///Emits the green jet exhaust: it burns at the tail of the ship, at
        ///a distance that scales with the ship model's own size (ships are
        ///different sizes), and stretches back along the thrust line like a
        ///jet engine flame.
        ///</summary>
        private void EmitExhaust(Player p)
        {
            //the ship on screen
            Vector2 shipScreen = WorldToScreen(p.ship.Position);

            //the tail of the ship in world space: the opposite end of the
            //nose (the nose is RotationMatrix.Backward, where bullets and
            //rockets exit)
            Vector3 worldTail = -p.ship.RotationMatrix.Backward;

            //the exhaust distance follows the ship model's bounding sphere,
            //so the plasma sits at the tail of every hull, not on top of it
            float tailDistance = p.ship.Model.Meshes[0].BoundingSphere.Radius
                * 1.15f;

            Vector2 tailTipScreen = WorldToScreen(
                p.ship.Position + worldTail * tailDistance);

            //the exhaust direction on screen, pointing away from the hull
            Vector2 tailDir = new(
                tailTipScreen.X - shipScreen.X,
                tailTipScreen.Y - shipScreen.Y);

            if (tailDir.LengthSquared() > 0.0001f)
            {
                tailDir.Normalize();
            }

            shipTrailX = tailTipScreen.X;
            shipTrailY = tailTipScreen.Y;

            //the plume stretches about one hull-length behind the tail and
            //scales with zoom, so it reads as a jet engine at any distance
            float plumeLength = Vector2.Distance(shipScreen, tailTipScreen)
                * 1.1f;
            float lateralSpread = plumeLength * 0.35f;

            particles.particlesCreate(shipTrail, shipTrailX, shipTrailY,
                2, 1, tailDir.X, tailDir.Y, plumeLength, lateralSpread); // Make the particles.
        }

        ///<summary>
        ///Fires a reaction-control jet near the head while rotating:
        ///pressing A burns the left-side jet, pressing D the right-side one.
        ///The flame always points away from the hull, pushing the nose into
        ///the turn — like real maneuvering thrusters in space.
        ///</summary>
        private void EmitRcsJet(Player p, int side)
        {
            //the nose of the ship in world space (same end the bullets exit)
            Vector3 worldNose = p.ship.RotationMatrix.Backward;

            //the lateral direction the nose swings towards when rotating
            Vector3 worldSide = new(
                +(float)Math.Cos(p.ship.Rotation),
                +(float)Math.Sin(p.ship.Rotation), 0.0f);

            float headDistance = p.ship.Model.Meshes[0].BoundingSphere.Radius
                * 1.05f;

            //the jet nozzle sits just off the head, on the flame's side
            Vector3 nozzle = p.ship.Position + worldNose * headDistance
                + worldSide * side * headDistance * 0.5f;

            //the flame pushes away from that side, into the turn
            Vector3 flameDir = worldSide * side;

            Vector2 nozzleScreen = WorldToScreen(nozzle);
            Vector2 flameTipScreen = WorldToScreen(
                nozzle + flameDir * headDistance);

            Vector2 plumeDir = new(
                flameTipScreen.X - nozzleScreen.X,
                flameTipScreen.Y - nozzleScreen.Y);

            if (plumeDir.LengthSquared() > 0.0001f)
            {
                plumeDir.Normalize();
            }

            //a shorter puff than the main engine: a thruster, not a burn
            float plumeLength = Vector2.Distance(
                WorldToScreen(p.ship.Position), nozzleScreen) * 0.8f;
            float lateralSpread = plumeLength * 0.35f;

            particles.particlesCreate(shipTrail, nozzleScreen.X, nozzleScreen.Y,
                2, 1, plumeDir.X, plumeDir.Y, plumeLength, lateralSpread);
        }

        private void EmitForwardThrusters(Player p)
        {
            EmitForwardThrusters(p.ship.Position, p.ship.RotationMatrix,
                p.ship.Model.Meshes[0].BoundingSphere.Radius);
        }

        ///<summary>
        ///Forward thrust: two bursts vent from the front of a ship, flanking
        ///the nose and pushing away from the hull — the reaction drives the
        ///ship along its travel direction, like twin nose thrusters.
        ///</summary>
        private void EmitForwardThrusters(Vector3 position,
            Matrix rotationMatrix, float hullRadius)
        {
            //the nose of the ship in world space (same end the bullets exit)
            Vector3 worldNose = rotationMatrix.Backward;

            //the lateral axis, for the twin nozzle offset
            Vector3 worldSide = new(
                rotationMatrix.Backward.Y,
                -rotationMatrix.Backward.X, 0.0f);

            float headDistance = hullRadius * 1.05f;

            //two nozzles flanking the centerline near the nose
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 nozzle = position + worldNose * headDistance
                    + worldSide * side * headDistance * 0.35f;

                //the flame vents forward, away from the hull
                Vector2 nozzleScreen = WorldToScreen(nozzle);
                Vector2 flameTipScreen = WorldToScreen(
                    nozzle + worldNose * headDistance);

                Vector2 plumeDir = new(
                    flameTipScreen.X - nozzleScreen.X,
                    flameTipScreen.Y - nozzleScreen.Y);

                if (plumeDir.LengthSquared() > 0.0001f)
                {
                    plumeDir.Normalize();
                }

                float plumeLength = Vector2.Distance(
                    WorldToScreen(position), nozzleScreen) * 0.85f;
                float lateralSpread = plumeLength * 0.3f;

                particles.particlesCreate(shipTrail, nozzleScreen.X,
                    nozzleScreen.Y, 2, 1, plumeDir.X, plumeDir.Y,
                    plumeLength, lateralSpread);
            }
        }

        private void zoomCamera(float newCameraHeight)
        {
            if (GameVariables._3D)
            {
                //the 3D camera's height lives on its (negative) Y axis
                float currentHeight = -camera.Position.Y * 3.0f;

                newCameraHeight = MathHelper.Clamp(
                    currentHeight + newCameraHeight,
                    GameVariables.MinZoomHeight, GameVariables.MaxZoomHeight)
                    - currentHeight;

                camera.SetTranslation(new Vector3(0.0f, -newCameraHeight / 3,
                    newCameraHeight / 1000));
            }
            else
            {
                //clamp the camera height so fully zoomed views stay sane
                newCameraHeight = MathHelper.Clamp(
                    camera.Position.Z + newCameraHeight,
                    GameVariables.MinZoomHeight, GameVariables.MaxZoomHeight)
                    - camera.Position.Z;

                camera.SetTranslation(new Vector3(0.0f, 0.0f, newCameraHeight));
            }
        }

        ///<summary>
        ///Spawns small asteroids at the given world position (used when a
        ///large asteroid is destroyed and splits in two).
        ///</summary>
        private void SpawnSplitAsteroids(Vector3 position, int count)
        {
            for (int n = 0; n < count; n++)
            {
                //the split-only slots live after the base small asteroids
                for (int i = GameVariables.NumAsteroids2; i < asteroid2List.Length; i++)
                {
                    if (!asteroid2List[i].isActive)
                    {
                        double angle = random.NextDouble() * 2 * Math.PI;

                        asteroid2List[i].position = position;
                        asteroid2List[i].direction.X = -(float)Math.Sin(angle);
                        asteroid2List[i].direction.Y = (float)Math.Cos(angle);
                        asteroid2List[i].speed = GameVariables.AsteroidMinSpeed +
                           (float)random.NextDouble() * GameVariables.AsteroidMaxSpeed +
                           asteroidSpeed;
                        asteroid2List[i].isActive = true;
                        break;
                    }
                }
            }
        }

        ///<summary>
        ///Steers every active rocket towards the nearest active asteroid,
        ///so rockets home in on their target.
        ///</summary>
        private void HomeRockets(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int j = 0; j < p.rocketList.Length; j++)
            {
                if (!p.rocketList[j].IsActive)
                {
                    continue;
                }

                float bestDistanceSq = float.MaxValue;
                Vector3 bestTarget = Vector3.Zero;
                bool targetFound = false;

                for (int i = 0; i < asteroid1List.Length; i++)
                {
                    if (!asteroid1List[i].isActive)
                    {
                        continue;
                    }

                    float distanceSq = Vector3.DistanceSquared(
                        p.rocketList[j].position, asteroid1List[i].position);

                    if (distanceSq < bestDistanceSq)
                    {
                        bestDistanceSq = distanceSq;
                        bestTarget = asteroid1List[i].position;
                        targetFound = true;
                    }
                }

                for (int i = 0; i < asteroid2List.Length; i++)
                {
                    if (!asteroid2List[i].isActive)
                    {
                        continue;
                    }

                    float distanceSq = Vector3.DistanceSquared(
                        p.rocketList[j].position, asteroid2List[i].position);

                    if (distanceSq < bestDistanceSq)
                    {
                        bestDistanceSq = distanceSq;
                        bestTarget = asteroid2List[i].position;
                        targetFound = true;
                    }
                }

                if (targetFound)
                {
                    p.rocketList[j].SteerToward(bestTarget, 3.0f, delta);
                }
            }
        }

        #endregion

        #region Input Handling

        private new bool IsActive
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
            {
                throw new ArgumentNullException("input");
            }

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
                if (p.ship.IsActive && (input.IsNewKeyPress(Keys.B)
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

                    //the blast takes the red enemy ships with it
                    foreach (EnemyShip enemy in enemyShips)
                    {
                        if (enemy.IsActive && !enemy.IsAlly)
                        {
                            enemy.IsActive = false;
                            p.Score += GameVariables.EnemyShipBonus;

                            Vector2 enemyBombScreen = WorldToScreen(
                                enemy.Position);
                            particles.particlesCreate(explosion,
                                enemyBombScreen.X, enemyBombScreen.Y,
                                random.Next(12, 16), 2);
                        }
                    }

                    //the bomb detonates at the world origin (screen centre)
                    Vector2 bombScreen = WorldToScreen(Vector3.Zero);
                    explosionX = bombScreen.X;
                    explosionY = bombScreen.Y;

                    particles.particlesCreate(explosion, explosionX, explosionY,
                        random.Next(50, 75), 2); // Make the particles.*/

                    // Make a sound when we fire.
                    soundHandler.PlayCue("explosion3");
                }

                particles.particlesUpdate(explosion, 2);

                //are we shooting bullets?
                if (p.ship.IsActive && (input.IsNewKeyPress(Keys.Space)
                    || input.IsMouseLeftButtonPressed()))
                {
                    if (p.ShootBullet())
                    {
                        // Make a sound when we fire.
                        soundHandler.PlayCue("tx0_fire1");
                    }
                }

                //are we shooting rockets?
                if (p.ship.IsActive && (input.IsNewKeyPress(Keys.R)
                    || input.IsMouseRightButtonPressed()))
                {
                    if (p.ShootRocket())
                    {
                        // Make a sound when we fire.
                        soundHandler.PlayCue("hax2_fire");
                    }
                }

                //Rotate to the viewer left while the A key is down
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.A,
                    PlayerIndex.One))
                {
                    p.ship.Rotation += 0.10f;
                    soundHandler.PlayMyEngineSound();

                    //reaction-control jet on the left side of the head
                    EmitRcsJet(p, -1);
                }

                //Rotate to the viewer right while the D key is down
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.D,
                    PlayerIndex.One))
                {
                    p.ship.Rotation -= 0.10f;
                    soundHandler.PlayMyEngineSound();

                    //reaction-control jet on the right side of the head
                    EmitRcsJet(p, +1);
                }

                //Move Up while the W key is pressed
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.W,
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

                    //twin thrusters venting from the front of the ship
                    EmitForwardThrusters(p);
                }

                //Move Down while the S key is pressed
                if (p.ship.IsActive && input.IsNewKeyStillPress(Keys.S,
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

                    //green plasma back push
                    EmitExhaust(p);
                }

                if (p.ship.IsActive && !input.IsNewKeyStillPress(Keys.W, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.S, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.A, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.D, PlayerIndex.One))
                {
                    soundHandler.PauseMyEngineSound();
                }

                particles.particlesUpdate(shipTrail, 1);

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
                    //clamp the playfield so it can never collapse to zero
                    GameVariables.PlayfieldSizeX = MathHelper.Clamp(
                        GameVariables.PlayfieldSizeX - 200,
                        GameVariables.MinPlayfieldSize,
                        GameVariables.MaxPlayfieldSize);
                    GameVariables.PlayfieldSizeY = MathHelper.Clamp(
                        GameVariables.PlayfieldSizeY - 200,
                        GameVariables.MinPlayfieldSize,
                        GameVariables.MaxPlayfieldSize);

                    zoomCamera(-500);
                }

                //ZOOM OUT if Q key is pressed
                if (input.IsNewKeyStillPress(Keys.Q, PlayerIndex.One))
                {
                    GameVariables.PlayfieldSizeX = MathHelper.Clamp(
                        GameVariables.PlayfieldSizeX + 200,
                        GameVariables.MinPlayfieldSize,
                        GameVariables.MaxPlayfieldSize);
                    GameVariables.PlayfieldSizeY = MathHelper.Clamp(
                        GameVariables.PlayfieldSizeY + 200,
                        GameVariables.MinPlayfieldSize,
                        GameVariables.MaxPlayfieldSize);

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
                HomeRockets(gameTime);

                //steer the red enemies and the blue ally, then simulate them
                SteerShips(gameTime);

                p.Update(gameTime);

                foreach (EnemyShip enemy in enemyShips)
                {
                    if (enemy.IsActive)
                    {
                        enemy.Update(gameTime);
                    }
                }

                if (allyShip.IsActive)
                {
                    allyShip.Update(gameTime);
                }

                //a destroyed red enemy returns after a short delay, as long
                //as the squadron is below its quota
                enemyRespawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (enemyRespawnTimer >= GameVariables.EnemySpawnSeconds)
                {
                    enemyRespawnTimer = 0.0f;

                    int activeEnemies = 0;

                    foreach (EnemyShip enemy in enemyShips)
                    {
                        if (enemy.IsActive && !enemy.IsAlly)
                        {
                            activeEnemies += 1;
                        }
                    }

                    if (activeEnemies < GameVariables.NumEnemyShips)
                    {
                        SpawnEnemyShip();
                    }
                }

                //the blue ally returns a few seconds after it falls, as long
                //as the player is still flying
                if (!allyShip.IsActive && p.ship.IsActive && p.lives > 0)
                {
                    allyRespawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (allyRespawnTimer >= 3.0f)
                    {
                        SpawnAllyShip();

                        //Make a sound when we warp.
                        soundHandler.PlayCue("hyperspace_activate");
                    }
                }

                //auto respawn: when the ship is destroyed but lives remain,
                //warp back into the fight after a short pause
                if (!p.ship.IsActive && p.lives > 0)
                {
                    respawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (respawnTimer >= 1.0f)
                    {
                        respawnTimer = 0.0f;
                        p.WarpToCenter();

                        //Make a sound when we warp.
                        soundHandler.PlayCue("hyperspace_activate");
                    }
                }

                //Play gameOver if we are 100% dead
                if (p.lives == 0)
                {
                    soundHandler.PlayGameOver();

                    //return to the main menu automatically after a short pause
                    gameOverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (gameOverTimer >= 4.0f && !returnedToMenu)
                    {
                        returnedToMenu = true;
                        LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(),
                            new MainMenuScreen());
                    }
                }

                if (GameVariables.ScrollingBackgroundIsActive)
                {
                    myBackground.Update(gameTime);
                }

                for (int i = 0; i < asteroid2List.Length; i++)
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
                    p, asteroid1List)))
                {
                    //large asteroids are too tough for a bullet: they split in two
                    SpawnSplitAsteroids(new Vector3(
                        collisionsHandler.explosionPosition.X,
                        collisionsHandler.explosionPosition.Y, 0.0f),
                        GameVariables.AsteroidSplitCount);
                    activeAsteroids += 1;

                    //the emitter sits on the explosion, on screen
                    Vector2 explosionScreen = WorldToScreen(new Vector3(
                        collisionsHandler.explosionPosition.X,
                        collisionsHandler.explosionPosition.Y, 0.0f));
                    explosionX = explosionScreen.X;
                    explosionY = explosionScreen.Y;

                    particles.particlesCreate(explosion, explosionX, explosionY,
                        random.Next(5, 8), 2);

                    //a large asteroid breaks into smaller ones
                    soundHandler.PlayCue("explosion4");
                }
                else if (collisionsHandler.CheckForBulletAsteroidCollision(
                    bulletModel.Meshes[0].BoundingSphere.Radius,
                    asteroidModels[1].Meshes[0].BoundingSphere.Radius,
                    p, asteroid2List))
                {
                    activeAsteroids -= 1;

                    //the emitter sits on the explosion, on screen
                    Vector2 explosionScreen = WorldToScreen(new Vector3(
                        collisionsHandler.explosionPosition.X,
                        collisionsHandler.explosionPosition.Y, 0.0f));
                    explosionX = explosionScreen.X;
                    explosionY = explosionScreen.Y;

                    particles.particlesCreate(explosion, explosionX, explosionY,
                        random.Next(5, 8), 2);

                    //a small asteroid is destroyed
                    soundHandler.PlayCue("explosion1");
                }

                int rocketHit = collisionsHandler.CheckForRocketAsteroidCollision(
                    rocketModel.Meshes[0].BoundingSphere.Radius,
                    asteroidModels[0].Meshes[0].BoundingSphere.Radius,
                    asteroidModels[1].Meshes[0].BoundingSphere.Radius,
                    p, asteroid1List, asteroid2List);

                if (rocketHit != 0)
                {
                    if (rocketHit == 2)
                    {
                        //large asteroids are too tough for a rocket: they split in two
                        SpawnSplitAsteroids(new Vector3(
                            collisionsHandler.explosionPosition.X,
                            collisionsHandler.explosionPosition.Y, 0.0f),
                            GameVariables.AsteroidSplitCount);
                        activeAsteroids += 1;
                    }
                    else
                    {
                        activeAsteroids -= 1;
                    }

                    //the emitter sits on the explosion, on screen
                    Vector2 rocketExplosionScreen = WorldToScreen(new Vector3(
                        collisionsHandler.explosionPosition.X,
                        collisionsHandler.explosionPosition.Y, 0.0f));
                    explosionX = rocketExplosionScreen.X;
                    explosionY = rocketExplosionScreen.Y;

                    particles.particlesCreate(explosion, explosionX, explosionY,
                        random.Next(10, 15), 2);

                    if (rocketHit == 2)
                    {
                        //the large asteroid broke into smaller ones
                        soundHandler.PlayCue("explosion4");
                    }
                    else
                    {
                        //the small asteroid was destroyed
                        soundHandler.PlayCue("explosion1");
                    }
                }

                //player and ally fire against the red enemy / blue ally ships
                foreach (EnemyShip enemy in enemyShips)
                {
                    if (!enemy.IsActive || enemy.IsAlly)
                    {
                        continue;
                    }

                    if (collisionsHandler.CheckForBulletEnemyShip(
                        bulletModel.Meshes[0].BoundingSphere.Radius,
                        p, enemy)
                        && !enemy.IsActive)
                    {
                        //the shot destroyed the enemy
                        Vector2 enemyExplosionScreen = WorldToScreen(new Vector3(
                            collisionsHandler.explosionPosition.X,
                            collisionsHandler.explosionPosition.Y, 0.0f));
                        particles.particlesCreate(explosion,
                            enemyExplosionScreen.X, enemyExplosionScreen.Y,
                            random.Next(10, 15), 2);

                        //a red enemy ship was destroyed
                        soundHandler.PlayCue("explosion2");
                    }

                    if (collisionsHandler.CheckForRocketEnemyShip(
                        rocketModel.Meshes[0].BoundingSphere.Radius,
                        p, enemy))
                    {
                        //the rocket destroyed the enemy outright
                        Vector2 enemyRocketScreen = WorldToScreen(new Vector3(
                            collisionsHandler.explosionPosition.X,
                            collisionsHandler.explosionPosition.Y, 0.0f));
                        particles.particlesCreate(explosion,
                            enemyRocketScreen.X, enemyRocketScreen.Y,
                            random.Next(10, 15), 2);

                        soundHandler.PlayCue("explosion2");
                    }
                }

                //the ally's guns count for the player's score
                foreach (EnemyShip enemy in enemyShips)
                {
                    if (!enemy.IsActive || enemy.IsAlly)
                    {
                        continue;
                    }

                    if (collisionsHandler.CheckForAllyBulletEnemyShip(
                        bulletModel.Meshes[0].BoundingSphere.Radius,
                        allyShip, enemy)
                        && !enemy.IsActive)
                    {
                        //the ally shot destroyed the enemy: credit the player
                        p.Score += GameVariables.EnemyShipBonus;

                        Vector2 allyKillScreen = WorldToScreen(new Vector3(
                            collisionsHandler.explosionPosition.X,
                            collisionsHandler.explosionPosition.Y, 0.0f));
                        particles.particlesCreate(explosion,
                            allyKillScreen.X, allyKillScreen.Y,
                            random.Next(10, 15), 2);

                        soundHandler.PlayCue("explosion2");
                    }
                }

                //red enemy fire against the player and the blue ally
                foreach (EnemyShip enemy in enemyShips)
                {
                    if (!enemy.IsActive || enemy.IsAlly)
                    {
                        continue;
                    }

                    if (collisionsHandler.CheckForEnemyBulletShip(
                        bulletModel.Meshes[0].BoundingSphere.Radius,
                        p, enemy))
                    {
                        if (!p.ship.IsActive)
                        {
                            //the player's ship was destroyed
                            Vector2 shipHitScreen = WorldToScreen(new Vector3(
                                collisionsHandler.explosionPosition.X,
                                collisionsHandler.explosionPosition.Y, 0.0f));
                            particles.particlesCreate(explosion,
                                shipHitScreen.X, shipHitScreen.Y,
                                random.Next(16, 18), 2);

                            soundHandler.PlayCue("explosion2");
                        }
                        else
                        {
                            //a meteor-class shield hit
                            soundHandler.PlayCue("phase_collide_ship1");
                        }
                    }

                    if (allyShip.IsActive
                        && collisionsHandler.CheckForEnemyBulletAlly(
                            bulletModel.Meshes[0].BoundingSphere.Radius,
                            allyShip, enemy)
                        && !allyShip.IsActive)
                    {
                        //the blue escort went down
                        Vector2 allyHitScreen = WorldToScreen(new Vector3(
                            collisionsHandler.explosionPosition.X,
                            collisionsHandler.explosionPosition.Y, 0.0f));
                        particles.particlesCreate(explosion,
                            allyHitScreen.X, allyHitScreen.Y,
                            random.Next(12, 16), 2);

                        soundHandler.PlayCue("explosion2");
                    }
                }

                //hull-vs-hull shield collisions: while both shields hold the
                //ships bounce apart and drain; when one shield gives in,
                //that ship blows up
                foreach (EnemyShip enemy in enemyShips)
                {
                    if (!enemy.IsActive || enemy.IsAlly)
                    {
                        continue;
                    }

                    if (p.ship.IsActive
                        && collisionsHandler.CheckForPlayerEnemyShipCollision(
                            p, enemy))
                    {
                        if (p.ship.IsActive && enemy.IsActive)
                        {
                            //both shields held: bounce with a clang
                            soundHandler.PlayCue("shield_collide_ship1");
                        }
                        else
                        {
                            //at least one ship blew up
                            Vector2 ramScreen = WorldToScreen(new Vector3(
                                collisionsHandler.explosionPosition.X,
                                collisionsHandler.explosionPosition.Y, 0.0f));
                            particles.particlesCreate(explosion,
                                ramScreen.X, ramScreen.Y,
                                random.Next(14, 18), 2);

                            soundHandler.PlayCue("explosion2");
                        }
                    }

                    if (allyShip.IsActive
                        && collisionsHandler.CheckForAllyEnemyShipCollision(
                            allyShip, enemy, out bool allyDestroyed,
                            out bool enemyDestroyed))
                    {
                        if (allyDestroyed || enemyDestroyed)
                        {
                            Vector2 allyRamScreen = WorldToScreen(new Vector3(
                                collisionsHandler.explosionPosition.X,
                                collisionsHandler.explosionPosition.Y, 0.0f));
                            particles.particlesCreate(explosion,
                                allyRamScreen.X, allyRamScreen.Y,
                                random.Next(12, 16), 2);

                            if (enemyDestroyed)
                            {
                                //the ally rammed an enemy down: credit the player
                                p.Score += GameVariables.EnemyShipBonus;
                            }

                            soundHandler.PlayCue("explosion2");
                        }
                        else
                        {
                            soundHandler.PlayCue("shield_collide_ship1");
                        }
                    }
                }

                particles.particlesUpdate(explosion, 2);

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
                        //the emitter sits on the destroyed ship, on screen
                        Vector2 shipExplosionScreen = WorldToScreen(
                            p.ship.Position);
                        explosionX = shipExplosionScreen.X;
                        explosionY = shipExplosionScreen.Y;

                        particles.particlesCreate(explosion, explosionX, explosionY,
                            random.Next(16, 18), 2);

                        //the ship and the asteroid are both destroyed
                        soundHandler.PlayCue("explosion2");
                    }
                    else
                    {
                        soundHandler.PlayCue("phase_collide_ship1");
                    }
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
            Vector2 titlePosition = new(400, 300);
            Vector2 titleOrigin = font.MeasureString(Resources.GameOver) / 2;
            Color titleColor = new((byte)139, (byte)0, (byte)0, TransitionAlpha);

            float titleScale = 2.0f;

            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            if (GameVariables.ScrollingBackgroundIsActive)
            {
                myBackground.Draw(spriteBatch);
            }
            else
            {
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

                //stretch the background over the whole screen, with a gentle
                //parallax shift that follows the ship
                float fx = (p.ship.Position.X + GameVariables.PlayfieldSizeX) /
                    (2 * GameVariables.PlayfieldSizeX);
                float fy = (p.ship.Position.Y + GameVariables.PlayfieldSizeY) /
                    (2 * GameVariables.PlayfieldSizeY);
                float marginX = viewport.Width * 0.05f;
                float marginY = viewport.Height * 0.05f;

                Rectangle destination = new(
                    (int)(marginX * (fx - 0.5f)) - (int)marginX,
                    (int)(marginY * (fy - 0.5f)) - (int)marginY,
                    (int)(viewport.Width + 2 * marginX),
                    (int)(viewport.Height + 2 * marginY));

                Texture2D background = space1;

                if (GameVariables.CurrentLevel == 2)
                {
                    background = space2;
                }
                else if (GameVariables.CurrentLevel == 3)
                {
                    background = space3;
                }
                else if (GameVariables.CurrentLevel == 4)
                {
                    background = space4;
                }
                else if (GameVariables.CurrentLevel > 4)
                {
                    background = space5;
                }

                spriteBatch.Draw(background, destination, Color.White);
            }

            spriteBatch.End();

            for (int i = 0; i < asteroid2List.Length; i++)
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

            //red enemy ships and their bullets
            foreach (EnemyShip enemy in enemyShips)
            {
                if (enemy.IsActive)
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                    Matrix enemyTransform = enemy.RotationMatrix
                        * Matrix.CreateTranslation(enemy.Position);
                    DrawModel(enemy.Model, enemyTransform, enemy.Transforms,
                        enemy.Tint);

                    for (int i = 0; i < GameVariables.NumBullets; i++)
                    {
                        if (enemy.bulletList[i].IsActive)
                        {
                            Matrix bulletTransform = Matrix.CreateTranslation(
                                enemy.bulletList[i].position);
                            DrawModel(bulletModel, bulletTransform,
                                bulletTransforms);
                        }
                    }

                    spriteBatch.End();
                }
            }

            //the blue ally escort
            if (allyShip.IsActive)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                Matrix allyTransform = allyShip.RotationMatrix
                    * Matrix.CreateTranslation(allyShip.Position);
                DrawModel(allyShip.Model, allyTransform, allyShip.Transforms,
                    allyShip.Tint);

                for (int i = 0; i < GameVariables.NumBullets; i++)
                {
                    if (allyShip.bulletList[i].IsActive)
                    {
                        Matrix bulletTransform = Matrix.CreateTranslation(
                            allyShip.bulletList[i].position);
                        DrawModel(bulletModel, bulletTransform,
                            bulletTransforms);
                    }
                }

                spriteBatch.End();
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            spriteBatch.DrawString(lucidaConsole, "Score: " + p.Score, scorePosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "Level: " + GameVariables.CurrentLevel, levelPosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "Lives: " + p.lives, remainingLivesPosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "Asteroids: " + activeAsteroids, remainingAsteroidPosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "AtomBombs: " + p.AtomBombs, remainingAtomBombsPosition, Color.LightGreen);
            spriteBatch.DrawString(lucidaConsole, "Rockets: " + p.Rockets, remainingRocketsPosition, Color.LightGreen);

            int activeEnemyCount = 0;

            foreach (EnemyShip enemy in enemyShips)
            {
                if (enemy.IsActive && !enemy.IsAlly)
                {
                    activeEnemyCount += 1;
                }
            }

            spriteBatch.DrawString(lucidaConsole, "Enemies: " + activeEnemyCount,
                enemiesPosition, Color.LightGreen);

            if (p.Shield < 0)
            {
                spriteBatch.DrawString(lucidaConsole, "Shield: 0%", shieldPosition, Color.LightGreen);
            }
            else
            {
                spriteBatch.DrawString(lucidaConsole, "Shield: " + p.Shield + "%", shieldPosition, Color.LightGreen);
            }

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
            particles.drawParticles(gameTime, ScreenManager.SpriteBatch, shipTrail, 1,
                particlesTexture);
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
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                DrawModel(p.ship.Model, shipTransformMatrix, p.ship.Transforms);
                spriteBatch.End();

                if (p.Shield > 0)
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
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

        public void DrawModel(Model model, Matrix modelTransform,
            Matrix[] absoluteBoneTransforms, Vector3? ambientTint = null)
        {
            //Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    //brighter ambient light so the models are not so dark;
                    //a tint (red enemies, blue allies) replaces the neutral
                    //ambient color
                    effect.AmbientLightColor = ambientTint
                        ?? new Vector3(0.55f, 0.55f, 0.55f);
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
