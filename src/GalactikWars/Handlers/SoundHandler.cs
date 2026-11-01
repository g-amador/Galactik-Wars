#region File Description
//-----------------------------------------------------------------------------
//SoundHandler.cs
//
//Author: GNPA & STX
//Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace GalactikWars
{
    public class SoundHandler
    {
        #region Fields

        //Volume Variables
        public static float musicVolume = GameVariables.defaultMusicVolume;
        public static float effectsVolume = GameVariables.defaultEffectsVolume;

        //Audio components
        public AudioEngine audioEngine;
        public WaveBank effectsWaveBank;
        public SoundBank effectsSoundBank;
        public WaveBank musicsWaveBank;
        public SoundBank musicsSoundBank;

        //Cues 
        public Cue myEngineSound = null;
        public Cue othersEngineSound = null;
        public Cue music = null;

        private float timeDelta = 0.0f;

        Random random = new Random();

        #endregion Fields

        #region Initialization, Unload and Update

        public SoundHandler()
        { 
            try
            {
                //Initialize audio objects.
                audioEngine = new AudioEngine("Content\\Audio\\GalactikWars.xgs");
                effectsWaveBank = new WaveBank(audioEngine, "Content\\Audio\\Effects.xwb");
                effectsSoundBank = new SoundBank(audioEngine, "Content\\Audio\\Effects.xsb");
                musicsWaveBank = new WaveBank(audioEngine, "Content\\Audio\\Music.xwb");
                musicsSoundBank = new SoundBank(audioEngine, "Content\\Audio\\Music.xsb");

                music = musicsSoundBank.GetCue("music");
            }
            catch (NoAudioHardwareException)
            {
                // silently fall back to silence
                audioEngine = null;
                effectsWaveBank = null;
                effectsSoundBank = null;
                musicsWaveBank = null;
                musicsSoundBank = null;
            }
        }

        public void UnLoad()
        {
            audioEngine.Dispose();
            effectsWaveBank.Dispose();
            effectsSoundBank.Dispose();
            musicsWaveBank.Dispose();
            musicsSoundBank.Dispose();

            //Cues 
            if (myEngineSound != null)
            {
                myEngineSound.Dispose();
            }

            if (othersEngineSound != null)
            {
                othersEngineSound.Dispose();
            }
            if (music != null)
            {
                music.Dispose();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (audioEngine != null)
            {
                timeDelta += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (timeDelta >= 0)
                {
                    timeDelta = 0.0f;

                    audioEngine.GetCategory("Music").SetVolume(SoundHandler.musicVolume);
                    audioEngine.GetCategory("Effects").SetVolume(SoundHandler.effectsVolume);

                    if (!music.IsPlaying)
                    {
                        if (random.NextDouble() <= 0.16)
                        {
                            music = musicsSoundBank.GetCue("music");
                            music.Play();
                        }
                        else if (random.NextDouble() <= 0.32)
                        {
                            music = musicsSoundBank.GetCue("music1");
                            music.Play();
                        }
                        else if (random.NextDouble() <= 0.48)
                        {
                            music = musicsSoundBank.GetCue("music2");
                            music.Play();
                        }
                        else if (random.NextDouble() <= 0.64)
                        {
                            music = musicsSoundBank.GetCue("music3");
                            music.Play();
                        }
                        else if (random.NextDouble() <= 0.8)
                        {
                            music = musicsSoundBank.GetCue("music4");
                            music.Play();
                        }
                        else
                        {
                            music = musicsSoundBank.GetCue("music5");
                            music.Play();
                        }
                    }
                    audioEngine.Update();
                }
            }
        }

        #endregion Initialization and Update

        #region Pause and Resume

        #region Pause and Resume Music

        public void ResumeMusic()
        {
            if (audioEngine != null)
            {
                if (music.IsPaused)
                    music.Resume();
            }
        }

        public void PauseMusic()
        {
            if (audioEngine != null)
            {
                if (music.IsPlaying)
                    music.Pause();
            }
        }

        #endregion Pause and Resume Music

        #region Pause Engine Sound

        public void PauseMyEngineSound()
        {
            if (myEngineSound != null && myEngineSound.IsPlaying)
            {
                myEngineSound.Pause();
            }
        }

        public void PauseOthersEngineSound()
        {
            if (othersEngineSound != null && othersEngineSound.IsPlaying)
            {
                othersEngineSound.Pause();
            }
        }

        #endregion Pause Engine Sound

        #endregion Pause

        #region Play

        #region Play Engine Sound

        public void PlayMyEngineSound()
        {
            if (audioEngine != null)
            {
                if (myEngineSound == null)
                {
                    myEngineSound = effectsSoundBank.GetCue("engine_1");
                    myEngineSound.Play();
                }
                else if (myEngineSound.IsPaused)
                {
                    myEngineSound.Resume();
                }
            }
        }

        public void PlayOthersEngineSound()
        {
            if (audioEngine != null)
            {
                if (othersEngineSound == null)
                {
                    othersEngineSound = effectsSoundBank.GetCue("engine_2");
                    othersEngineSound.Play();
                }
                else if (othersEngineSound.IsPaused)
                {
                    othersEngineSound.Resume();
                }
            }
        }

        #endregion Play Engine Sound

        public void PlayCue(string cue)
        {
            if (audioEngine != null)
            {
                effectsSoundBank.PlayCue(cue);
            }
        }

        public void PlayGameOver()
        {
            if (audioEngine != null)
            {
                if (!music.Name.Equals("gameOver"))
                {
                    if (music.IsPlaying)
                    {
                        music.Stop(AudioStopOptions.Immediate);
                    }
                    music = musicsSoundBank.GetCue("gameOver");
                    music.Play();
                }
            }
        }

        #endregion Play
    }
}
