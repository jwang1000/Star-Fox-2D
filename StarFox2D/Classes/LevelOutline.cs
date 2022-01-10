using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public static class LevelOutline
    {
        public static FullLevelDetails GetLevelDetails(LevelID levelID)
        {
            switch (levelID)
            {
                case LevelID.Corneria:
                    return Corneria;
                case LevelID.Asteroid:
                    return Asteroid;
                case LevelID.SpaceArmada:
                    return SpaceArmada;
                case LevelID.Meteor:
                    return Meteor;
                case LevelID.Venom:
                    return Venom;
            }

            Debug.WriteLine("ERROR: GetLevelOutline was given a level ID that was not in covered by a switch case");
            return Corneria;
        }


        public static long FramestoTicks(int frames)
        {
            return ((long)frames) * 1000000 / 6;  // 1e7 ticks per second, at 60 fps per second (1 zero removed from both for less computation)
        }


        private static ObjectSpawn[] corneriaObjectSpawns =
        {
            new ObjectSpawn(ObjectID.Fly, 55)
        };
        private static ObjectID[] corneriaBossSpawns = { ObjectID.Granga };
        private static ScreenText[] corneriaStartText = { new ScreenText("Star Fox! Andross sent me to stop you!", new Vector2(35, 380)) };
        private static ScreenText[] corneriaEndText = 
        { 
            new ScreenText("Once Andross hears about this, you're", new Vector2(40, 370)),
            new ScreenText("through, you hear me? Through!", new Vector2(70, 400))
        };
        public static readonly FullLevelDetails Corneria = new FullLevelDetails(corneriaObjectSpawns, corneriaBossSpawns, corneriaStartText, corneriaEndText);




        public static readonly FullLevelDetails Asteroid = new FullLevelDetails();




        public static readonly FullLevelDetails SpaceArmada = new FullLevelDetails();




        public static readonly FullLevelDetails Meteor = new FullLevelDetails();




        public static readonly FullLevelDetails Venom = new FullLevelDetails();
    }

    public struct FullLevelDetails
    {
        /// <summary>
        /// Which object should be spawned next. Spawning is finished if this equals the length of ObjectsToSpawn.
        /// </summary>
        public int SpawnIndex;

        /// <summary>
        /// Object Spawns should be listed in order from earliest to latest.
        /// </summary>
        public ObjectSpawn[] ObjectsToSpawn;

        public ObjectID[] BossesToSpawn;

        public ScreenText[] BossStartText;

        public ScreenText[] BossEndText;

        public FullLevelDetails(ObjectSpawn[] objectsToSpawn, ObjectID[] bossesToSpawn, ScreenText[] bossStartText, ScreenText[] bossEndText)
        {
            SpawnIndex = 0;
            ObjectsToSpawn = objectsToSpawn;
            BossesToSpawn = bossesToSpawn;
            BossStartText = bossStartText;
            BossEndText = bossEndText;
        }
    }


    public struct ObjectSpawn
    {
        public ObjectID ObjectToSpawn;

        /// <summary>
        /// SpawnTime is given by the time since the last spawn (change from Python version).
        /// Should be specified in number of frames, not ticks! (Will be converted later)
        /// </summary>
        public int SpawnTime;
        public int LeftX;
        public int RightX;

        public ObjectSpawn(ObjectID objectToSpawn, int spawnTime, int leftX = 0, int rightX = 0)
        {
            ObjectToSpawn = objectToSpawn;
            SpawnTime = spawnTime;
            LeftX = leftX;
            RightX = rightX;
        }
    }

    public struct ScreenText
    {
        public string Line;
        public Vector2 TopLeft;

        public ScreenText(string line, Vector2 topLeft)
        {
            Line = line;
            TopLeft = topLeft;
        }
    }

    public enum LevelID
    {
        Corneria,
        Asteroid,
        SpaceArmada,
        Meteor,
        Venom
    }
}
