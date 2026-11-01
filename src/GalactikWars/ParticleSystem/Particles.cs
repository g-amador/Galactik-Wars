#region File Description
//-----------------------------------------------------------------------------
//Particles.cs
//
//Author: GNPA & STX
//Microsoft XNA Community Game Platform
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using statements
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
    class Particles : GameScreen
    {
        #region Fields

        const int ppemitter = 100, ppemitter2 = 100;//The number of a type of particles allowed per emitter.
        int ppemitterprime, ppemitterprime2;//Real particles per emitter.
        int i;//Used in for loops.
        int existingParticles = 0, existingParticles2 = 0;//How many total particles are active.
        Random random = new Random();
        Texture2D particlesTexture;
        
        #endregion

        #region particlesCreate, particlesUpdate
        
        public void particlesCreate(List<Particle> part, float emitx, float emity, int size, int particle)
        {
            #region Ship Trail

            if (particle == 1)
            {
                ppemitterprime = 0;//We use this to find out how many particles are used in the emitter.
                
                for (i = 0; i < part.Count; i++)//This cycles through the list of particles to count how many are for the specified emitter. This is the first part in recycling the old particles.
                {
                    if (part[i].xyorg.X == emitx && part[i].xyorg.Y == emity && part[i].used == true)//Checking if the particle is active and if it belongs to the emitter placed at X,Y position.
                    {
                        ppemitterprime += 1;//Adding to the count.
                    }
                }

                if (ppemitterprime < ppemitter)// If not enough particles are given to the emitter, we'll add to them by recycling all the old ones we find in the list.
                {
                    for (i = 0; i < part.Count && ppemitterprime < ppemitter; i++)//We look through the list again, but this time we're looking for unused particles. We stop when we have all we need.
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
                            existingParticles += 1;//We add to the total count.
                            ppemitterprime += 1;//Adding to the count.
                        }

                    }
                }

                if (ppemitterprime < ppemitter)//If we STILL do not have enough particles, then we need to make some.
                {
                    while (ppemitterprime < ppemitter)//This while loop will give us the rest of the paqrticles we need.
                    {
                        part.Add(new Particle());//This creates a new particle.
                        //part.Count-1 will give us the most recent particle made.
                        part[part.Count - 1].xyorg.X = emitx;//The origin of the particle.
                        part[part.Count - 1].xyorg.Y = emity;//The origin of the particle.
                        part[part.Count - 1].xyoffset.X = 0;//No movement yet.
                        part[part.Count - 1].xyoffset.Y = 0;//No movement yet.

                        part[part.Count - 1].lifeSpan = random.Next(5, 21);//The lifeSpan of the particle.
                        part[part.Count - 1].halfLife = 8;// The halfLife of the particle.
                        part[part.Count - 1].vspd = random.Next(1, 11);//The vspeed of the particle.
                        part[part.Count - 1].hspd = random.Next(-10, 11);//The hspeed of the particle.
                        part[part.Count - 1].clr = random.Next(0, 5);//There will be five different random colors in this sample.
                        part[part.Count - 1].size = size;//The size of the particle.

                        part[part.Count - 1].used = true;//We're using it.
                        existingParticles += 1;//We add to the total count.
                        ppemitterprime += 1;//Adding to the count.
                    }
                }
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
                        part[part.Count - 1].xyorg.X = emitx;//The origin of the particle.
                        part[part.Count - 1].xyorg.Y = emity;//The origin of the particle.
                        part[part.Count - 1].xyoffset.X = 0;//No movement yet.
                        part[part.Count - 1].xyoffset.Y = 0;//No movement yet.

                        part[part.Count - 1].lifeSpan = random.Next(5, 21);//The lifeSpan of the particle.
                        part[part.Count - 1].halfLife = 8;// The halfLife of the particle.
                        part[part.Count - 1].vspd = random.Next(1, 11);//The vspeed of the particle.
                        part[part.Count - 1].hspd = random.Next(-10, 11);//The hspeed of the particle.
                        part[part.Count - 1].clr = random.Next(0, 5);//There will be five different random colors in this sample.
                        part[part.Count - 1].size = size;//The size of the particle.

                        part[part.Count - 1].used = true;//We're using it.
                        existingParticles2 += 1;//We add to the total count.
                        ppemitterprime2 += 1;//Adding to the count.
                    }
                }
            }

            #endregion
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
            Color color = new Color();
            this.particlesTexture = particleTexture;

            if (drawParticle == 1)
            {
                if (existingParticles > 0)
                {
                    for (i = 0; i < part.Count; i++)//We go through the list and draw the active particles.
                    {
                        if (part[i].lifeSpan > 17)
                        {
                            color = new Color(0, 0, (byte)random.Next(0, 100), 255);
                        }

                        if (part[i].lifeSpan > 10 && part[i].lifeSpan < 17)
                        {
                            color = new Color(0, 0, (byte)random.Next(145, 185), 255);
                        }

                        if (part[i].lifeSpan > 5 && part[i].lifeSpan < 10)
                        {
                            color = new Color(0, 0, (byte)random.Next(200, 255), 255);
                        }

                        if (part[i].used == true)
                        {
                            Vector2 prtcl = new Vector2(part[i].xyorg.X, part[i].xyorg.Y);//The X and Y position to draw each particle.
                            
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
                            color = new Color((byte)random.Next(175, 255), (byte)random.Next(150, 255), (byte)random.Next(100, 255), 255);
                        }

                        if (part[i].lifeSpan > 10 && part[i].lifeSpan < 17)
                        {
                            color = new Color((byte)random.Next(150, 175), (byte)random.Next(75, 150), (byte)random.Next(50, 100), 255);
                        }

                        if (part[i].lifeSpan > 5 && part[i].lifeSpan < 10)
                        {
                            color = new Color((byte)random.Next(0, 150), (byte)random.Next(0, 75), (byte)random.Next(0, 50), 255);
                        }

                        if (part[i].used == true)
                        {
                            Vector2 prtcl = new Vector2(part[i].xyorg.X, part[i].xyorg.Y);//The X and Y position to draw each particle.
                            spriteBatch.Draw(particlesTexture, prtcl, null, color, part[i].rot, Vector2.Zero, part[i].size, SpriteEffects.None, 0);//And we draw it.
                        }
                    }
                }
            }
        }

        #endregion
    }
}
