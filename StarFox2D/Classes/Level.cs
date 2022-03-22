﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class Level
    {
        /// <summary>
        /// Which level this instance is.
        /// </summary>
        public LevelID LevelNumber { get; private set; }

        /// <summary>
        /// What stage the level has progressed to. Spawns objects/the boss accordingly; draw text in MainGame accordingly.
        /// </summary>
        public LevelState State { get; private set; }

        /// <summary>
        /// The length of time before spawning can occur at the start of the level. Screen displays "Ready?" at this time, or similar.
        /// </summary>
        public TimeSpan TimeBeforeLevelStart { get; private set; }

        /// <summary>
        /// The length of time after the last object is spawned before the boss text appears.
        /// </summary>
        public TimeSpan TimeBeforeBossText { get; private set; }

        /// <summary>
        /// The length of time that the boss text displays for before the boss fight begins.
        /// </summary>
        public TimeSpan BossStartTextDisplayTime { get; private set; }

        /// <summary>
        /// The length of time that the boss text displays for after the boss is defeated.
        /// </summary>
        public TimeSpan BossEndTextDisplayTime { get; private set; }

        public MusicIntroLoop LevelMusic { get; private set; }

        public string BossStartText { get; private set; }

        public string BossEndText { get; private set; }

        private FullLevelDetails levelDetails;

        private TimeSpan lastSpawnedObjectTime;
        private TimeSpan bossStartTextTime;
        private TimeSpan bossDefeatTime;

        private bool spawningIsValid;

        private long nextSpawnTimeTicks;


        public Level(LevelID levelNumber, int timeBeforeLevelStartSeconds = 3, int timeBeforeBossTextSeconds = 5, int bossStartTextSeconds = 5, int bossEndTextSeconds = 5)
        {
            LevelNumber = levelNumber;

            levelDetails = LevelOutline.GetLevelDetails(levelNumber);
            nextSpawnTimeTicks = LevelOutline.FramestoTicks(levelDetails.ObjectsToSpawn[0].FramesSinceLastSpawn);
            LevelMusic = levelDetails.Music;

            TimeBeforeLevelStart = new TimeSpan(0, 0, timeBeforeLevelStartSeconds);
            TimeBeforeBossText = new TimeSpan(0, 0, timeBeforeBossTextSeconds);
            BossStartTextDisplayTime = new TimeSpan(0, 0, bossStartTextSeconds);
            BossEndTextDisplayTime = new TimeSpan(0, 0, bossEndTextSeconds);
            BossStartText = levelDetails.BossStartText;
            BossEndText = levelDetails.BossEndText;
            State = LevelState.BeforeStart;
            lastSpawnedObjectTime = TimeBeforeLevelStart;
            bossStartTextTime = TimeBeforeLevelStart;
        }



        public void Update(TimeSpan levelTime)
        {
            switch (State)
            {
                case LevelState.BeforeStart:
                    if (levelTime >= TimeBeforeLevelStart)
                    {
                        State = LevelState.Main;
                    }
                    break;

                case LevelState.Main:
                    spawningIsValid = levelTime.Ticks >= lastSpawnedObjectTime.Ticks + nextSpawnTimeTicks;

                    if (spawningIsValid)
                    {
                        MainGame.Objects.Add(CreateObject(levelDetails.ObjectsToSpawn[levelDetails.SpawnIndex].ObjectToSpawn));

                        lastSpawnedObjectTime += new TimeSpan(nextSpawnTimeTicks);
                        levelDetails.SpawnIndex++;

                        if (levelDetails.SpawnIndex == levelDetails.ObjectsToSpawn.Length)
                        {
                            State = LevelState.DoneSpawning;
                        }
                        else
                        {
                            nextSpawnTimeTicks = LevelOutline.FramestoTicks(levelDetails.ObjectsToSpawn[levelDetails.SpawnIndex].FramesSinceLastSpawn);
                        }
                    }
                    break;

                case LevelState.DoneSpawning:
                    if (levelTime >= lastSpawnedObjectTime + TimeBeforeBossText && MainGame.Objects.Count == 0)
                    {
                        bossStartTextTime = levelTime;
                        State = LevelState.BossStartText;
                    }
                    break;

                case LevelState.BossStartText:
                    if (MainGame.Boss == null)
                        CreateBoss(levelDetails.BossToSpawn);

                    if (levelTime >= bossStartTextTime + BossStartTextDisplayTime)
                        State = LevelState.BossFight;
                    break;

                case LevelState.BossFight:
                    if (!MainGame.Boss.IsAlive)
                    {
                        State = LevelState.BossEndText;
                        bossDefeatTime = levelTime;
                    }
                    break;

                case LevelState.BossEndText:
                    if (levelTime >= bossDefeatTime + BossEndTextDisplayTime)
                    {
                        State = LevelState.Win;
                        MainGame.FinishLevel();
                    }
                    break;
            }
        }


        /// <summary>
        /// Given the ID of an object, returns an instance of the object.
        /// Does not create bosses. Does not add the object to the list of objects in MainGame.
        /// </summary>
        public Object CreateObject(ObjectID id)
        {
            Object o = new RoundObject(1, ObjectID.Asteroid, 0, 1, 25, Textures.Asteroid1)
            {
                Position = new Vector2(250, -30),
                Velocity = MainGame.BackgroundObjectVelocity
            };
            int randInt;
            Vector2 pos;
            Vector2 vel;
            Texture2D texture;

            switch (id)
            {
                case ObjectID.Fly:
                    o = new RoundEnemy(2, id, 10, 5, 20, Textures.Fly)
                    {
                        Position = new Vector2(250, -10),
                        Velocity = new Vector2(50, 400)
                    };
                    break;

                case ObjectID.Mosquito:
                    randInt = MainGame.Random.Next(1, 3);
                    pos = new Vector2(125, -10);

                    if (randInt == 2)
                        pos.X = 250;
                    else if (randInt == 3)
                        pos.X = 375;

                    o = new RoundEnemy(3, id, 10, 5, 24, Textures.Mosquito)
                    {
                        Position = pos,
                        Velocity = new Vector2(50, 400)
                    };
                    break;

                case ObjectID.Hornet:
                    // 3 are spawned at once - return 1, add 2 directly
                    o = new RoundEnemy(2, id, 5, 5, 25, Textures.Hornet)
                    {
                        Position = new Vector2(100, -10),
                        Velocity = new Vector2(180, 480)
                    };
                    MainGame.Objects.Add(new RoundEnemy(2, id, 5, 5, 25, Textures.Hornet)
                    {
                        Position = new Vector2(250, -10),
                        Velocity = new Vector2(180, 480)
                    });
                    MainGame.Objects.Add(new RoundEnemy(2, id, 5, 5, 25, Textures.Hornet)
                    {
                        Position = new Vector2(400, -10),
                        Velocity = new Vector2(180, 480)
                    });
                    break;

                case ObjectID.QueenFly:
                    randInt = MainGame.Random.Next(1, 3);
                    pos = new Vector2(125, -10);

                    if (randInt == 2)
                        pos.X = 250;
                    else if (randInt == 3)
                        pos.X = 375;

                    o = new RoundEnemy(4, id, 10, 5, 30, Textures.Queen, EffectType.Slow)
                    {
                        Position = pos,
                        Velocity = new Vector2(50, 400)
                    };
                    break;

                case ObjectID.MiniAndross:
                    o = new RoundEnemy(15, id, 5, 30, 32, Textures.MiniAndross)
                    {
                        Position = new Vector2(0, -10),
                        Velocity = new Vector2(300, 300)
                    };
                    break;

                case ObjectID.Asteroid:
                    randInt = MainGame.Random.Next(1, 3);
                    pos = new Vector2(150, -30);

                    if (randInt == 1)
                    {
                        texture = Textures.Asteroid1;
                    }
                    else if (randInt == 2)
                    {
                        pos.X = 250;
                        texture = Textures.Asteroid2;
                    }
                    else
                    {
                        pos.X = 350;
                        texture = Textures.Asteroid3;
                    }

                    o = new RoundObject(1, ObjectID.Asteroid, 0, 1, 25, texture)
                    {
                        Position = pos,
                        Velocity = MainGame.BackgroundObjectVelocity
                    };
                    break;

                case ObjectID.SmallAsteroid:
                    randInt = MainGame.Random.Next(1, 2);
                    pos = new Vector2(150, -30);

                    if (randInt == 1)
                    {
                        pos.X = 200;
                        texture = Textures.Asteroid4;
                    }
                    else
                    {
                        pos.X = 400;
                        texture = Textures.Asteroid5;
                    }

                    o = new RoundObject(1, ObjectID.SmallAsteroid, 0, 1, 20, texture)
                    {
                        Position = pos,
                        Velocity = MainGame.BackgroundObjectVelocity
                    };
                    break;

                case ObjectID.Debris:
                    // debris move in random directions
                    randInt = MainGame.Random.Next(1, 3);
                    pos = new Vector2(150, -30);
                    vel = new Vector2((float)MainGame.Random.Next(0, 100) / 100, (float)MainGame.Random.Next(50, 70) / 10);

                    if (randInt == 1)
                    {
                        texture = Textures.Debris1;
                    }
                    else if (randInt == 2)
                    {
                        pos.X = 300;
                        texture = Textures.Debris2;
                    }
                    else
                    {
                        pos.X = 425;
                        texture = Textures.Debris3;
                    }

                    o = new RoundObject(1, ObjectID.Debris, 0, 1, 20, texture)
                    {
                        Position = pos,
                        Velocity = vel
                    };
                    break;

                case ObjectID.Satellite:
                    randInt = MainGame.Random.Next(1, 3);
                    pos = new Vector2(125, -30);
                    vel = MainGame.BackgroundObjectVelocity;

                    if (randInt == 1)
                    {
                        texture = Textures.Satellite1;
                    }
                    else if (randInt == 2)
                    {
                        pos.X = 250;
                        texture = Textures.Satellite2;
                    }
                    else
                    {
                        pos.X = 375;
                        texture = Textures.Satellite3;
                    }

                    o = new SquareObject(2, ObjectID.Satellite, 0, 1, 55, texture)
                    {
                        Position = pos,
                        Velocity = vel
                    };
                    break;

                case ObjectID.Turret:
                    break;

                case ObjectID.RingWhite:
                    o = new Ring(1, id, 0, 10, 20, 10, 0, Textures.RingThick, Color.White)
                    {
                        Position = new Vector2(100, -10),
                        Velocity = new Vector2(0, 200)
                    };
                    break;

                case ObjectID.RingYellow:
                    o = new Ring(1, id, 0, 10, 20, 20, 10, Textures.RingThick, Color.Yellow)
                    {
                        Position = new Vector2(250, -10),
                        Velocity = new Vector2(0, 200)
                    };
                    break;

                case ObjectID.RingGreen:
                    o = new Ring(1, id, 0, 10, 20, 40, 10, Textures.RingThick, Color.Green)
                    {
                        Position = new Vector2(375, -10),
                        Velocity = new Vector2(0, 200)
                    };
                    break;

                case ObjectID.RingRed:
                    o = new Ring(1, id, 0, 10, 20, 500, 5, Textures.RingThick, Color.Red)
                    {
                        Position = new Vector2(250, -10),
                        Velocity = new Vector2(0, 200)
                    };
                    break;

                default:
                    Debug.WriteLine("Error: tried to create object " + id);
                    break;
            }

            return o;
        }

        /// <summary>
        /// Given the ID of a boss, creates the Boss and sets MainGame.Boss.
        /// </summary>
        public void CreateBoss(ObjectID id)
        {
            Object boss = null;

            switch (id)
            {
                case ObjectID.Granga:
                    boss = new Granga(100, id, 2, 150, 50, Textures.Granga)
                    {
                        Position = new Vector2(250, 200),
                        Velocity = new Vector2(240)
                    };
                    break;

                case ObjectID.MechaTurret:
                    break;

                case ObjectID.GrangaRematch:
                    break;

                case ObjectID.StarWolfTeam:
                    break;

                case ObjectID.Andross:
                    break;

                default:
                    Debug.WriteLine("Error: tried to create boss when given ID " + id);
                    break;
            }

            MainGame.Boss = boss;
        }

        /// <summary>
        /// Should only be called by the Player class.
        /// </summary>
        public void PlayerDeath()
        {
            State = LevelState.Loss;
            MainGame.FinishLevel();
            // sets MainGame.Update to show game over screen
        }
    }

    public enum LevelState
    {
        Loss,
        BeforeStart,
        Main,
        DoneSpawning,
        BossStartText,
        BossFight,
        BossEndText,
        Win
    }
}
