using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public static class SaveData
    {
        /// <summary>
        /// The highest level copmleted. Ranges from -1 to 4.
        /// </summary>
        public static int LevelCompleted;

        /// <summary>
        /// The highest score achieved in each level.
        /// </summary>
        public static int[] HighScores;

        /// <summary>
        /// Saves only completed levels and high scores.
        /// </summary>
        public static void SaveLevelData()
        {
            // update HighScores with MainGame.CurrentScore before writing to disk
            int level = (int)MainGame.CurrentLevel.LevelNumber;
            HighScores[level] = Math.Max(HighScores[level], MainGame.CurrentScore);

            if (MainGame.CurrentLevel.State == LevelState.Win)
                LevelCompleted = Math.Max(LevelCompleted, (int)MainGame.CurrentLevel.LevelNumber);

            Debug.WriteLine("SaveLevelData called");
            Debug.WriteLine("LevelCompleted " + LevelCompleted);
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
