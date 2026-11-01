#region File Description
//-----------------------------------------------------------------------------
//GameplayScreen.cs
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
    class GameplayScreenMultiplayer : GameScreen
    {
        #region Fields

        NetworkSession networkSession;

        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();

        ContentManager content = null;

        //Objects to do sound/keyboard/Camera/Collision handling
        CollisionsHandler collisionsHandler = new CollisionsHandler(); 
        SoundHandler soundHandler = new SoundHandler();
        CCamera camera = new CCamera();

        //Visual components
        Texture2D stars = null;
        Texture2D particlesTexture = null;

        Model rocketModel = null;
        Model bulletModel = null;
        Model shieldModel = null;
        Matrix[] shieldTransforms = null;
        Matrix[] bulletTransforms = null;
        Matrix[] rocketTransforms = null;

        SpriteBatch spriteBatch = null;
        SpriteFont lucidaConsole = null;
        SpriteFont font = null;

        //Players
        Player[] players = new Player[NetworkSessionComponent.MaxGamers];
        int playerID = 0; //this players ID
        int activePlayers = 1; //number of active players
        int sessionPlayers = 1; //number of players in this section
        bool isPaused = false;
        bool idChanged = false;
        bool isMovingUp = false;
        bool isMovingDown = false;
        bool isRotatingLeft = false;
        bool isRotatingRight = false;
        bool isFiringBullets = false;
        bool isFiringRockets = false;

        Vector2 scorePosition = new Vector2(60, 30);
        Vector2 shieldPosition = new Vector2(60, 10);
        Vector2 scoresPosition = new Vector2(300, 0);

        float scoresScale = 1.0f;

        float timeElapsed = 0; //time since a scores last shown
         
        Random random = new Random();

        #endregion

        #region Initialization

        ///<summary>
        ///Constructor.
        ///</summary>
        public GameplayScreenMultiplayer(NetworkSession networkSession)
        {
            this.networkSession = networkSession;

            activePlayers = networkSession.AllGamers.Count;
            sessionPlayers = networkSession.AllGamers.Count;

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            camera.Initialize(new Vector3(0.0f, 0.0f, GameVariables.CameraHeight), 
                Vector3.Zero);
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

        private void LoadPlayerModel(string modelName)
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = new Player();
                players[i].ship.Model = content.Load<Model>(modelName);
                players[i].ship.Transforms = SetupEffectDefaults(players[i].ship.Model);
            }
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
                LoadPlayerModel("Models/p1_pencil");
            }
            else if (random.NextDouble() <= 0.34)
            {
                LoadPlayerModel("Models/p2_pencil");
            }
            else if (random.NextDouble() <= 0.51)
            {
                LoadPlayerModel("Models/p1_wedge");
            }
            else if (random.NextDouble() <= 0.68)
            {
                LoadPlayerModel("Models/p2_wedge");
            }
            else if (random.NextDouble() <= 0.85)
            {
                LoadPlayerModel("Models/p1_saucer");
            }
            else
            {
                LoadPlayerModel("Models/p2_saucer");
            }

            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                if (gamer.IsLocal)
                {
                    playerID = networkSession.AllGamers.IndexOf(gamer);
                }
            }

            bulletModel = content.Load<Model>("Models/pea_proj");
            bulletTransforms = SetupEffectDefaults(bulletModel);
            rocketModel = content.Load<Model>("Models/p1_rocket");
            rocketTransforms = SetupEffectDefaults(rocketModel);
            shieldModel = content.Load<Model>("Models/sphere");
            shieldTransforms = SetupEffectDefaults(shieldModel);

            if (random.NextDouble() <= 0.2)
                stars = content.Load<Texture2D>("Textures/galaxy");
            else if (random.NextDouble() <= 0.4)
                stars = content.Load<Texture2D>("Textures/aurora");
            else if (random.NextDouble() <= 0.6)
                stars = content.Load<Texture2D>("Textures/nebula");
            else if (random.NextDouble() <= 0.8)
                stars = content.Load<Texture2D>("Textures/nebula2");
            else
                stars = content.Load<Texture2D>("Textures/planets");

            particlesTexture = content.Load<Texture2D>("Textures/multicolored");

            lucidaConsole = content.Load<SpriteFont>("Fonts/Lucida Console");
            spriteBatch = ScreenManager.SpriteBatch;
            font = ScreenManager.Font;

            players[playerID].ship.IsActive = true;

            players[0].ship.Position = new Vector3(-GameVariables.PlayfieldSizeX / 2, 0, 0);
            players[1].ship.Position = new Vector3(GameVariables.PlayfieldSizeX / 2, 0, 0);
            players[2].ship.Position = new Vector3(-GameVariables.PlayfieldSizeX / 2, -GameVariables.PlayfieldSizeY / 2, 0);
            players[3].ship.Position = new Vector3(GameVariables.PlayfieldSizeX / 2, GameVariables.PlayfieldSizeY / 2, 0);
            players[4].ship.Position = new Vector3(GameVariables.PlayfieldSizeX / 2, -GameVariables.PlayfieldSizeY / 2, 0);
            players[5].ship.Position = new Vector3(-GameVariables.PlayfieldSizeX / 2, GameVariables.PlayfieldSizeY / 2, 0);
        }

        ///<summary>
        ///Unload graphics content used by the game.
        ///</summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            packetWriter.Flush();

            soundHandler.UnLoad();

            stars.Dispose();
            particlesTexture.Dispose();

            content.Unload();
        }

        #endregion

        #region Networking and Auxiliar Functions

        void ReceiveNetworkData(LocalNetworkGamer gamer, GameTime gameTime)
        {
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                gamer.ReceiveData(packetReader, out sender);

                if (GameVariables.FlawToleranceOn)
                {
                    if (packetReader.ReadBoolean())
                    {
                        switchTwoPlayersData(players[packetReader.ReadInt32()],
                            players[packetReader.ReadInt32()], gameTime);
                    }

                    if (packetReader.ReadBoolean())
                    {
                        for (int i = 0; i < players.Length; i++)
                        {
                            players[i].ship.Position = packetReader.ReadVector3();
                            players[i].ship.Velocity = packetReader.ReadVector3();
                            players[i].ship.IsActive = packetReader.ReadBoolean();
                            players[i].ship.Rotation = packetReader.ReadSingle();
                            players[i].Radius = packetReader.ReadSingle();
                            players[i].Shield = packetReader.ReadInt32();
                            players[i].Score = packetReader.ReadInt32();

                            players[i].Update(gameTime);
                        }
                    }
                }

                players[networkSession.AllGamers.IndexOf(sender)].ship.IsActive = packetReader.ReadBoolean();
                players[networkSession.AllGamers.IndexOf(sender)].Radius = packetReader.ReadSingle();

                shipRadius(players[networkSession.AllGamers.IndexOf(gamer)], 180);

                // are players shooting bullets
                if (packetReader.ReadBoolean())
                {
                    if (players[networkSession.AllGamers.IndexOf(sender)].ShootBullet())
                    {
                        soundHandler.PlayCue("tx0_fire1");
                    }
                }

                // are players shooting rockets
                if (packetReader.ReadBoolean())
                {
                    if (players[networkSession.AllGamers.IndexOf(sender)].ShootRocket())
                    {
                        soundHandler.PlayCue("hax2_fire");
                    }
                }

                bool rotatedLeft = packetReader.ReadBoolean();
                bool rotatedRight = packetReader.ReadBoolean();
                bool movedUp = packetReader.ReadBoolean();
                bool movedDown = packetReader.ReadBoolean();

                if (rotatedLeft)
                {
                    players[networkSession.AllGamers.IndexOf(sender)].ship.Rotation += 0.10f;
                }

                if (rotatedRight)
                {
                    players[networkSession.AllGamers.IndexOf(sender)].ship.Rotation -= 0.10f;
                }

                if (movedUp)
                {
                    //Create some velocity
                    Vector3 modelVelocityAdd = Vector3.Zero;

                    //Find out what direction we should be thrusting, using rotation.
                    modelVelocityAdd.X = -(float)Math.Sin(players[networkSession.AllGamers.IndexOf(sender)].ship.Rotation);
                    modelVelocityAdd.Y = +(float)Math.Cos(players[networkSession.AllGamers.IndexOf(sender)].ship.Rotation);

                    //Now scale our direction by pressing this key
                    modelVelocityAdd *= 6;

                    //Finally, add this vector to our velocity.
                    players[networkSession.AllGamers.IndexOf(sender)].ship.Update(modelVelocityAdd);
                }

                if (movedDown)
                {
                    //Create some velocity if the Down key is down.
                    Vector3 modelVelocityAdd = Vector3.Zero;

                    //Find out what direction we should be thrusting, using rotation.
                    modelVelocityAdd.X = +(float)Math.Sin(players[networkSession.AllGamers.IndexOf(sender)].ship.Rotation);
                    modelVelocityAdd.Y = -(float)Math.Cos(players[networkSession.AllGamers.IndexOf(sender)].ship.Rotation);

                    //Now scale our direction by pressing this key
                    modelVelocityAdd *= 6;

                    //Finally, add this vector to our velocity.
                    players[networkSession.AllGamers.IndexOf(sender)].ship.Update(modelVelocityAdd);
                }

                // are other players moving
                if (rotatedLeft || rotatedRight || movedUp || movedDown)
                {
                    if (players[networkSession.AllGamers.IndexOf(sender)].ship.Position.X > GameVariables.PlayfieldSizeX)
                        players[networkSession.AllGamers.IndexOf(sender)].ship.Position.X -= 2 * GameVariables.PlayfieldSizeX;
                    if (players[networkSession.AllGamers.IndexOf(sender)].ship.Position.X < -GameVariables.PlayfieldSizeX)
                        players[networkSession.AllGamers.IndexOf(sender)].ship.Position.X += 2 * GameVariables.PlayfieldSizeX;
                    if (players[networkSession.AllGamers.IndexOf(sender)].ship.Position.Y > GameVariables.PlayfieldSizeY)
                        players[networkSession.AllGamers.IndexOf(sender)].ship.Position.Y -= 2 * GameVariables.PlayfieldSizeY;
                    if (players[networkSession.AllGamers.IndexOf(sender)].ship.Position.Y < -GameVariables.PlayfieldSizeY)
                        players[networkSession.AllGamers.IndexOf(sender)].ship.Position.Y += 2 * GameVariables.PlayfieldSizeY;

                    //    if (!sender.IsLocal)
                    //        soundHandler.PlayOthersEngineSound();
                    //}
                    //else
                    //{
                    //    if (!sender.IsLocal)
                    //        soundHandler.PauseOthersEngineSound();
                }

                // what are players doing?
                for (int i = 0; i < NetworkSessionComponent.MaxGamers; i++)
                {
                    if (i != networkSession.AllGamers.IndexOf(sender))
                    {
                        checkNetworkingInteraction(players[i],
                            players[networkSession.AllGamers.IndexOf(sender)]);
                    }
                }

                players[networkSession.AllGamers.IndexOf(sender)].Update(gameTime);
            }
        }

        private void checkNetworkingInteraction(Player p1, Player p2)
        {
            // check for weapon ship collisions
            if (collisionsHandler.CheckForBulletShip(
                bulletModel.Meshes[0].BoundingSphere.Radius, p1, p2))
            {
                if (p1.Shield < 0)
                {
                    soundHandler.PlayCue("explosion3");
                }
                else
                    soundHandler.PlayCue("phase_collide_ship1");
            }

            if (collisionsHandler.CheckForBulletShip(
                bulletModel.Meshes[0].BoundingSphere.Radius, p2, p1))
            {
                if (p2.Shield < 0)
                {
                    soundHandler.PlayCue("explosion3");
                }
                else
                    soundHandler.PlayCue("phase_collide_ship1");
            }

            if (collisionsHandler.CheckForRocketShip(
                rocketModel.Meshes[0].BoundingSphere.Radius, p2, p1))
            {
                soundHandler.PlayCue("explosion2");

                if (p2.Shield < 0)
                {
                    soundHandler.PlayCue("explosion3");
                }
                else
                    soundHandler.PlayCue("phase_collide_ship1");
            }

            if (collisionsHandler.CheckForRocketShip(
                rocketModel.Meshes[0].BoundingSphere.Radius, p1, p2))
            {
                soundHandler.PlayCue("explosion2");

                if (p1.Shield < 0)
                {
                    soundHandler.PlayCue("explosion3");
                }
                else
                    soundHandler.PlayCue("phase_collide_ship1");
            }

            // check for weapon weapon collisions
            if ((collisionsHandler.CheckForBulletRocket(
                bulletModel.Meshes[0].BoundingSphere.Radius,
                rocketModel.Meshes[0].BoundingSphere.Radius,
                p2, p1)) ||
                (collisionsHandler.CheckForBulletRocket(
                bulletModel.Meshes[0].BoundingSphere.Radius,
                rocketModel.Meshes[0].BoundingSphere.Radius, p1, p2)) ||
                (collisionsHandler.CheckForRocketRocket(
                rocketModel.Meshes[0].BoundingSphere.Radius, p2, p1)) ||
                (collisionsHandler.CheckForRocketRocket(
                rocketModel.Meshes[0].BoundingSphere.Radius, p1, p2)))
            {
                soundHandler.PlayCue("explosion2");
            }

            // check for ship ship collisions
            if (collisionsHandler.CheckForShipShip(p1, p2))
            {
                Vector3 modelVelocityAdd = Vector3.Zero;

                modelVelocityAdd.X = -(float)Math.Sin(p1.ship.Rotation);
                modelVelocityAdd.Y = +(float)Math.Cos(p1.ship.Rotation);

                modelVelocityAdd *= -50;

                p1.ship.Update(modelVelocityAdd);

                modelVelocityAdd.X = -(float)Math.Sin(p2.ship.Rotation);
                modelVelocityAdd.Y = +(float)Math.Cos(p2.ship.Rotation);

                modelVelocityAdd *= -50;

                p2.ship.Update(modelVelocityAdd);

                if ((p1.Shield > 0) || (p2.Shield > 0))
                {
                    soundHandler.PlayCue("phase_collide_ship1");
                }
            }
        }

        private void switchTwoPlayersData(Player p1, Player p2, GameTime gameTime)
        {
            //change is data with old host data
            Player p = new Player();

            p.ship.Position = p1.ship.Position;
            p.ship.Velocity = p1.ship.Velocity;
            p.ship.Rotation = p1.ship.Rotation;
            p.ship.IsActive = p1.ship.IsActive;
            p.Radius = p1.Radius;
            p.Shield = p1.Shield;
            p.Score = p1.Score;

            p1.ship.Position = p2.ship.Position;
            p1.ship.Velocity = p2.ship.Velocity;
            p1.ship.Rotation = p2.ship.Rotation;
            p1.ship.IsActive = p2.ship.IsActive;
            p1.Radius = p2.Radius;
            p1.Shield = p2.Shield;
            p1.Score = p2.Score;

            p2.ship.Position = p.ship.Position;
            p2.ship.Velocity = p.ship.Velocity;
            p2.ship.Rotation = p.ship.Rotation;
            p2.ship.IsActive = p.ship.IsActive;
            p2.Radius = p.Radius;
            p2.Shield = p.Shield;
            p2.Score = p.Score;

            p1.Update(gameTime);
            p2.Update(gameTime);
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
                    * GameVariables.shipBoundingSphereScale * 2;
            }
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
                // Pause behavior for networked games.
                return !IsExiting;
            }
        }

        ///<summary>
        ///Lets the game respond to player input. Unlike the Update method,
        ///this will only be called when the gameplay screen is active.
        ///</summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.PauseGame || timeElapsed > 300)
            {
                isPaused = true;
                soundHandler.PauseMusic();
                ScreenManager.AddScreen(new PauseMenuScreen(networkSession));  
            }
            else
            {
                isPaused = false;
                isMovingUp = false;
                isMovingDown = false;
                isRotatingLeft = false;
                isRotatingRight = false;
                isFiringRockets = false;
                isFiringBullets = false;

                soundHandler.ResumeMusic();

                //are we shooting bullets?
                if (players[playerID].ship.IsActive && (input.IsNewKeyPress(Keys.Z)
                    || input.IsMouseLeftButtonPressed()))
                {
                    isFiringBullets = true;
                }

                //are we shooting rockets?
                if (players[playerID].ship.IsActive && (input.IsNewKeyPress(Keys.X)
                    || input.IsMouseRightButtonPressed()))
                {
                    isFiringRockets = true;
                }

                //Rotate to the viwer left while the Left key down
                if (players[playerID].ship.IsActive && 
                    input.IsNewKeyStillPress(Keys.Left, PlayerIndex.One))
                {
                    soundHandler.PlayMyEngineSound();
                    isRotatingLeft = true;
                }

                //Rotate to the viewer right while the Right key down
                if (players[playerID].ship.IsActive && 
                    input.IsNewKeyStillPress(Keys.Right, PlayerIndex.One))
                {
                    soundHandler.PlayMyEngineSound();
                    isRotatingRight = true;
                }

                //Move Up while the Up key is pressed
                if (players[playerID].ship.IsActive && input.IsNewKeyStillPress(Keys.Up,
                    PlayerIndex.One))
                {
                    soundHandler.PlayMyEngineSound();
                    isMovingUp = true;
                }

                //Move Down while the Down key is pressed
                if (players[playerID].ship.IsActive && input.IsNewKeyStillPress(Keys.Down,
                    PlayerIndex.One))
                {
                    soundHandler.PlayMyEngineSound();
                    isMovingDown = true;
                }

                if (!input.IsNewKeyStillPress(Keys.Up, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.Down, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.Left, PlayerIndex.One)
                    && !input.IsNewKeyStillPress(Keys.Right, PlayerIndex.One))
                {
                    isMovingUp = false;
                    isMovingDown = false;
                    isRotatingLeft = false;
                    isRotatingRight = false;
                    soundHandler.PauseMyEngineSound();
                }

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
                //If we are in a network game, check if we should return to the lobby.
                if (!IsExiting)
                {
                    if (networkSession.SessionState == NetworkSessionState.Lobby)
                    {
                        LoadingScreen.Load(ScreenManager, true,
                                           new BackgroundScreen(),
                                           new LobbyScreen(networkSession));
                    }
                }

                foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
                {
                    if (GameVariables.FlawToleranceOn)
                    {
                        if (sessionPlayers > networkSession.AllGamers.Count)
                        {
                            if (playerID != networkSession.AllGamers.IndexOf(gamer))
                            {
                                idChanged = true;
                            }
                            else
                            {
                                idChanged = false;
                            }
                        }
                    }

                    ReceiveNetworkData(gamer, gameTime);

                    if (GameVariables.FlawToleranceOn)
                    {
                        if (sessionPlayers != networkSession.AllGamers.Count)
                        {
                            packetWriter.Write(idChanged);

                            if (idChanged)
                            {
                                packetWriter.Write(playerID);
                                packetWriter.Write(networkSession.AllGamers.IndexOf(gamer));

                                idChanged = false;
                                playerID = networkSession.AllGamers.IndexOf(gamer);
                            }

                            if (sessionPlayers < networkSession.AllGamers.Count)
                            {
                                packetWriter.Write(networkSession.IsHost);

                                if (networkSession.IsHost)
                                {
                                    for (int i = 0; i < players.Length; i++)
                                    {
                                        packetWriter.Write(players[i].ship.Position);
                                        packetWriter.Write(players[i].ship.Velocity);
                                        packetWriter.Write(players[i].ship.IsActive);
                                        packetWriter.Write(players[i].ship.Rotation);
                                        packetWriter.Write(players[i].Radius);
                                        packetWriter.Write(players[i].Shield);
                                        packetWriter.Write(players[i].Score);
                                    }
                                }
                            }
                            else
                            {
                                packetWriter.Write(false);
                            }

                            //update the number of players in session
                            sessionPlayers = networkSession.AllGamers.Count;
                        }
                        else
                        {
                            packetWriter.Write(false);
                            packetWriter.Write(false);
                        }
                    }

                    packetWriter.Write(players[playerID].ship.IsActive);
                    packetWriter.Write(players[playerID].Radius);
                    packetWriter.Write(isFiringBullets);
                    packetWriter.Write(isFiringRockets);
                    packetWriter.Write(isRotatingLeft);
                    packetWriter.Write(isRotatingRight);
                    packetWriter.Write(isMovingUp);
                    packetWriter.Write(isMovingDown);

                    gamer.SendData(packetWriter, SendDataOptions.ReliableInOrder);
                }
            }

            //get the number of active players;
            activePlayers = networkSession.AllGamers.Count;

            for (int i = 0; i < networkSession.AllGamers.Count; i++)
            {
                if (!players[i].ship.IsActive)
                {
                    activePlayers--;
                }
            }

            camera.Update();
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region Draw, Draw Player, Draw Shooting Draw Scores, and Draw Model

        public override void Draw(GameTime gameTime)
        {
            Vector2 titlePosition = new Vector2(400, 60);
            Vector2 titleOrigin = font.MeasureString(Resources.Scores) / 2;
            Color titleColor = new Color(192, 192, 192, TransitionAlpha);
            float titleScale = 1.5f;

            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteBlendMode.None,
                SpriteSortMode.Immediate, SaveStateMode.None);
            spriteBatch.Draw(stars, new Rectangle(0, 0, 800, 600), Color.White);

            spriteBatch.End();

            if (activePlayers == 1 && !isPaused)
            {
                spriteBatch.Begin();

                //Make the menu slide into place during transitions, using a
                //power curve to make things look more interesting (this makes
                //the movement slow down as it nears the end).
                float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

                //Draw the menu title.
                titlePosition.Y -= transitionOffset * 100;

                spriteBatch.DrawString(font, Resources.Scores, titlePosition, titleColor, 0,
                                       titleOrigin, titleScale, SpriteEffects.None, 0);

                spriteBatch.End();
            }

            //list to sort all gamers in session scores if necessary 
            List<NetworkGamer> gamers = new List<NetworkGamer>();

            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                //add each networkGamer in current session to gamers list
                gamers.Add(gamer);

                DrawShooting(players[networkSession.AllGamers.IndexOf(gamer)]);
                DrawPlayer(players[networkSession.AllGamers.IndexOf(gamer)]);

                spriteBatch.Begin(SpriteBlendMode.AlphaBlend,
                    SpriteSortMode.Immediate, SaveStateMode.None);

                if (!gamer.IsDisposed && gamer.IsLocal && players[playerID].ship.IsActive)
                {
                    int posX = (int)
                        (((players[networkSession.AllGamers.IndexOf(gamer)].ship.Position.X
                        + (GameVariables.PlayfieldSizeX / 2)) /
                        (GameVariables.PlayfieldSizeX)) *
                        (100 + (GraphicsDeviceManager.DefaultBackBufferWidth / 2))) +
                        170;
                    int posY = (int)
                        ((50 + (GraphicsDeviceManager.DefaultBackBufferHeight / 2)) -
                        (((players[networkSession.AllGamers.IndexOf(gamer)].ship.Position.Y
                        + (GameVariables.PlayfieldSizeY / 2)) /
                        (GameVariables.PlayfieldSizeY)) *
                        (50 + (GraphicsDeviceManager.DefaultBackBufferHeight / 2))) + 50);

                    spriteBatch.Draw(gamer.GetProfile().GamerPicture,
                        new Rectangle(posX, posY, 25, 25), Color.White);
                }

                spriteBatch.End();
            }

            spriteBatch.Begin();

            if (activePlayers <= 1 && !isPaused)
            {
                timeElapsed++;

                gamers.Sort(delegate(NetworkGamer g2, NetworkGamer g1)
                {
                    return (players[networkSession.AllGamers.IndexOf(g1)].Score.CompareTo(
                        players[networkSession.AllGamers.IndexOf(g2)].Score));
                });

                scoresPosition.Y = 100;

                foreach (NetworkGamer gamer in gamers)
                {
                    scoresPosition.Y += 30.0f;

                    spriteBatch.DrawString(font, gamer.Gamertag + "     " +
                        players[networkSession.AllGamers.IndexOf(gamer)].Score,
                        scoresPosition, titleColor, 0, titleOrigin, scoresScale,
                        SpriteEffects.None, 0);
                }
            }
            else
            {
                timeElapsed = 0;
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend,
                    SpriteSortMode.Immediate, SaveStateMode.None);

            spriteBatch.DrawString(lucidaConsole, "Score: " +
                players[playerID].Score, scorePosition, Color.LightGreen);

            if (players[playerID].Shield < 0)
                spriteBatch.DrawString(lucidaConsole, "Shield: 0%",
                    shieldPosition, Color.LightGreen);
            else
                spriteBatch.DrawString(lucidaConsole, "Shield: " +
                    players[playerID].Shield + "%", shieldPosition, Color.LightGreen);

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
                    camera.SetProjection( GameVariables.CameraHeight * 100.0f );
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
