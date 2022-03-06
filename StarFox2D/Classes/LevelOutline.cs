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
                case LevelID.Corneria:  // Granga
                    return Corneria;
                case LevelID.Asteroid:  // Mecha Turret
                    return Asteroid;
                case LevelID.SpaceArmada:  // Granga 2
                    return SpaceArmada;
                case LevelID.Meteor:  // Star Wolf
                    return Meteor;
                case LevelID.Venom:  // Andross
                    return Venom;
            }

            Debug.WriteLine("ERROR: GetLevelOutline was given a level ID that was not in covered by a switch case");
            return Corneria;
        }


        public static long FramestoTicks(int frames)
        {
            return ((long)frames) * 1000000 / 6;  // 1e7 ticks per second, at 60 fps per second (1 zero removed from both for less computation)
        }

        /*
         * ID VALUES:
        1 = player
        2, 4 = Granga, Granga rematch
        3 = Mecha Turret
        5 = Star Wolf crew (5.1 = wolf, 5.2 = pigma)
        6 = Andross (6 = head, 6.1 = left hand, 6.2 = right hand)
        10 = fly
        20, 21, 22 = mosquito
        30 = hornet, three spawn at once
        40, 41, 42 = queen fly
        50 = mini andross
        100, 101, 102 = asteroid (round)
        110, 111 = small_asteroid (round)
        120, 121, 122 = debris (round)
        500, 501, 502 = satellite (square)
        510, 511, 512 = turret (square)
        1000, 1001, 1002 = ring (white ring, heal 15)
        1010, 1011, 1012 = yl_ring (yellow ring, heal 20, increase max shield by 10)
        1020, 1021, 1022 = gr_ring (green ring, heal 40, increase max shield by 10)
        1030, 1031, 1032 = re_ring (red ring, heal full, increase max shield by 5)
        """
    
        if level == 1:
            b_spawn_list = []  # square building spawn list
            e_spawn_list = [[55, 10], [45, 10], [38, 10], [34, 10], [32, 10], [30, 10], [25, 10],
                            [23, 10], [22, 10], [18, 10], [15, 10], [10, 10], [7, 10]]  # enemy spawn list
            rb_spawn_list = [[50, 100], [50, 110], [48, 100], [45, 110], [42, 111], [42, 1000], [40, 111],
                             [37, 110], [34, 102], [32, 110], [30, 100], [29, 1002], [27, 110], [26, 100],
                             [26, 101], [26, 110], [25, 110], [24, 100], [22, 102], [22, 110], [21, 111],
                             [21, 101], [20, 102], [17, 110], [15, 100], [12, 1001], [11, 101]]  # round building and ring spawn list
         */

        private static ObjectSpawn[] corneriaObjectSpawns =
        {
            new ObjectSpawn(ObjectID.Fly, 60),
            new ObjectSpawn(ObjectID.RingWhite, 10)
            /*new ObjectSpawn(ObjectID.Fly, 300),
            new ObjectSpawn(ObjectID.Fly, 300),
            new ObjectSpawn(ObjectID.Fly, 280),
            new ObjectSpawn(ObjectID.Fly, 280),
            new ObjectSpawn(ObjectID.Fly, 260),
            new ObjectSpawn(ObjectID.Fly, 260),
            new ObjectSpawn(ObjectID.Fly, 240),
            new ObjectSpawn(ObjectID.Fly, 240),
            new ObjectSpawn(ObjectID.Fly, 200),
            new ObjectSpawn(ObjectID.Fly, 200),
            new ObjectSpawn(ObjectID.Fly, 180),
            new ObjectSpawn(ObjectID.Fly, 60)*/
        };
        private static ObjectID corneriaBoss = ObjectID.Granga;
        private static string corneriaStartText = "Star Fox! Andross sent me to stop you!";
        private static string corneriaEndText = "Once Andross hears about this, you're through, you hear me? Through!";
        public static readonly FullLevelDetails Corneria = new FullLevelDetails(Sounds.Corneria, corneriaObjectSpawns, corneriaBoss, corneriaStartText, corneriaEndText);




        public static readonly FullLevelDetails Asteroid = new FullLevelDetails();




        public static readonly FullLevelDetails SpaceArmada = new FullLevelDetails();




        public static readonly FullLevelDetails Meteor = new FullLevelDetails();




        public static readonly FullLevelDetails Venom = new FullLevelDetails();
    }

    public struct FullLevelDetails
    {
        public MusicIntroLoop Music;

        /// <summary>
        /// Which object should be spawned next. Spawning is finished if this equals the length of ObjectsToSpawn.
        /// </summary>
        public int SpawnIndex;

        /// <summary>
        /// Object Spawns should be listed in order from earliest to latest.
        /// </summary>
        public ObjectSpawn[] ObjectsToSpawn;

        public ObjectID BossToSpawn;

        public string BossStartText;

        public string BossEndText;

        public FullLevelDetails(MusicIntroLoop music, ObjectSpawn[] objectsToSpawn, ObjectID bossToSpawn, string bossStartText, string bossEndText)
        {
            Music = music;
            SpawnIndex = 0;
            ObjectsToSpawn = objectsToSpawn;
            BossToSpawn = bossToSpawn;
            BossStartText = bossStartText;
            BossEndText = bossEndText;
        }
    }


    public struct ObjectSpawn
    {
        public ObjectID ObjectToSpawn;

        /// <summary>
        /// TimeSinceLastSpawn is given by the time since the last spawn (change from Python version).
        /// Should be specified in number of frames (@60FPS), not ticks! (converted in Level.Update)
        /// </summary>
        public int FramesSinceLastSpawn;
        public int LeftX;
        public int RightX;

        public ObjectSpawn(ObjectID objectToSpawn, int framesSinceLastSpawn, int leftX = 0, int rightX = 0)
        {
            ObjectToSpawn = objectToSpawn;
            FramesSinceLastSpawn = framesSinceLastSpawn;
            LeftX = leftX;
            RightX = rightX;
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
