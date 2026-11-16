#region File Description
//-----------------------------------------------------------------------------
//ParticleSystem.cs
//
//Author: GNPA & STX
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using statements
using GalactikWars.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#endregion

namespace GalactikWars.Particles
{
    internal class ParticleSystem : GameScreen
    {
        #region Fields

        private const int ppemitter = 100, ppemitter2 = 100;//The number of a type of particles allowed per emitter.
        private int ppemitterprime2;//Real particles per emitter.
        private int i;//Used in for loops.
        private int existingParticles = 0, existingParticles2 = 0;//How many total particles are active.
        private readonly Random random = new();
        private Texture2D particlesTexture;

        #endregion

        #region particlesCreate, particlesUpdate

        public void particlesCreate(List<Particle> part, float emitx, float emity, int size, int particle)
        {
            #region Ship Trail

            if (particle == 1)
            {
                CreateTrailParticles(part, emitx, emity, size, 0.0f, 0.0f);
            }

            #endregion

            #region Explosions

            if (particle == 2)
            {
                ppemitterprime2 = 0;//We use this to find out how many particles are used in the emitter.

                for (i = 0; i < part.Count; i++)//This cycles through the list of particles to count how many are for the specified emitter. This is the first part in recycling the old particles.
                {
                    if (part[i].xyorg.X == emitx && part[i].xyorg.Y == emity && part[i].used == true)//Checking if the particle is active and if it belongs to the emitter placed at X,Y position.
                    {
                        ppemitterprime2 += 1;//Adding to the count.
                    }
                }

                if (ppemitterprime2 < ppemitter2)// If not enough particles are given to the emitter, we'll add to them by recycling all the old ones we find in the list.
                {
                    for (i = 0; i < part.Count && ppemitterprime2 < ppemitter2; i++)//We look through the list again, but this time we're looking for unused particles. We stop when we have all we need.
                    {
                        if (part[i].used == false)//If a specific particle is unused, we begin recycling it.
                        {
                            part[i].xyorg.X = emitx;//The origin of the particle.
                            part[i].xyorg.Y = emity;//The origin of the particle.
                            part[i].xyoffset.X = 0;//No movement yet.
                            part[i].xyoffset.Y = 0;//No movement yet.

                            part[i].lifeSpan = random.Next(5, 21);//The lifeSpan of the particle.
                            part[i].halfLife = 8;// The halfLife of the particle.
                            part[i].vspd = random.Next(1, 11);//The vspeed of the particle.
                            part[i].hspd = random.Next(-10, 11);//The hspeed of the particle.
                            part[i].clr = random.Next(0, 5);//There will be five different random colors in this sample.
                            part[i].size = size;//The size of the particle.

                            part[i].used = true;//We're using it.
                            existingParticles2 += 1;//We add to the total count.
                            ppemitterprime2 += 1;//Adding to the count.
                        }

                    }
                }

                if (ppemitterprime2 < ppemitter2)//If we STILL do not have enough particles, then we need to make some.
                {
                    while (ppemitterprime2 < ppemitter2)//This while loop will give us the rest of the paqrticles we need.
                    {
                        part.Add(new Particle());//This creates a new particle.
                        //part.Count-1 will give us the most recent particle made.
                        part[^1].xyorg.X = emitx;//The origin of the particle.
                        part[^1].xyorg.Y = emity;//The origin of the particle.
                        part[^1].xyoffset.X = 0;//No movement yet.
                        part[^1].xyoffset.Y = 0;//No movement yet.

                        part[^1].lifeSpan = random.Next(5, 21);//The lifeSpan of the particle.
                        part[^1].halfLife = 8;// The halfLife of the particle.
                        part[^1].vspd = random.Next(1, 11);//The vspeed of the particle.
                        part[^1].hspd = random.Next(-10, 11);//The hspeed of the particle.
                        part[^1].clr = random.Next(0, 5);//There will be five different random colors in this sample.
                        part[^1].size = size;//The size of the particle.

                        part[^1].used = true;//We're using it.
                        existingParticles2 += 1;//We add to the total count.
                        ppemitterprime2 += 1;//Adding to the count.
                    }
                }
            }

            #endregion
        }

        ///<summary>
        ///Creates trail particles along a given screen direction (dirx,
        ///diry) — used for the jet exhaust behind the ship. The plume is
        ///staggered along the direction with a slight lateral spread and
        ///graded from hot (at the tail) to cool (at the tip).
        ///</summary>
        public void particlesCreate(List<Particle> part, float emitx, float emity,
            int size, int particle, float dirx, float diry,
            float plumeLength, float lateralSpread)
        {
            if (particle == 1)
            {
                CreateTrailParticles(part, emitx, emity, size, dirx, diry,
                    plumeLength, lateralSpread);
            }
        }

        private void CreateTrailParticles(List<Particle> part, float emitx, float emity,
            int size, float dirx, float diry,
            float plumeLength = 0.0f, float lateralSpread = 0.0f)
        {
            bool directed = (dirx != 0.0f) || (diry != 0.0f);
            const int trailParticlesPerFrame = 4;

            //emit a few particles per frame, recycling the dead ones first
            for (int n = 0; n < trailParticlesPerFrame; n++)
            {
                int slot = -1;

                for (i = 0; i < part.Count; i++)
                {
                    if (part[i].used == false)//If a specific particle is unused, we begin recycling it.
                    {
                        slot = i;
                        break;
                    }
                }

                if (slot == -1)
                {
                    part.Add(new Particle());//This creates a new particle.
                    slot = part.Count - 1;
                }

                part[slot].xyorg.X = emitx;//The origin of the particle.
                part[slot].xyorg.Y = emity;//The origin of the particle.
                part[slot].xyoffset.X = 0;//No movement yet.
                part[slot].xyoffset.Y = 0;//No movement yet.

                part[slot].halfLife = 8;// The halfLife of the particle.

                if (directed)//Jet exhaust: stagger each particle along the
                {            //thrust line so the plume reads as a flame,
                             //not a blob sitting on one point.
                    float along = (float)random.NextDouble() * plumeLength;
                    float side = ((float)random.NextDouble() - 0.5f)
                        * lateralSpread;

                    //walk down the plume axis and jitter across it
                    part[slot].xyorg.X += dirx * along - diry * side;
                    part[slot].xyorg.Y += diry * along + dirx * side;

                    //the closer to the tail, the hotter the particle: the
                    //lifeSpan drives the bright-to-dark green color bands
                    float heat = 1.0f - (along / Math.Max(plumeLength, 0.001f));

                    part[slot].lifeSpan = 1 + (int)(heat * 7.0f);//8 (tail) .. 1 (tip)

                    //the plume thins out as it gets away from the tail
                    part[slot].size = Math.Max(1, (int)(size * (0.6f + 0.4f * heat)));
                }
                else
                {
                    part[slot].lifeSpan = random.Next(3, 9);//A short life: the flame fades quickly.
                    part[slot].size = size;
                }

                if (directed)
                {
                    int spd = random.Next(3, 9);

                    part[slot].hspd = (int)(dirx * spd);
                    part[slot].vspd = (int)(-diry * spd);
                }
                else
                {
                    part[slot].vspd = random.Next(1, 11);//The vspeed of the particle.
                    part[slot].hspd = random.Next(-10, 11);//The hspeed of the particle.
                }

                part[slot].clr = random.Next(0, 5);//There will be five different random colors in this sample.

                part[slot].used = true;//We're using it.
                existingParticles += 1;//We add to the total count.
            }
        }

        public void particlesUpdate(List<Particle> part, int particle)
        {
            #region Ship Trail

            if (particle == 1)
            {
                if (existingParticles > 0)
                {
                    for (i = 0; i < part.Count; i++)//Going through the list again.
                    {
                        if (part[i].used == true)//If the particle is being used, we update its position.
                        {
                            part[i].xyoffset.Y -= part[i].vspd - ((part[i].halfLife - part[i].lifeSpan) + 5);//We move the particle up with relevance to the halfLife. The 5 is an arbitrary number to make it look better.
                            part[i].xyoffset.X += part[i].hspd;//We move it left or right depending on the value of the hspeed.
                            part[i].rot = random.Next(0, 361);//We randomly rotate the particle.
                            part[i].lifeSpan -= 1;//We update the lifeSpan.
                            if (part[i].lifeSpan == 0)//If it's dead...
                            {
                                part[i].used = false;//We kill it.
                                existingParticles -= 1;//We subtract from the total count.
                            }
                        }
                    }
                }
            }

            #endregion

            #region Explosions 

            if (particle == 2)
            {
                if (existingParticles2 > 0)
                {
                    for (i = 0; i < part.Count; i++)//Going through the list again.
                    {
                        if (part[i].used == true)//If the particle is being used, we update its position.
                        {
                            part[i].xyoffset.Y -= part[i].vspd - ((part[i].halfLife - part[i].lifeSpan) + 5);//We move the particle up with relevance to the halfLife. The 5 is an arbitrary number to make it look better.
                            part[i].xyoffset.X += part[i].hspd;//We move it left or right depending on the value of the hspeed.
                            part[i].rot = random.Next(0, 361);//We randomly rotate the particle.
                            part[i].lifeSpan -= 1;//We update the lifeSpan.
                            if (part[i].lifeSpan == 0)//If it's dead...
                            {
                                part[i].used = false;//We kill it.
                                existingParticles2 -= 1;//We subtract from the total count.
                            }
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #region drawParticles

        public void drawParticles(GameTime gameTime, SpriteBatch spriteBatch, List<Particle> part, int drawParticle, Texture2D particleTexture)
        {
            Color color = new();
            this.particlesTexture = particleTexture;

            if (drawParticle == 1)
            {
                if (existingParticles > 0)
                {
                    for (i = 0; i < part.Count; i++)//We go through the list and draw the active particles.
                    {
                        //green plasma: bright when young, fading to dark green
                        if (part[i].lifeSpan > 6)
                        {
                            color = new Color((byte)random.Next(100, 180), (byte)255, (byte)random.Next(130, 200), (byte)255);
                        }

                        if (part[i].lifeSpan is > 3 and <= 6)
                        {
                            color = new Color((byte)random.Next(0, 60), (byte)255, (byte)random.Next(0, 80), (byte)255);
                        }

                        if (part[i].lifeSpan is > 0 and <= 3)
                        {
                            color = new Color((byte)0, (byte)random.Next(110, 180), (byte)random.Next(0, 40), (byte)255);
                        }

                        if (part[i].used == true)
                        {
                            Vector2 prtcl = new(part[i].xyorg.X, part[i].xyorg.Y);//The X and Y position to draw each particle.

                            spriteBatch.Draw(particlesTexture, prtcl, null, color, part[i].rot, Vector2.Zero, part[i].size, SpriteEffects.None, 0);//And we draw it.               
                        }
                    }
                }
            }

            if (drawParticle == 2)
            {
                if (existingParticles2 > 0)
                {
                    for (i = 0; i < part.Count; i++)//We go through the list and draw the active particles.
                    {
                        if (part[i].lifeSpan > 17)
                        {
                            color = new Color((byte)random.Next(175, 255), (byte)random.Next(150, 255), (byte)random.Next(100, 255), (byte)255);
                        }

                        if (part[i].lifeSpan is > 10 and < 17)
                        {
                            color = new Color((byte)random.Next(150, 175), (byte)random.Next(75, 150), (byte)random.Next(50, 100), (byte)255);
                        }

                        if (part[i].lifeSpan is > 5 and < 10)
                        {
                            color = new Color((byte)random.Next(0, 150), (byte)random.Next(0, 75), (byte)random.Next(0, 50), (byte)255);
                        }

                        if (part[i].used == true)
                        {
                            Vector2 prtcl = new(part[i].xyorg.X, part[i].xyorg.Y);//The X and Y position to draw each particle.
                            spriteBatch.Draw(particlesTexture, prtcl, null, color, part[i].rot, Vector2.Zero, part[i].size, SpriteEffects.None, 0);//And we draw it.
                        }
                    }
                }
            }
        }

        #endregion
    }
}
