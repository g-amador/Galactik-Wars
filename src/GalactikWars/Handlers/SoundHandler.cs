#region File Description
//-----------------------------------------------------------------------------
//SoundHandler.cs
//
//Author: GNPA & STX
//Ported from the XNA XACT audio engine to MonoGame SoundEffect playback.
//The original XACT project (GalactikWars.xap) defined cues for effects and
//music; this class keeps the same cue based API while loading the .wav
//files through the MonoGame content pipeline instead.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#endregion

namespace GalactikWars
{
    public class SoundHandler
    {
        #region Fields

        //Volume Variables
        public static float musicVolume = GameVariables.defaultMusicVolume;
        public static float effectsVolume = GameVariables.defaultEffectsVolume;

        //Cue names
        const string EffectsDirectory = "Audio/Waves/Effects/";
        const string MusicDirectory = "Audio/Waves/Music/";

        //Music cues, matching the cue names from the original XACT project.
        static readonly string[] musicCues =
        {
            "music", "music1", "music2", "music3", "music4", "music5",
            "gameOver",
        };

        //Loaded sound banks
        Dictionary<string, SoundEffect> effectSounds;
        Dictionary<string, SoundEffect> musicSounds;

        //Cues
        Cue myEngineSound = null;
        Cue othersEngineSound = null;
        Cue music = null;

        float timeDelta = 0.0f;

        bool loaded = false;

        Random random = new Random();

        #endregion Fields

        #region Cue Wrapper

        ///<summary>
        ///Minimal replacement for the XACT Cue class: wraps a
        ///SoundEffectInstance and exposes the play state API the game uses.
        ///</summary>
        class Cue
        {
            readonly SoundEffect sound;
            SoundEffectInstance instance;

            public string Name { get; private set; }

            public Cue(SoundEffect sound, string name, bool looped)
            {
                this.sound = sound;
                Name = name;
                Looped = looped;
            }

            public bool Looped { get; private set; }

            public bool IsPlaying
            {
                get { return instance != null && !instance.IsDisposed &&
                              instance.State == SoundState.Playing; }
            }

            public bool IsPaused
            {
                get { return instance != null && !instance.IsDisposed &&
                              instance.State == SoundState.Paused; }
            }

            public void Play()
            {
                Stop();

                instance = sound.CreateInstance();
                instance.IsLooped = Looped;
                instance.Volume = Looped ? musicVolume : effectsVolume;
                instance.Play();
            }

            public void Pause()
            {
                if (IsPlaying)
                {
                    instance.Pause();
                }
            }

            public void Resume()
            {
                if (IsPaused)
                {
                    instance.Resume();
                }
            }

            public void Stop()
            {
                if (instance != null)
                {
                    if (!instance.IsDisposed && instance.State != SoundState.Stopped)
                    {
                        instance.Stop();
                    }
                    instance.Dispose();
                    instance = null;
                }
            }

            ///<summary>
            ///Applies the current volume category, called every update so
            ///runtime volume changes take effect immediately.
            ///</summary>
            public void UpdateVolume(float volume)
            {
                if (instance != null && !instance.IsDisposed)
                {
                    instance.Volume = volume;
                }
            }
        }

        #endregion

        #region Initialization, Unload and Update

        ///<summary>
        ///Loads the sound banks from the content pipeline. Must be called
        ///from LoadContent before any sound is played.
        ///</summary>
        public void Load(ContentManager content)
        {
            if (loaded)
            {
                return;
            }

            effectSounds = new Dictionary<string, SoundEffect>();
            musicSounds = new Dictionary<string, SoundEffect>();

            try
            {
                //Initialize the effect bank.
                foreach (string name in effectCueNames)
                {
                    effectSounds[name] = content.Load<SoundEffect>(
                        EffectsDirectory + name);
                }

                //Initialize the music bank.
                foreach (string name in musicCues)
                {
                    musicSounds[name] = content.Load<SoundEffect>(
                        MusicDirectory + name);
                }

                music = new Cue(musicSounds["music"], "music", false);
            }
            catch (NoAudioHardwareException)
            {
                // silently fall back to silence
                effectSounds = null;
                musicSounds = null;
            }

            loaded = true;
        }

        ///<summary>Cue names available in the effects bank.</summary>
        static readonly string[] effectCueNames =
        {
            "engine_1",
            "engine_2",
            "explosion1",
            "explosion2",
            "explosion3",
            "explosion4",
            "hax2_fire",
            "hyperspace_activate",
            "phase_collide_ship1",
            "shield_collide_ship1",
            "tx0_fire1",
        };

        public void UnLoad()
        {
            if (myEngineSound != null)
            {
                myEngineSound.Stop();
                myEngineSound = null;
            }

            if (othersEngineSound != null)
            {
                othersEngineSound.Stop();
                othersEngineSound = null;
            }

            if (music != null)
            {
                music.Stop();
                music = null;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (musicSounds != null)
            {
                timeDelta += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (timeDelta >= 0)
                {
                    timeDelta = 0.0f;

                    //Keep the volume categories in sync with the settings.
                    music.UpdateVolume(musicVolume);
                    if (myEngineSound != null)
                    {
                        myEngineSound.UpdateVolume(effectsVolume);
                    }
                    if (othersEngineSound != null)
                    {
                        othersEngineSound.UpdateVolume(effectsVolume);
                    }

                    if (!music.IsPlaying)
                    {
                        if (random.NextDouble() <= 0.16)
                        {
                            PlayMusic("music");
                        }
                        else if (random.NextDouble() <= 0.32)
                        {
                            PlayMusic("music1");
                        }
                        else if (random.NextDouble() <= 0.48)
                        {
                            PlayMusic("music2");
                        }
                        else if (random.NextDouble() <= 0.64)
                        {
                            PlayMusic("music3");
                        }
                        else if (random.NextDouble() <= 0.8)
                        {
                            PlayMusic("music4");
                        }
                        else
                        {
                            PlayMusic("music5");
                        }
                    }
                }
            }
        }

        ///<summary>
        ///Switches to the specified music cue, applying the music volume.
        ///</summary>
        void PlayMusic(string cue)
        {
            if (music != null)
            {
                music.Stop();
            }

            music = new Cue(musicSounds[cue], cue, false);
            music.Play();
        }

        #endregion Initialization and Update

        #region Pause and Resume

        #region Pause and Resume Music

        public void ResumeMusic()
        {
            if (music != null)
            {
                if (music.IsPaused)
                    music.Resume();
            }
        }

        public void PauseMusic()
        {
            if (music != null)
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
            if (effectSounds != null)
            {
                if (myEngineSound == null)
                {
                    myEngineSound = new Cue(effectSounds["engine_1"],
                                            "engine_1", true);
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
            if (effectSounds != null)
            {
                if (othersEngineSound == null)
                {
                    othersEngineSound = new Cue(effectSounds["engine_2"],
                                                "engine_2", true);
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
            if (effectSounds != null && effectSounds.ContainsKey(cue))
            {
                //One-shot cues use the effects volume.
                Cue oneShot = new Cue(effectSounds[cue], cue, false);
                oneShot.Play();
            }
        }

        public void PlayGameOver()
        {
            if (musicSounds != null && musicSounds.ContainsKey("gameOver"))
            {
                if (music == null || !music.Name.Equals("gameOver"))
                {
                    if (music != null && music.IsPlaying)
                    {
                        music.Stop();
                    }
                    music = new Cue(musicSounds["gameOver"], "gameOver", false);
                    music.Play();
                }
            }
        }

        #endregion Play
    }
}
