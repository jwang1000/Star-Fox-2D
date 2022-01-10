using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StarFox2D.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StarFox2D
{
    public class MainGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;


        public static Player Player;

        /// <summary>
        /// List of all enemies spawned in the level, including bosses.
        /// </summary>
        public static List<Classes.Object> Enemies;

        /// <summary>
        /// List of all buildings spawned in the level (both round and square), not including enemies.
        /// </summary>
        public static List<Classes.Object> Buildings;

        /// <summary>
        /// List of all bullets in the level, from players or enemies.
        /// </summary>
        public static List<Bullet> Bullets;

        public static Level CurrentLevel;

        public static int CurrentScore;

        /// <summary>
        /// The amount of time elapsed since the start of the level. Used for score and animation.
        /// </summary>
        public static TimeSpan CurrentTime;


        public static SpriteFont FontRegular;

        public static SpriteFont FontLarge;

        public static SpriteFont FontTitle;

        /// <summary>
        /// The speed that objects should move at to appear motionless against the background.
        /// X = 0, Y = 300.
        /// </summary>
        public static readonly Vector2 BackgroundObjectVelocity = new Vector2(0, 300);

        public static int ScreenWidth = 500;

        public static int ScreenHeight = 800;

        /// <summary>
        /// The number of pixels that an object can be outside the screen without despawning.
        /// </summary>
        public static readonly int DespawnBuffer = 10;


        /// <summary>
        /// Whether or not the player is currently in a level.
        /// </summary>
        private bool playingLevel;

        /// <summary>
        /// The position of the background image.
        /// Used for moving the background in a level.
        /// </summary>
        private Vector2 backgroundImagePosition;

        private MenuPages page;

        private float elapsedSeconds;

        private KeyboardState kstate;

        private Vector2 playerVelocity;

        /// <summary>
        /// The minimum y-value that the player can reach during a boss fight. (all range mode)
        /// </summary>
        private float playerBossBorder;

        /// <summary>
        /// Accumulates elapsed time in the update method until it reaches >1. Subtracts 1 afterwards.
        /// </summary>
        private double secondTimer;

        /// <summary>
        /// The current song playing. Necessary for switching from intro to the looped part of the song.
        /// </summary>
        private MusicIntroLoop currentSong;

        /// <summary>
        /// Set to true once all content is done loading. If this is true, then all textures are ready to use and the
        /// main menu music has already started.
        /// </summary>
        private bool allMediaLoaded;

        /// <summary>
        /// All buttons that currently exist on screen. Add to and remove from list as needed.
        /// </summary>
        private List<Button> buttons;

        private MouseState mouseState;



        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            playingLevel = false;
            page = MenuPages.TitleScreen;
            playerVelocity = Vector2.Zero;
            secondTimer = 0;
            buttons = new List<Button>();
        }

        protected override void Initialize()
        {
            // set window size
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load media
            FontRegular = Content.Load<SpriteFont>("FontRegular");
            FontLarge = Content.Load<SpriteFont>("FontLarge");
            FontTitle = Content.Load<SpriteFont>("FontTitle");
            LoadTextures();
            LoadSounds();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (!Textures.TexturesAreLoaded || !Sounds.SoundsAreLoaded)
            {
                return;
            }
            else if (!allMediaLoaded)
            {
                // complete final initialization needed

                // initialize classes that require content (i.e. sprites)
                backgroundImagePosition = new Vector2(Textures.Background.Width / 2, 0);
                playerBossBorder = ScreenHeight / 2;

                StartMusic();

                allMediaLoaded = true;
            }

            currentSong.Update(gameTime);


            // update buttons
            mouseState = Mouse.GetState();
            foreach (Button b in buttons)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && b.MouseHoversButton(mouseState.Position.ToVector2()))
                {
                    // TEMP testing only
                    b.Clicked(() => Debug.WriteLine("Clicked!" + CurrentTime));
                }
            }


            if (playingLevel)
            {
                CurrentTime += gameTime.ElapsedGameTime;

                // update level (spawns objects)
                CurrentLevel.Update(CurrentTime);


                // move all objects (players, enemies, etc)
                // TODO movement multipliers from effects
                kstate = Keyboard.GetState();
                elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

                // get input from player
                playerVelocity = Vector2.Zero;

                if (CurrentLevel.InBossFight)
                {
                    if (kstate.IsKeyDown(Keys.W))
                        playerVelocity.Y -= Player.BaseVelocity;
                    if (kstate.IsKeyDown(Keys.S))
                        playerVelocity.Y += Player.BaseVelocity;

                    // constrain movement vertically
                    if (Player.Position.Y <= playerBossBorder)
                        playerVelocity.Y = MathF.Max(0, playerVelocity.Y);
                    else if (Player.Position.Y >= ScreenHeight)
                        playerVelocity.Y = MathF.Min(0, playerVelocity.Y);
                }
                if (kstate.IsKeyDown(Keys.A))
                    playerVelocity.X -= Player.BaseVelocity;
                if (kstate.IsKeyDown(Keys.D))
                    playerVelocity.X += Player.BaseVelocity;

                // constrain player movement horizontally
                if (Player.Position.X <= 0)
                    playerVelocity.X = MathF.Max(0, playerVelocity.X);
                else if (Player.Position.X >= ScreenWidth)
                    playerVelocity.X = MathF.Min(0, playerVelocity.X);


                // call update methods simultaneously for all objects for additional logic
                Player.Position += playerVelocity * elapsedSeconds;
                Player.Update(CurrentTime);

                foreach (var enemy in Enemies)
                {
                    enemy.Position += enemy.Velocity * elapsedSeconds;
                    enemy.Update(CurrentTime);
                }
                foreach (var building in Buildings)
                {
                    building.Position += building.Velocity * elapsedSeconds;
                    building.Update(CurrentTime);
                }
                foreach (var bullet in Bullets)
                {
                    bullet.Position += bullet.Velocity * elapsedSeconds;
                    bullet.Update(CurrentTime);

                    // TODO check bullets against all enemies, buildings, and player
                    // TODO remove dead objects immediately
                }

                // all updates that occur once per second are here
                secondTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (secondTimer > 1)
                {
                    secondTimer -= 1;

                    // delete objects that are out of range
                    for (int i = Enemies.Count - 1; i >= 0; --i)
                    {
                        if (Enemies[i].ObjectIsOutsideScreen())
                        {
                            Enemies.RemoveAt(i);
                        }
                    }
                    for (int i = Buildings.Count - 1; i >= 0; --i)
                    {
                        if (Buildings[i].ObjectIsOutsideScreen())
                        {
                            Buildings.RemoveAt(i);
                        }
                    }
                    for (int i = Bullets.Count - 1; i >= 0; --i)
                    {
                        if (Bullets[i].ObjectIsOutsideScreen())
                        {
                            Bullets.RemoveAt(i);
                        }
                    }
                }
            }

            // TEMP TESTING
            if (gameTime.TotalGameTime > TimeSpan.FromSeconds(1) && !playingLevel)
            {
                StartLevel(LevelID.Corneria);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(Textures.Background, backgroundImagePosition, null, Color.White, 0f, new Vector2(Textures.Background.Width/2, Textures.Background.Height/2), Vector2.One, SpriteEffects.None, 0f);

            // Draw buttons
            foreach (Button b in buttons)
            {
                b.Draw(spriteBatch, mouseState.Position.ToVector2());
            }

            if (playingLevel)
            {
                Player.Draw(spriteBatch);
                foreach (var enemy in Enemies)
                    enemy.Draw(spriteBatch);
                foreach (var building in Buildings)
                    building.Draw(spriteBatch);
                foreach (var bullet in Bullets)
                    bullet.Draw(spriteBatch);

                // update background
                backgroundImagePosition.Y = (backgroundImagePosition.Y + 5) % (Textures.Background.Height / 2);
            }
            else
            {
                spriteBatch.DrawString(FontRegular, "v2.0", new Vector2(10, ScreenHeight - 25), Color.White);
                switch(page)
                {
                    case MenuPages.TitleScreen:
                        spriteBatch.DrawString(FontTitle, "STAR FOX 2D", new Vector2(50, 50), Color.White);
                        break;
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        private void LoadTextures()
        {
            try
            {
                Textures.AndrossHead = Content.Load<Texture2D>("andross-head");
                Textures.AndrossLeftHand = Content.Load<Texture2D>("andross-lh");
                Textures.AndrossRightHand = Content.Load<Texture2D>("andross-rh");

                Textures.Arwing = Content.Load<Texture2D>("arwing");
                Textures.ArwingFront = Content.Load<Texture2D>("arwing-front");

                Textures.Asteroid1 = Content.Load<Texture2D>("aster1");
                Textures.Asteroid2 = Content.Load<Texture2D>("aster2");
                Textures.Asteroid3 = Content.Load<Texture2D>("aster3");
                Textures.Asteroid4 = Content.Load<Texture2D>("aster4");
                Textures.Asteroid5 = Content.Load<Texture2D>("aster5");

                Textures.Background = Content.Load<Texture2D>("background");

                Textures.FilledCircle = Content.Load<Texture2D>("Circle");

                Textures.Debris1 = Content.Load<Texture2D>("debris1");
                Textures.Debris2 = Content.Load<Texture2D>("debris2");
                Textures.Debris3 = Content.Load<Texture2D>("debris3");

                Textures.Fly = Content.Load<Texture2D>("fly");

                Textures.FoxMcCloud = Content.Load<Texture2D>("fox");

                Textures.Granga = Content.Load<Texture2D>("granga");
                Textures.Hornet = Content.Load<Texture2D>("hornet");
                Textures.MiniAndross = Content.Load<Texture2D>("mini-andross");
                Textures.Mosquito = Content.Load<Texture2D>("mosquito");
                Textures.Queen = Content.Load<Texture2D>("queen");

                Textures.Ring = Content.Load<Texture2D>("Ring");

                Textures.Satellite1 = Content.Load<Texture2D>("sat1");
                Textures.Satellite2 = Content.Load<Texture2D>("sat2");
                Textures.Satellite3 = Content.Load<Texture2D>("sat3");

                Textures.TurretArm = Content.Load<Texture2D>("turret-arm");
                Textures.TurretBase = Content.Load<Texture2D>("turret-base");

                Textures.Wolfen = Content.Load<Texture2D>("wolfen");

                Textures.Button = new Texture2D(graphics.GraphicsDevice, 1, 1);
                Textures.Button.SetData(new[] { Color.White });

                Textures.TexturesAreLoaded = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error in loading textures:");
                Debug.WriteLine(e);
                Exit();
            }
        }

        private void LoadSounds()
        {
            try
            {
                SoundEffect corneriaIntro = Content.Load<SoundEffect>("music/CorneriaIntro");
                SoundEffect corneria = Content.Load<SoundEffect>("music/Corneria");
                Sounds.Corneria = new MusicIntroLoop(corneria, corneriaIntro);

                Sounds.SoundsAreLoaded = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error in loading sounds:");
                Debug.WriteLine(e);
                Exit();
            }
        }

        /// <summary>
        /// Starts the main menu music. Only necessary for the beginning of the game.
        /// </summary>
        private void StartMusic()
        {
            // TESTING play corneria and loop properly
            currentSong = Sounds.Corneria;
            currentSong.Start();
        }

        private void StartLevel(LevelID level)
        {
            CurrentTime = TimeSpan.Zero;
            CurrentLevel = new Level(level);

            // initialize Player
            // TEMP implement shield formula later
            int health = 50;
            Player = new Player(health, ObjectID.Player, 1, 0, 45, Textures.Arwing);
            Player.Position = new Vector2(250, 625);

            Enemies = new List<Classes.Object>();
            Buildings = new List<Classes.Object>();
            Bullets = new List<Bullet>();


            // TEMP
            buttons.Add(new Button(new Vector2(250, 200), 45, 45, Color.White, Color.AliceBlue, Textures.Button, Textures.Button));

            playingLevel = true;
        }
    }

    enum MenuPages
    {
        TitleScreen,
        LevelSelect,
        BackgroundStory,
        Help,
        Settings,
        About
    }
}
