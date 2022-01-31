using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public static class SaveData
    {
        /// <summary>
        /// The highest level copmleted. Ranges from 0-5.
        /// </summary>
        public static int LevelCompleted;

        /// <summary>
        /// The highest score achieved in each level.
        /// </summary>
        public static int[] HighScores;

        /// <summary>
        /// Saves only completed levels and high scores.
        /// </summary>
        public static void SaveLevelData(object state)
        {
            Debug.WriteLine("SaveLevelData called");
        }

        /// <summary>
        /// Saves only menu settings (controls, volumes)
        /// </summary>
        public static void SaveSettings()
        {
            Debug.WriteLine("SaveSettings called");
        }

        /// <summary>
        /// Loads completed levels, high scores, and settings.
        /// </summary>
        public static void Load(object state)
        {
            Debug.WriteLine("Load called");
        }
    }
}
