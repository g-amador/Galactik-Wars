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
using GalactikWars.Entities;
using Microsoft.Xna.Framework;
using System;
#endregion

namespace GalactikWars.Handlers
{
    internal class CollisionsHandler
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
                  new(p1.ship.Position, p1.Radius *
                                 GameVariables.shipBoundingSphereScale);
                for (int i = 0; i < p2.bulletList.Length; i++)
                {
                    if (p2.bulletList[i].IsActive)
                    {
                        BoundingSphere bulletSphere = new(
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
                                {
                                    p1.Score = 0;
                                }
                                else
                                {
                                    p1.Score -= GameVariables.DeathPenalty;
                                }
                            }
                            else
                            {
                                if (p1.Score - GameVariables.ShotPenalty < 0)
                                {
                                    p1.Score = 0;
                                }
                                else
                                {
                                    p1.Score -= GameVariables.ShotPenalty;
                                }
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
                  new(p1.ship.Position, p1.Radius *
                                 GameVariables.shipBoundingSphereScale);
                for (int i = 0; i < p2.rocketList.Length; i++)
                {
                    if (p2.rocketList[i].IsActive)
                    {
                        BoundingSphere rocketSphere = new(
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
                                {
                                    p1.Score = 0;
                                }
                                else
                                {
                                    p1.Score -= GameVariables.DeathPenalty;
                                }
                            }
                            else
                            {
                                if (p1.Score - GameVariables.ShotPenalty < 0)
                                {
                                    p1.Score = 0;
                                }
                                else
                                {
                                    p1.Score -= GameVariables.ShotPenalty;
                                }
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region Enemy Ships

        //player or ally bullets hitting a red enemy / blue ally ship
        public bool CheckForBulletEnemyShip(float bulletRadius, Player shooter,
            EnemyShip target)
        {
            if (target.IsActive)
            {
                BoundingSphere shipSphere = new(target.Position, target.Radius);

                for (int i = 0; i < shooter.bulletList.Length; i++)
                {
                    if (shooter.bulletList[i].IsActive)
                    {
                        BoundingSphere bulletSphere = new(
                            shooter.bulletList[i].position, bulletRadius);

                        if (bulletSphere.Intersects(shipSphere))
                        {
                            shooter.bulletList[i].IsActive = false;
                            target.Shield -= GameVariables.BulletDamage;

                            if (target.Shield < 0)
                            {
                                target.IsActive = false;
                                shooter.Score += GameVariables.EnemyShipBonus;
                                explosionPosition.X = target.Position.X;
                                explosionPosition.Y = target.Position.Y;
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //blue ally bullets hitting a red enemy ship (kills credit the
        //player, handled by the caller)
        public bool CheckForAllyBulletEnemyShip(float bulletRadius,
            EnemyShip shooter, EnemyShip target)
        {
            if (target.IsActive && shooter.IsActive)
            {
                BoundingSphere shipSphere = new(target.Position, target.Radius);

                for (int i = 0; i < shooter.bulletList.Length; i++)
                {
                    if (shooter.bulletList[i].IsActive)
                    {
                        BoundingSphere bulletSphere = new(
                            shooter.bulletList[i].position, bulletRadius);

                        if (bulletSphere.Intersects(shipSphere))
                        {
                            shooter.bulletList[i].IsActive = false;
                            target.Shield -= GameVariables.BulletDamage;

                            if (target.Shield < 0)
                            {
                                target.IsActive = false;
                                explosionPosition.X = target.Position.X;
                                explosionPosition.Y = target.Position.Y;
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //rockets destroy red enemy / blue ally ships outright
        public bool CheckForRocketEnemyShip(float rocketRadius, Player shooter,
            EnemyShip target)
        {
            if (target.IsActive)
            {
                BoundingSphere shipSphere = new(target.Position, target.Radius);

                for (int i = 0; i < shooter.rocketList.Length; i++)
                {
                    if (shooter.rocketList[i].IsActive)
                    {
                        BoundingSphere rocketSphere = new(
                            shooter.rocketList[i].position, rocketRadius);

                        if (rocketSphere.Intersects(shipSphere))
                        {
                            shooter.rocketList[i].IsActive = false;
                            target.Shield = -1;
                            target.IsActive = false;
                            shooter.Score += GameVariables.EnemyShipBonus;
                            explosionPosition.X = target.Position.X;
                            explosionPosition.Y = target.Position.Y;

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //red enemy fire hitting the player's shield
        public bool CheckForEnemyBulletShip(float bulletRadius, Player p,
            EnemyShip enemy)
        {
            if (p.ship.IsActive && enemy.IsActive)
            {
                BoundingSphere shipSphere = new(
                    p.ship.Position, p.Radius *
                    GameVariables.shipBoundingSphereScale);

                for (int i = 0; i < enemy.bulletList.Length; i++)
                {
                    if (enemy.bulletList[i].IsActive)
                    {
                        BoundingSphere bulletSphere = new(
                            enemy.bulletList[i].position, bulletRadius);

                        if (bulletSphere.Intersects(shipSphere))
                        {
                            enemy.bulletList[i].IsActive = false;
                            p.Shield -= GameVariables.BulletDamage;

                            if (p.Shield < 0)
                            {
                                p.ship.IsActive = false;
                                p.lives--;
                                explosionPosition.X = p.ship.Position.X;
                                explosionPosition.Y = p.ship.Position.Y;

                                if (p.Score - GameVariables.DeathPenalty < 0)
                                {
                                    p.Score = 0;
                                }
                                else
                                {
                                    p.Score -= GameVariables.DeathPenalty;
                                }
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //red enemy fire hitting the blue ally escort
        public bool CheckForEnemyBulletAlly(float bulletRadius, EnemyShip ally,
            EnemyShip enemy)
        {
            if (ally.IsActive && enemy.IsActive)
            {
                BoundingSphere allySphere = new(ally.Position, ally.Radius);

                for (int i = 0; i < enemy.bulletList.Length; i++)
                {
                    if (enemy.bulletList[i].IsActive)
                    {
                        BoundingSphere bulletSphere = new(
                            enemy.bulletList[i].position, bulletRadius);

                        if (bulletSphere.Intersects(allySphere))
                        {
                            enemy.bulletList[i].IsActive = false;
                            ally.Shield -= GameVariables.BulletDamage;

                            if (ally.Shield < 0)
                            {
                                ally.IsActive = false;
                                explosionPosition.X = ally.Position.X;
                                explosionPosition.Y = ally.Position.Y;
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //ship-vs-ship shield collisions (phase 3): while both shields hold,
        //the hulls bounce apart and both shields drain; when one shield
        //gives in, that ship blows up — the caller plays the sounds and
        //spawns the explosions from explosionPosition
        public bool CheckForPlayerEnemyShipCollision(Player p, EnemyShip enemy)
        {
            if (!(p.ship.IsActive && enemy.IsActive))
            {
                return false;
            }

            BoundingSphere playerSphere = new(
                p.ship.Position, p.Radius *
                GameVariables.shipBoundingSphereScale);
            BoundingSphere enemySphere = new(
                enemy.Position, enemy.Radius *
                GameVariables.shipBoundingSphereScale);

            if (!playerSphere.Intersects(enemySphere))
            {
                return false;
            }

            //both shields drain from the impact
            p.Shield -= GameVariables.ShipCollideDamage;
            enemy.Shield -= GameVariables.ShipCollideDamage;

            bool playerDown = p.Shield < 0;
            bool enemyDown = enemy.Shield < 0;

            if (!playerDown && !enemyDown)
            {
                //both shields hold: bounce the hulls apart
                BounceApart(p.ship.Position,
                    p.Radius * GameVariables.shipBoundingSphereScale,
                    enemy.Position,
                    enemy.Radius * GameVariables.shipBoundingSphereScale,
                    out Vector3 playerPush,
                    out Vector3 enemyPush, out Vector3 separation);

                p.ship.Velocity += playerPush;
                enemy.Velocity += enemyPush;
                p.ship.Position += separation * 0.5f;
                enemy.Position -= separation * 0.5f;
            }
            else if (playerDown && !enemyDown)
            {
                //the player's shield gave in: the player blows up
                p.ship.IsActive = false;
                p.lives--;
                explosionPosition.X = p.ship.Position.X;
                explosionPosition.Y = p.ship.Position.Y;

                if (p.Score - GameVariables.DeathPenalty < 0)
                {
                    p.Score = 0;
                }
                else
                {
                    p.Score -= GameVariables.DeathPenalty;
                }
            }
            else if (enemyDown && !playerDown)
            {
                //the enemy's shield gave in: it blows up
                enemy.IsActive = false;
                p.Score += GameVariables.EnemyShipBonus;
                explosionPosition.X = enemy.Position.X;
                explosionPosition.Y = enemy.Position.Y;
            }
            else
            {
                //both shields gave in: both ships blow up
                p.ship.IsActive = false;
                p.lives--;
                enemy.IsActive = false;
                p.Score += GameVariables.EnemyShipBonus;
                explosionPosition.X = (p.ship.Position.X + enemy.Position.X) * 0.5f;
                explosionPosition.Y = (p.ship.Position.Y + enemy.Position.Y) * 0.5f;

                if (p.Score - GameVariables.DeathPenalty < 0)
                {
                    p.Score = 0;
                }
                else
                {
                    p.Score -= GameVariables.DeathPenalty;
                }
            }

            return true;
        }

        //the blue ally ramming a red enemy (kills credit the player)
        public bool CheckForAllyEnemyShipCollision(EnemyShip ally,
            EnemyShip enemy, out bool allyDestroyed, out bool enemyDestroyed)
        {
            allyDestroyed = false;
            enemyDestroyed = false;

            if (!(ally.IsActive && enemy.IsActive))
            {
                return false;
            }

            BoundingSphere allySphere = new(ally.Position, ally.Radius);
            BoundingSphere enemySphere = new(enemy.Position, enemy.Radius);

            if (!allySphere.Intersects(enemySphere))
            {
                return false;
            }

            //both shields drain from the impact
            ally.Shield -= GameVariables.ShipCollideDamage;
            enemy.Shield -= GameVariables.ShipCollideDamage;

            bool allyDown = ally.Shield < 0;
            bool enemyDown = enemy.Shield < 0;

            if (!allyDown && !enemyDown)
            {
                //both shields hold: bounce the hulls apart
                BounceApart(ally.Position, ally.Radius,
                    enemy.Position, enemy.Radius,
                    out Vector3 allyPush,
                    out Vector3 enemyPush, out Vector3 separation);

                ally.Velocity += allyPush;
                enemy.Velocity += enemyPush;
                ally.Position += separation * 0.5f;
                enemy.Position -= separation * 0.5f;
            }
            else
            {
                if (allyDown)
                {
                    ally.IsActive = false;
                    allyDestroyed = true;
                    explosionPosition.X = ally.Position.X;
                    explosionPosition.Y = ally.Position.Y;
                }

                if (enemyDown)
                {
                    enemy.IsActive = false;
                    enemyDestroyed = true;
                    explosionPosition.X = enemy.Position.X;
                    explosionPosition.Y = enemy.Position.Y;
                }
            }

            return true;
        }

        //shared bounce math: pushes the two hulls apart along the collision
        //normal and reports how far the spheres overlap
        private static void BounceApart(Vector3 positionA,
            float radiusA, Vector3 positionB, float radiusB,
            out Vector3 pushA, out Vector3 pushB, out Vector3 separation)
        {
            Vector3 normal = positionA - positionB;

            if (normal.LengthSquared() < 0.0001f)
            {
                normal = Vector3.UnitY;
            }

            normal.Normalize();

            //each ship is pushed away from the other along the normal —
            //swapping the velocity components would feel too abrupt here,
            //a firm impulse reads better for shield collisions
            pushA = normal * GameVariables.ShipBounceSpeed;
            pushB = -normal * GameVariables.ShipBounceSpeed;

            //separate overlapping spheres so the collision does not repeat
            float overlap = (radiusA + radiusB)
                - Vector3.Distance(positionA, positionB);
            separation = normal * Math.Max(overlap, 0.0f);
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
                    BoundingSphere bulletSphere = new(
                        p1.bulletList[i].position, bulletRadius);
                    for (int j = 0; j < p2.rocketList.Length; j++)
                    {
                        if (p2.rocketList[j].IsActive)
                        {
                            BoundingSphere rocketSphere = new(
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
                    BoundingSphere rocketSphere1 = new(
                        p1.rocketList[i].position, rocketRadius);
                    for (int j = 0; j < p2.rocketList.Length; j++)
                    {
                        if (p2.rocketList[j].IsActive)
                        {
                            BoundingSphere rocketSphere2 = new(
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
                BoundingSphere shipSphere1 = new(
                    p1.ship.Position, p1.Radius / 2);
                if (p2.ship.IsActive)
                {
                    BoundingSphere shipSphere2 = new(
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
                BoundingSphere shipSphere = new(
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
                                    {
                                        p.Score = 0;
                                    }
                                    else
                                    {
                                        p.Score -= GameVariables.DeathPenalty;
                                    }
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
                                {
                                    p.Score = 0;
                                }
                                else
                                {
                                    p.Score -= GameVariables.DeathPenalty;
                                }
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
                    BoundingSphere b1 = new(
                        asteroid1List[i].position, asteroid1Radius *
                        GameVariables.AsteroidBoundingSphereScale);

                    for (int j = 0; j < asteroid1List.Length; j++)
                    {
                        if (i != j && asteroid1List[j].isActive)
                        {
                            BoundingSphere b2 = new(
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
                        BoundingSphere b2 = new(
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
                    BoundingSphere b1 = new(
                        asteroid2List[i].position, asteroid2Radius *
                        GameVariables.AsteroidBoundingSphereScale);

                    for (int j = 0; j < asteroid2List.Length; j++)
                    {
                        BoundingSphere b2 = new(
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
                      new(asteroidList[i].position, asteroidRadius *
                                     GameVariables.AsteroidBoundingSphereScale);
                    for (int j = 0; j < p.bulletList.Length; j++)
                    {
                        if (p.bulletList[j].IsActive)
                        {
                            BoundingSphere bulletSphere = new(p.bulletList[j].position,
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
            //Rockets are single-target: they destroy a small asteroid, but a
            //large one is too tough and only splits in two. Returns 0 when
            //nothing was hit, 1 when a small asteroid was destroyed and 2
            //when a large asteroid was destroyed (caller spawns the split).
            for (int j = 0; j < p.rocketList.Length; j++)
            {
                if (p.rocketList[j].IsActive)
                {
                    BoundingSphere rocketSphere = new(
                        p.rocketList[j].position,
                        rocketRadius);

                    for (int i = 0; i < asteroid1List.Length; i++)
                    {
                        if (asteroid1List[i].isActive)
                        {
                            BoundingSphere asteroidSphere1 =
                              new(asteroid1List[i].position, asteroid1Radius *
                                             GameVariables.AsteroidBoundingSphereScale);
                            if (asteroidSphere1.Intersects(rocketSphere))
                            {
                                asteroid1List[i].isActive = false;
                                p.rocketList[j].IsActive = false;
                                p.Score += GameVariables.KillAsteroidBonus;

                                explosionPosition.X = p.rocketList[j].position.X;
                                explosionPosition.Y = p.rocketList[j].position.Y;

                                return 2; //large asteroid down - split it
                            }
                        }
                    }

                    for (int i = 0; i < asteroid2List.Length; i++)
                    {
                        if (asteroid2List[i].isActive)
                        {
                            BoundingSphere asteroidSphere2 =
                              new(asteroid2List[i].position, asteroid2Radius *
                                             GameVariables.AsteroidBoundingSphereScale);
                            if (asteroidSphere2.Intersects(rocketSphere))
                            {
                                asteroid2List[i].isActive = false;
                                p.rocketList[j].IsActive = false;
                                p.Score += GameVariables.KillAsteroidBonus;

                                explosionPosition.X = p.rocketList[j].position.X;
                                explosionPosition.Y = p.rocketList[j].position.Y;

                                return 1; //small asteroid destroyed
                            }
                        }
                    }
                }
            }
            return 0;
        }

        #endregion

        #endregion
    }
}
