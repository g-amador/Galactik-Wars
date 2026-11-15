#region File Description
//-----------------------------------------------------------------------------
//CollisionHandler.cs
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
using Microsoft.Xna.Framework;
#endregion

namespace GalactikWars
{
    class CollisionsHandler
    {
        #region Collisions Check

        #region Fields

        public Vector2 explosionPosition = Vector2.Zero;

        #endregion

        #region Weapon Ship

        public bool CheckForBulletShip(float bulletRadius, Player p1, Player p2)
        {
            if (p1.ship.IsActive)
            {
                BoundingSphere shipSphere =
                  new BoundingSphere(p1.ship.Position, p1.Radius *
                                 GameVariables.shipBoundingSphereScale);
                for (int i = 0; i < p2.bulletList.Length; i++)
                {
                    if (p2.bulletList[i].IsActive)
                    {
                        BoundingSphere bulletSphere = new BoundingSphere(
                            p2.bulletList[i].position, bulletRadius);

                        if (bulletSphere.Intersects(shipSphere))
                        {
                            p2.bulletList[i].IsActive = false;
                            p1.Shield -= GameVariables.BulletDamage;

                            if (p1.Shield < 0)
                            {
                                p1.ship.IsActive = false;
                                p2.Score += GameVariables.KillShipBonus;
                                explosionPosition.X = p1.ship.Position.X;
                                explosionPosition.Y = p1.ship.Position.Y;

                                if (p1.Score - GameVariables.DeathPenalty < 0)
                                    p1.Score = 0;
                                else
                                    p1.Score -= GameVariables.DeathPenalty;
                            }
                            else
                            {
                                if (p1.Score - GameVariables.ShotPenalty < 0)
                                    p1.Score = 0;
                                else
                                    p1.Score -= GameVariables.ShotPenalty;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool CheckForRocketShip(float rocketRadius, Player p1, Player p2)
        {
            if (p1.ship.IsActive)
            {
                BoundingSphere shipSphere =
                  new BoundingSphere(p1.ship.Position, p1.Radius *
                                 GameVariables.shipBoundingSphereScale);
                for (int i = 0; i < p2.rocketList.Length; i++)
                {
                    if (p2.rocketList[i].IsActive)
                    {
                        BoundingSphere rocketSphere = new BoundingSphere(
                            p2.rocketList[i].position, rocketRadius);

                        if (rocketSphere.Intersects(shipSphere))
                        {
                            p2.rocketList[i].IsActive = false;
                            p1.Shield -= GameVariables.RocketDamage;

                            if (p1.Shield < 0)
                            {
                                p1.ship.IsActive = false;
                                p2.Score += GameVariables.KillShipBonus;
                                explosionPosition.X = p1.ship.Position.X;
                                explosionPosition.Y = p1.ship.Position.Y;

                                if (p1.Score - GameVariables.DeathPenalty < 0)
                                    p1.Score = 0;
                                else
                                    p1.Score -= GameVariables.DeathPenalty;
                            }
                            else
                            {
                                if (p1.Score - GameVariables.ShotPenalty < 0)
                                    p1.Score = 0;
                                else
                                    p1.Score -= GameVariables.ShotPenalty;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region Weapon Weapon

        public bool CheckForBulletRocket(float bulletRadius, float rocketRadius,
            Player p1, Player p2)
        {
            for (int i = 0; i < p1.bulletList.Length; i++)
            {
                if (p1.bulletList[i].IsActive)
                {
                    BoundingSphere bulletSphere = new BoundingSphere(
                        p1.bulletList[i].position, bulletRadius);
                    for (int j = 0; j < p2.rocketList.Length; j++)
                    {
                        if (p2.rocketList[j].IsActive)
                        {
                            BoundingSphere rocketSphere = new BoundingSphere(
                                p2.rocketList[j].position, rocketRadius);

                            if (rocketSphere.Intersects(bulletSphere))
                            {
                                p2.rocketList[j].IsActive = false;
                                p1.bulletList[i].IsActive = false;

                                explosionPosition.X = p2.rocketList[j].position.X;
                                explosionPosition.Y = p2.rocketList[j].position.Y;

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool CheckForRocketRocket(float rocketRadius, Player p1, Player p2)
        {
            for (int i = 0; i < p1.rocketList.Length; i++)
            {
                if (p1.rocketList[i].IsActive)
                {
                    BoundingSphere rocketSphere1 = new BoundingSphere(
                        p1.rocketList[i].position, rocketRadius);
                    for (int j = 0; j < p2.rocketList.Length; j++)
                    {
                        if (p2.rocketList[j].IsActive)
                        {
                            BoundingSphere rocketSphere2 = new BoundingSphere(
                                p2.rocketList[j].position, rocketRadius);

                            if (rocketSphere2.Intersects(rocketSphere1))
                            {
                                p2.rocketList[j].IsActive = false;
                                p1.rocketList[i].IsActive = false;

                                explosionPosition.X = p2.rocketList[j].position.X;
                                explosionPosition.Y = p2.rocketList[j].position.Y;

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region Ship Ship/Asteroid

        public bool CheckForShipShip(Player p1, Player p2)
        {
            if (p1.ship.IsActive)
            {
                BoundingSphere shipSphere1 = new BoundingSphere(
                    p1.ship.Position, p1.Radius / 2);
                if (p2.ship.IsActive)
                {
                    BoundingSphere shipSphere2 = new BoundingSphere(
                        p2.ship.Position, p2.Radius / 2);
                    if (shipSphere2.Intersects(shipSphere1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckForShipAsteroidCollision(float asteroid1Radius,
            float asteroid2Radius, Asteroid[] asteroid1List, Asteroid[] asteroid2List,
            Player p)
        {
            //ship-asteroid collision check
            if (p.ship.IsActive)
            {
                BoundingSphere shipSphere = new BoundingSphere(
                    p.ship.Position, p.Radius);
                for (int i = 0; i < asteroid2List.Length; i++)
                {
                    BoundingSphere b;

                    if (i < asteroid1List.Length)
                    {
                        if (asteroid1List[i].isActive)
                        {
                            b = new BoundingSphere(
                                asteroid1List[i].position, asteroid1Radius *
                                GameVariables.AsteroidBoundingSphereScale);
                            if (b.Intersects(shipSphere))
                            {
                                p.Shield -= GameVariables.Asteroid1Damage;

                                if (p.Shield < 0)
                                {
                                    p.ship.IsActive = false;
                                    p.lives--;

                                    if (p.Score - GameVariables.DeathPenalty < 0)
                                        p.Score = 0;
                                    else
                                        p.Score -= GameVariables.DeathPenalty;
                                }

                                asteroid1List[i].isActive = false;
                                return true;
                            }
                        }
                    }

                    if (asteroid2List[i].isActive)
                    {
                        b = new BoundingSphere(
                            asteroid2List[i].position, asteroid2Radius *
                            GameVariables.AsteroidBoundingSphereScale);
                        if (b.Intersects(shipSphere))
                        {
                            p.Shield -= GameVariables.Asteroid2Damage;

                            if (p.Shield < 0)
                            {
                                p.ship.IsActive = false;
                                p.lives--;
                                if (p.Score - GameVariables.DeathPenalty < 0)
                                    p.Score = 0;
                                else
                                    p.Score -= GameVariables.DeathPenalty;
                            }
                            asteroid2List[i].isActive = false;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region Asteroid Asteroid

        public void CheckForAsteroidCollision(float asteroid1Radius,
            Asteroid[] asteroid1List, float asteroid2Radius, Asteroid[] asteroid2List)
        {
            for (int i = 0; i < asteroid1List.Length; i++)
            {
                if (asteroid1List[i].isActive)
                {
                    BoundingSphere b1 = new BoundingSphere(
                        asteroid1List[i].position, asteroid1Radius *
                        GameVariables.AsteroidBoundingSphereScale);

                    for (int j = 0; j < asteroid1List.Length; j++)
                    {
                        if (i != j && asteroid1List[j].isActive)
                        {
                            BoundingSphere b2 = new BoundingSphere(
                                asteroid1List[j].position, asteroid1Radius *
                                GameVariables.AsteroidBoundingSphereScale);

                            if (b1.Intersects(b2))
                            {
                                //???????
                            }
                        }
                    }

                    for (int j = 0; j < asteroid2List.Length; j++)
                    {
                        BoundingSphere b2 = new BoundingSphere(
                            asteroid2List[j].position, asteroid2Radius *
                            GameVariables.AsteroidBoundingSphereScale);

                        if (b1.Intersects(b2))
                        {
                            //???????
                        }
                    }
                }
            }

            for (int i = 0; i < asteroid2List.Length; i++)
            {
                if (asteroid2List[i].isActive)
                {
                    BoundingSphere b1 = new BoundingSphere(
                        asteroid2List[i].position, asteroid2Radius *
                        GameVariables.AsteroidBoundingSphereScale);

                    for (int j = 0; j < asteroid2List.Length; j++)
                    {
                        BoundingSphere b2 = new BoundingSphere(
                            asteroid2List[j].position, asteroid2Radius *
                            GameVariables.AsteroidBoundingSphereScale);

                        if (b1.Intersects(b2))
                        {
                            //???????
                        }
                    }
                }
            }
        }

        #endregion

        #region Weapon Asteroid

        public bool CheckForBulletAsteroidCollision(float bulletRadius,
            float asteroidRadius, Player p, Asteroid[] asteroidList)
        {
            //bullet-asteroid collision check
            for (int i = 0; i < asteroidList.Length; i++)
            {
                if (asteroidList[i].isActive)
                {
                    BoundingSphere asteroidSphere =
                      new BoundingSphere(asteroidList[i].position, asteroidRadius *
                                     GameVariables.AsteroidBoundingSphereScale);
                    for (int j = 0; j < p.bulletList.Length; j++)
                    {
                        if (p.bulletList[j].IsActive)
                        {
                            BoundingSphere bulletSphere = new BoundingSphere(p.bulletList[j].position,
                                bulletRadius);
                            if (asteroidSphere.Intersects(bulletSphere))
                            {
                                asteroidList[i].isActive = false;
                                p.bulletList[j].IsActive = false;
                                p.Score += GameVariables.KillAsteroidBonus;

                                explosionPosition.X = asteroidList[i].position.X;
                                explosionPosition.Y = asteroidList[i].position.Y;

                                return true; //no need to check other bullets
                            }
                        }
                    }
                }
            }
            return false;
        }

        public int CheckForRocketAsteroidCollision(float rocketRadius,
            float asteroid1Radius, float asteroid2Radius, Player p,
            Asteroid[] asteroid1List, Asteroid[] asteroid2List)
        {
            int destroyedAsteroids = 0;

            for (int i = 0; i < asteroid1List.Length; i++)
            {
                if (asteroid1List[i].isActive)
                {
                    BoundingSphere asteroidSphere1 =
                      new BoundingSphere(asteroid1List[i].position, asteroid1Radius *
                                     GameVariables.AsteroidBoundingSphereScale);
                    for (int j = 0; j < p.rocketList.Length; j++)
                    {
                        if (p.rocketList[j].IsActive)
                        {
                            BoundingSphere rocketSphere = new BoundingSphere(
                                p.rocketList[j].position,
                                rocketRadius);
                            BoundingSphere rocketDestructionSphere = new BoundingSphere(
                                p.rocketList[j].position,
                                rocketRadius * 5);
                            if (asteroidSphere1.Intersects(rocketSphere))
                            {
                                BoundingSphere asteroidSphere2;

                                for (int k = 0; k < asteroid1List.Length; k++)
                                {
                                    asteroidSphere2 = new BoundingSphere(
                                        asteroid1List[k].position, asteroid1Radius *
                                        GameVariables.AsteroidBoundingSphereScale);

                                    if (asteroid1List[k].isActive &&
                                        asteroidSphere2.Intersects(
                                        rocketDestructionSphere))
                                    {
                                        asteroid1List[k].isActive = false;
                                        destroyedAsteroids++;
                                        p.Score += GameVariables.KillAsteroidBonus;
                                    }
                                }

                                for (int k = 0; k < asteroid2List.Length; k++)
                                {
                                    asteroidSphere2 = new BoundingSphere(
                                        asteroid2List[k].position, asteroid2Radius *
                                        GameVariables.AsteroidBoundingSphereScale);

                                    if (asteroid2List[k].isActive &&
                                        asteroidSphere2.Intersects(
                                        rocketDestructionSphere))
                                    {
                                        asteroid2List[k].isActive = false;
                                        destroyedAsteroids++;
                                        p.Score += GameVariables.KillAsteroidBonus;
                                    }
                                }

                                p.rocketList[j].IsActive = false;

                                explosionPosition.X = p.rocketList[j].position.X;
                                explosionPosition.Y = p.rocketList[j].position.Y;

                                return destroyedAsteroids;
                            }
                        }
                    }
                }
            }
            return destroyedAsteroids;
        }

        #endregion

        #endregion
    }
}
