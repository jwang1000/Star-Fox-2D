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
        /// The next song to play. Only contains a song if a transition is occurring, otherwise is null.
        /// </summary>
        private MusicIntroLoop nextSong;

        /// <summary>
        /// Set to true once all content is done loading. If this is true, then all textures are ready to use and the
        /// main menu music has already started.
        /// </summary>
        private bool allMediaLoaded;

        /// <summary>
        /// All buttons that currently exist on screen. Add to and remove from list as needed.
        /// </summary>
        private List<Button> buttons;

        /// <summary>
        /// All text that is on screen (which are not part of any other component, i.e. buttons).
        /// </summary>
        private List<TextBox> text;

        private MouseState mouseState;

        private MouseState lastMouseState;



        private List<TextBox> mainMenuText;



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
            text = new List<TextBox>();

            mainMenuText = new List<TextBox>();
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
            LoadFonts();
            LoadTextures();
            LoadSounds();
            ChangeMenu(MenuPages.TitleScreen);
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
                // complete any remaining initialization

                // initialize classes that require content (i.e. sprites)
                backgroundImagePosition = new Vector2(Textures.Background.Width / 2, 0);
                playerBossBorder = ScreenHeight / 2;
                lastMouseState = Mouse.GetState();

                StartMusic();

                text = mainMenuText;

                allMediaLoaded = true;
            }

            currentSong.Update(gameTime);
            MusicUpdate();


            // update buttons
            mouseState = Mouse.GetState();
            if (mouseState.LeftButton != lastMouseState.LeftButton)
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                    MouseLeftClicked();
                else
                    MouseLeftReleased();
            }
            lastMouseState = mouseState;


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
            /*
            if (gameTime.TotalGameTime > TimeSpan.FromSeconds(1) && !playingLevel)
            {
                StartLevel(LevelID.Corneria);
                text = new List<TextBox>();
            }*/

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);

            // Draw background
            spriteBatch.Draw(Textures.Background, backgroundImagePosition, null, Color.White, 0f, new Vector2(Textures.Background.Width/2, Textures.Background.Height/2), Vector2.One, SpriteEffects.None, 0f);

            // All menu text and buttons are handled by the following:

            // Draw buttons
            foreach (Button b in buttons)
            {
                b.Draw(spriteBatch, mouseState.Position.ToVector2());
            }

            // Draw text
            foreach (TextBox t in text)
            {
                t.Draw(spriteBatch);
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

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void LoadFonts()
        {
            TextBox.FontSmall = Content.Load<SpriteFont>("FontSmall");
            TextBox.FontRegular = Content.Load<SpriteFont>("FontRegular");
            TextBox.FontLarge = Content.Load<SpriteFont>("FontLarge");
            TextBox.FontTitle = Content.Load<SpriteFont>("FontTitle");
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

                Textures.Button = Content.Load<Texture2D>("Button");
                // outdated
                //Textures.Button = new Texture2D(graphics.GraphicsDevice, 1, 1);
                //Textures.Button.SetData(new[] { Color.White });

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

                SoundEffect menuTheme = Content.Load<SoundEffect>("music/Title");
                SoundEffect mapTheme = Content.Load<SoundEffect>("music/MapSelect");
                Sounds.Menu = new MusicIntroLoop(mapTheme, menuTheme);

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
            currentSong = Sounds.Menu;
            currentSong.Start();
            nextSong = null;
        }

        /// <summary>
        /// Handles transitioning between songs.
        /// </summary>
        private void MusicUpdate()
        {
            if (nextSong != null)
            {
                if (currentSong.State == MusicIntroLoop.MusicState.Stopped)
                {
                    currentSong = nextSong;
                    currentSong.Start();
                    nextSong = null;
                }
                else if (currentSong.State != MusicIntroLoop.MusicState.FadingOut)
                {
                    currentSong.FadeOut();
                }
            }
        }

        /// <summary>
        /// Called when a button is pressed to go to another menu. Fills the button and text lists.
        /// </summary>
        private void ChangeMenu(MenuPages page)
        {
            mainMenuText.Clear();
            buttons.Clear();
            switch (page)
            {
                case MenuPages.TitleScreen:
                    mainMenuText.Add(new TextBox("v2.0", new Vector2(10, ScreenHeight - 25)));
                    mainMenuText.Add(new TextBox("STAR FOX 2D", new Vector2(70, 50), FontSize.Title));

                    // temp - change to level select button after
                    buttons.Add(new Button(new Vector2(250, 395), 200, 50, Color.Blue, Color.CornflowerBlue, () => StartLevel(LevelID.Corneria), Textures.Button, text: "Start Game"));
                    break;
            }
        }

        private void StartLevel(LevelID level)
        {
            mainMenuText.Clear();
            buttons.Clear();
            CurrentTime = TimeSpan.Zero;
            CurrentLevel = new Level(level);
            nextSong = CurrentLevel.LevelMusic;  // begins the transition process for music

            // initialize Player
            // TEMP implement shield formula later
            int health = 50;
            Player = new Player(health, ObjectID.Player, 1, 0, 45, Textures.Arwing);
            Player.Position = new Vector2(250, 625);

            Enemies = new List<Classes.Object>();
            Buildings = new List<Classes.Object>();
            Bullets = new List<Bullet>();


            // TEMP
            buttons.Add(new Button(new Vector2(250, 200), 45, 45, () => Debug.WriteLine("Clicked!" + CurrentTime), Textures.Button));

            playingLevel = true;
        }

        /// <summary>
        /// Called once when the left mouse button is clicked.
        /// </summary>
        private void MouseLeftClicked()
        {
            // fire player bullets here TODO
            if (playingLevel)
            {

            }
        }

        /// <summary>
        /// Called once when the left mouse button is released.
        /// </summary>
        private void MouseLeftReleased()
        {
            // check for button presses here
            try
            {
                foreach (Button b in buttons)
                {
                    if (b.MouseHoversButton(mouseState.Position.ToVector2()))
                    {
                        b.Clicked();
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Buttons list was modified as it was looping - this should only happen on changing menus or loading a level");
            }
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
