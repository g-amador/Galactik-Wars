#region File Description
//-----------------------------------------------------------------------------
//SoundHandler.cs
//
//Author: GNPA & STX
//Ported from the XNA XACT audio engine to MonoGame SoundEffect playback.
//The original XACT project (GalactikWars.xap) defined cues for effects and
//music; this class keeps the same cue based API for effects while music is
//organized as named tracks (one per game state), loaded through the MonoGame
//content pipeline instead.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
#endregion

namespace GalactikWars.Handlers
{
    public class SoundHandler
    {
        #region Fields

        //Volume Variables
        public static float musicVolume = GameVariables.defaultMusicVolume;
        public static float effectsVolume = GameVariables.defaultEffectsVolume;

        //Cue names
        private const string EffectsDirectory = "audio/Effects/";
        private const string MusicDirectory = "audio/Music/";

        //Music tracks, one per game state. The names match the .wav files
        //in assets/audio/Music (loaded via the content pipeline).
        public const string MenuTrack = "menu";
        public const string LoadingTrack = "loading";
        public const string BattleTrack = "battle";
        public const string LobbyTrack = "slow-travel";
        public const string GameOverTrack = "in-the-wreckage";
        public const string CreditsTrack = "meet-the-princess";

        private static readonly string[] musicTrackNames =
        {
            MenuTrack, LoadingTrack, BattleTrack, LobbyTrack,
            GameOverTrack, CreditsTrack,
        };

        //Loaded sound banks
        private Dictionary<string, SoundEffect> effectSounds;
        private Dictionary<string, SoundEffect> musicSounds;

        //Cues
        private Cue myEngineSound = null;
        private Cue othersEngineSound = null;
        private Cue music = null;

        /// <summary>The track currently selected (see the *Track constants).</summary>
        private string currentTrack = null;

        private float timeDelta = 0.0f;

        private bool loaded = false;

        #endregion Fields

        #region Cue Wrapper

        ///<summary>
        ///Minimal replacement for the XACT Cue class: wraps a
        ///SoundEffectInstance and exposes the play state API the game uses.
        ///</summary>
        private class Cue
        {
            private readonly SoundEffect sound;
            private SoundEffectInstance instance;

            /// <summary>
            /// Volume category this cue belongs to (music or effects),
            /// applied at play time and kept in sync by UpdateVolume.
            /// </summary>
            private readonly float baseVolume;

            public string Name { get; private set; }

            public Cue(SoundEffect sound, string name, bool looped,
                       float initialVolume)
            {
                this.sound = sound;
                Name = name;
                Looped = looped;
                baseVolume = initialVolume;
            }

            public bool Looped { get; private set; }

            public bool IsPlaying
            {
                get
                {
                    return instance != null && !instance.IsDisposed &&
                              instance.State == SoundState.Playing;
                }
            }

            public bool IsPaused
            {
                get
                {
                    return instance != null && !instance.IsDisposed &&
                              instance.State == SoundState.Paused;
                }
            }

            public void Play()
            {
                Stop();

                instance = sound.CreateInstance();
                instance.IsLooped = Looped;
                instance.Volume = baseVolume;
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

            effectSounds = [];
            musicSounds = [];

            try
            {
                //Initialize the effect bank.
                foreach (string name in effectCueNames)
                {
                    effectSounds[name] = content.Load<SoundEffect>(
                        EffectsDirectory + name);
                }

                //Initialize the music bank.
                foreach (string name in musicTrackNames)
                {
                    musicSounds[name] = content.Load<SoundEffect>(
                        MusicDirectory + name);
                }

                //The menu track plays until a screen selects another one.
                PlayTrack(MenuTrack);
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
        private static readonly string[] effectCueNames =
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

            //Note: the shared music cue is intentionally not stopped here —
            //it keeps playing across screen transitions (menus, gameplay).
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
                }
            }
        }

        ///<summary>
        ///Switches the background music to the named track (see the *Track
        ///constants). Idempotent: selecting the track that is already
        ///playing keeps it playing untouched. Tracks loop until another
        ///track is selected.
        ///</summary>
        public void PlayTrack(string trackName)
        {
            if (musicSounds == null || trackName == currentTrack)
            {
                return;
            }

            if (!musicSounds.ContainsKey(trackName))
            {
                return;
            }

            currentTrack = trackName;

            if (music != null)
            {
                music.Stop();
            }

            music = new Cue(musicSounds[trackName], trackName, true,
                            musicVolume);
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
                {
                    music.Resume();
                }
            }
        }

        public void PauseMusic()
        {
            if (music != null)
            {
                if (music.IsPlaying)
                {
                    music.Pause();
                }
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
                                            "engine_1", true, effectsVolume);
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
                                                "engine_2", true,
                                                effectsVolume);
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
                Cue oneShot = new(effectSounds[cue], cue, false,
                                      effectsVolume);
                oneShot.Play();
            }
        }

        public void PlayGameOver()
        {
            PlayTrack(GameOverTrack);
        }

        #endregion Play
    }
}
