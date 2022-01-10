﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    /// <summary>
    /// Class containing all sound effects and music in the game.
    /// </summary>
    public static class Sounds
    {
        public static bool SoundsAreLoaded = false;

        public static float SoundEffectVolume = 1f;

        public static float MusicVolume = 0.25f;  // temp set to 0.25

        public static MusicIntroLoop Corneria;
    }

    /// <summary>
    /// Handles intros and looping sections of music.
    /// NOTE: Uses SoundEffects instead of Songs in order to work with .wav files (and avoid the terror of .mp3).
    /// Best practice is to only load the music necessary and unload after the scene ends. However, we have <10 soundtracks, so loading all is okay for now.
    /// </summary>
    public class MusicIntroLoop
    {
        public enum MusicState
        {
            Stopped,
            PlayingIntro,
            PlayingMainSection,
            FadingOut
        }

        public bool HasIntro { get; private set; }

        public MusicState State { get; private set; }

        public SoundEffect Intro { get; private set; }

        private SoundEffectInstance IntroInstance;

        public float IntroVolume { get; set; }

        public SoundEffect MainSection { get; private set; }

        private SoundEffectInstance MainSectionInstance;

        public float MainSectionVolume { get; set; }

        private TimeSpan IntroTimeSpent;

        public TimeSpan FadeoutTimeRemaining { get; private set; }


        public MusicIntroLoop(SoundEffect mainSection, SoundEffect intro = null)
        {
            MainSection = mainSection;
            MainSectionInstance = MainSection.CreateInstance();
            MainSectionInstance.IsLooped = true;
            MainSectionVolume = 1;
            State = MusicState.Stopped;

            if (intro != null)
            {
                Intro = intro;
                IntroInstance = Intro.CreateInstance();
                IntroVolume = 1;
                HasIntro = true;
                IntroTimeSpent = new TimeSpan(0);
            }

            ChangeVolume();
        }

        public void Start()
        {
            if (HasIntro)
            {
                IntroInstance.Play();
                State = MusicState.PlayingIntro;
            }
            else
            {
                MainSectionInstance.Play();
                State = MusicState.PlayingMainSection;
            }
        }

        /// <summary>
        /// Called when the music volume is changed in the settings menu.
        /// </summary>
        public void ChangeVolume()
        {
            if (HasIntro)
                IntroInstance.Volume = IntroVolume * Sounds.MusicVolume;
            MainSectionInstance.Volume = MainSectionVolume * Sounds.MusicVolume;
        }

        public void Update(GameTime gameTime)
        {
            if (State == MusicState.FadingOut)
            {
                // TODO
                FadeoutTimeRemaining -= gameTime.ElapsedGameTime;
                if (FadeoutTimeRemaining < TimeSpan.Zero)
                    Stop();
                else
                {

                }
            }
            else if (State == MusicState.PlayingIntro)
            {
                IntroTimeSpent += gameTime.ElapsedGameTime;
                if (IntroTimeSpent >= Intro.Duration)
                {
                    MainSectionInstance.Play();
                    State = MusicState.PlayingMainSection;
                }
            }
        }

        public void Stop()
        {
            if (State == MusicState.PlayingIntro)
            {
                IntroInstance.Stop();
            }
            MainSectionInstance.Stop();

            State = MusicState.Stopped;
            IntroTimeSpent = TimeSpan.Zero;
        }

        public void FadeOut(int seconds = 2)
        {
            State = MusicState.FadingOut;
            FadeoutTimeRemaining = new TimeSpan(0, 0, seconds);
        }
    }
}