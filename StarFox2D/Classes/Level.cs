using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class Level
    {
        /// <summary>
        /// Which level this instance is. Starts from 1.
        /// </summary>
        public LevelID LevelNumber { get; private set; }

        /// <summary>
        /// Whether or not the player has reached the boss fight section of the level.
        /// </summary>
        public bool InBossFight { get; private set; }

        /// <summary>
        /// The length of time after the last object is spawned before the boss text appears.
        /// InBossFight is not true during this time.
        /// </summary>
        public TimeSpan TimeBeforeBossText { get; private set; }

        /// <summary>
        /// The length of time that the boss text displays for before the boss fight begins.
        /// InBossFight is true once this time STARTS.
        /// </summary>
        public TimeSpan BossTextTime { get; private set; }

        private FullLevelDetails levelDetails;

        private TimeSpan lastSpawnedObjectTime;

        private bool spawningIsValid;

        private bool spawningIsDone;

        private long nextSpawnTimeTicks;


        public Level(LevelID levelNumber)
        {
            LevelNumber = levelNumber;

            levelDetails = LevelOutline.GetLevelDetails(levelNumber);
            lastSpawnedObjectTime = new TimeSpan(0);
            nextSpawnTimeTicks = LevelOutline.FramestoTicks(levelDetails.ObjectsToSpawn[0].SpawnTime);
        }



        public void Update(TimeSpan levelTime)
        {
            if (!InBossFight)
            {
                if (spawningIsDone)
                {
                    if (levelTime >= lastSpawnedObjectTime + TimeBeforeBossText)
                    {
                        InBossFight = true;
                    }
                }
                else
                {
                    spawningIsValid = levelTime.Ticks >= lastSpawnedObjectTime.Ticks + nextSpawnTimeTicks;

                    if (spawningIsValid)
                    {
                        Object o = CreateObject(levelDetails.ObjectsToSpawn[levelDetails.SpawnIndex].ObjectToSpawn);
                        if (o is Enemy)
                            MainGame.Enemies.Add(o);
                        else
                            MainGame.Buildings.Add(o);

                        lastSpawnedObjectTime += new TimeSpan(nextSpawnTimeTicks);
                        levelDetails.SpawnIndex++;

                        if (levelDetails.SpawnIndex == levelDetails.ObjectsToSpawn.Length)
                        {
                            spawningIsDone = true;
                        }
                        else
                        {
                            nextSpawnTimeTicks = LevelOutline.FramestoTicks(levelDetails.ObjectsToSpawn[levelDetails.SpawnIndex].SpawnTime);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Given the ID of an object and optional horizontal range it can spawn in, returns an instance of the object.
        /// TODO remove horizonatal range? Can just have boolean to randomize or not
        /// </summary>
        public Object CreateObject(ObjectID id, int startX = 0, int endX = 0)
        {
            Object o;
            switch (id)
            {
                case ObjectID.Fly:
                    o = new Enemy(2, id, 1, 5, 20, Textures.Fly);
                    o.Position = new Vector2(250, -10);
                    o.Velocity = new Vector2(50, 400);
                    break;


                default:
                    // TEMP create asteroid
                    o = new RoundObject(1, ObjectID.Asteroid, 0, 1, 25, Textures.Asteroid1);
                    o.Position = new Vector2(250, -30);
                    o.Velocity = new Vector2(0, 350);
                    break;
            }

            return o;
        }
    }
}
