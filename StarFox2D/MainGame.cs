using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StarFox2D.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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

        private MenuPages menuPage;

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
        /// Does not include sliders for the settings menu.
        /// </summary>
        private List<Button> buttons;

        /// <summary>
        /// All sliders in the settings menu.
        /// </summary>
        private List<Slider> settingsSliders;

        /// <summary>
        /// All text that is on screen (which are not part of any other component, i.e. buttons).
        /// </summary>
        private List<TextBox> text;

        private MouseState mouseState;
        private MouseState lastMouseState;

        private string loadingText;
        private ControlScheme controlScheme;
        private Button WASDControlScheme;
        private Button ArrowKeysControlScheme;
        private Color WASDControlSchemeButtonColour;
        private Color ArrowKeysControlSchemeButtonColour;
        private TextBox ControlSchemeDescription;



        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            playingLevel = false;
            menuPage = MenuPages.TitleScreen;
            playerVelocity = Vector2.Zero;
            secondTimer = 0;

            buttons = new List<Button>();
            settingsSliders = new List<Slider>();
            text = new List<TextBox>();
            loadingText = "Loading";
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

            // load fonts synchronously to draw text
            // TODO may need to load one or two sprites for loading screen animation
            LoadFonts();

            // asynchronously load textures and sounds
            ThreadPool.QueueUserWorkItem(LoadTextures);
            ThreadPool.QueueUserWorkItem(LoadSounds);
        }

        protected override void Update(GameTime gameTime)
        {
            if (!Textures.TexturesAreLoaded || !Sounds.SoundsAreLoaded)
            {
                return;
            }
            else if (!allMediaLoaded)
            {
                // complete any remaining initialization
                ChangeMenu(MenuPages.TitleScreen);
                CreatePersistentMenuItems();
                SetControlScheme(ControlScheme.WASD);

                // initialize classes that require content (i.e. sprites)
                backgroundImagePosition = new Vector2(Textures.Background.Width / 2, 0);
                playerBossBorder = ScreenHeight / 2;
                lastMouseState = Mouse.GetState();

                StartMusic();

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
            else
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    MouseLeftHold();
                }
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
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);

            if (!Textures.TexturesAreLoaded || !Sounds.SoundsAreLoaded)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds % 1000 < 333)
                    loadingText = "Loading.";
                else if (gameTime.TotalGameTime.TotalMilliseconds % 1000 < 667)
                    loadingText = "Loading..";
                else
                    loadingText = "Loading...";
                spriteBatch.DrawString(TextBox.FontLarge, loadingText, new Vector2(175, ScreenHeight / 2 - TextBox.FontLarge.LineSpacing), Color.White);
                spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            // Draw background
            spriteBatch.Draw(Textures.Background, backgroundImagePosition, null, Color.White, 0f, new Vector2(Textures.Background.Width/2, Textures.Background.Height/2), Vector2.One, SpriteEffects.None, 0f);

            // All menu text and buttons are handled by the following:
            foreach (Button b in buttons)
            {
                b.Draw(spriteBatch, mouseState.Position.ToVector2());
            }
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
            else
            {
                // used for drawing additional graphics in various menus
                switch(menuPage)
                {
                    case MenuPages.TitleScreen:
                        spriteBatch.Draw(Textures.ArwingFront, new Vector2(255, 242), null, Color.White, 0, new Vector2(Textures.ArwingFront.Width/2), new Vector2((float)275 / Textures.ArwingFront.Width), SpriteEffects.None, 0f);
                        break;
                    case MenuPages.Settings:
                        foreach (Slider s in settingsSliders)
                        {
                            s.Draw(spriteBatch, mouseState.Position.ToVector2());
                        }
                        break;
                    case MenuPages.BackgroundStory:
                        spriteBatch.Draw(Textures.FoxMcCloud, new Vector2(146, 630), null, Color.White, 0, new Vector2(Textures.FoxMcCloud.Width / 2), new Vector2((float)275 / Textures.FoxMcCloud.Width), SpriteEffects.None, 0f);
                        break;
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void LoadFonts()
        {
            TextBox.FontSmall = Content.Load<SpriteFont>("FontSmall");
            TextBox.FontMedium = Content.Load<SpriteFont>("FontMedium");
            TextBox.FontRegular = Content.Load<SpriteFont>("FontRegular");
            TextBox.FontLarge = Content.Load<SpriteFont>("FontLarge");
            TextBox.FontTitle = Content.Load<SpriteFont>("FontTitle");
        }


        private void LoadTextures(object state)
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

        private void LoadSounds(object state)
        {
            try
            {
                SoundEffect corneriaIntro = Content.Load<SoundEffect>("music/CorneriaIntro");
                SoundEffect corneria = Content.Load<SoundEffect>("music/Corneria");
                Sounds.Corneria = new MusicIntroLoop(corneria, corneriaIntro);

                SoundEffect menuTheme = Content.Load<SoundEffect>("music/Title");
                SoundEffect mapTheme = Content.Load<SoundEffect>("music/MapSelect");
                Sounds.Menu = new MusicIntroLoop(mapTheme, menuTheme);

                SoundEffect asteroidIntro = Content.Load<SoundEffect>("music/AsteroidIntro");
                SoundEffect asteroid = Content.Load<SoundEffect>("music/Asteroid");
                Sounds.Asteroid = new MusicIntroLoop(asteroid, asteroidIntro);

                SoundEffect spaceArmadaIntro = Content.Load<SoundEffect>("music/SectorAlphaIntro");
                SoundEffect SpaceArmada = Content.Load<SoundEffect>("music/SectorAlpha");
                Sounds.SpaceArmada = new MusicIntroLoop(SpaceArmada, spaceArmadaIntro);

                SoundEffect meteorIntro = Content.Load<SoundEffect>("music/StarWolfIntro");
                SoundEffect meteor = Content.Load<SoundEffect>("music/StarWolf");
                Sounds.Meteor = new MusicIntroLoop(meteor, meteorIntro);

                SoundEffect venomIntro = Content.Load<SoundEffect>("music/CorneriaZeroIntro");
                SoundEffect venom = Content.Load<SoundEffect>("music/CorneriaZero");
                Sounds.Venom = new MusicIntroLoop(venom, venomIntro);

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
        /// Creates sliders and all menu items (text, buttons) that need to be modified later or should only be created once.
        /// </summary>
        private void CreatePersistentMenuItems()
        {
            settingsSliders.Add(new Slider(new Vector2(ScreenWidth / 2, 230), ScreenWidth - 100, 30, 15, (float value) => Sounds.SoundEffectVolume = value, Textures.FilledCircle, Textures.Button, startValue: Sounds.SoundEffectVolume));
            settingsSliders.Add(new Slider(new Vector2(ScreenWidth / 2, 360), ScreenWidth - 100, 30, 15, (float value) => SetMusicVolume(value), Textures.FilledCircle, Textures.Button, startValue: Sounds.MusicVolume));
            WASDControlScheme = new Button(new Vector2(150, 500), 160, 50, WASDControlSchemeButtonColour, Color.CornflowerBlue, () => SetControlScheme(ControlScheme.WASD), Textures.Button, text: "WASD");
            ArrowKeysControlScheme = new Button(new Vector2(ScreenWidth - 150, 500), 160, 50, ArrowKeysControlSchemeButtonColour, Color.CornflowerBlue, () => SetControlScheme(ControlScheme.ArrowKeys), Textures.Button, text: "Arrow Keys");
            ControlSchemeDescription = new TextBox("", new Vector2(0, 525), new Vector2(ScreenWidth, 100), FontSize.Medium);
        }

        /// <summary>
        /// Called when a button is pressed to go to another menu. Fills the button and text lists.
        /// </summary>
        private void ChangeMenu(MenuPages page)
        {
            text.Clear();
            buttons.Clear();
            Button.ClickAction backButtonAction = () => ChangeMenu(MenuPages.TitleScreen);
            Vector2 backButtonPosition = new Vector2(375, 700);
            string backButtonText = "Back";
            switch (page)
            {
                case MenuPages.TitleScreen:
                    text.Add(new TextBox("STAR FOX 2D", new Vector2(70, 50), FontSize.Title));

                    buttons.Add(new Button(new Vector2(250, 400), 200, 50, Color.Blue, Color.CornflowerBlue, () => ChangeMenu(MenuPages.LevelSelect), Textures.Button, text: "Level Select"));
                    buttons.Add(new Button(new Vector2(250, 490), 200, 50, Color.Blue, Color.CornflowerBlue, () => ChangeMenu(MenuPages.Settings), Textures.Button, text: "Settings"));
                    buttons.Add(new Button(new Vector2(250, 580), 200, 50, Color.Blue, Color.CornflowerBlue, () => ChangeMenu(MenuPages.Help), Textures.Button, text: "Help"));
                    buttons.Add(new Button(new Vector2(250, 670), 200, 50, Color.Blue, Color.CornflowerBlue, () => ChangeMenu(MenuPages.About), Textures.Button, text: "About"));
                    backButtonPosition = new Vector2(425, 775);
                    backButtonText = "Exit";
                    backButtonAction = () => Exit();
                    break;

                case MenuPages.Settings:
                    text.Add(new TextBox("Settings", new Vector2(25, 40), new Vector2(ScreenWidth - 50, 50), FontSize.Large));

                    text.Add(new TextBox("Sound Effects Volume", new Vector2(25, 130), new Vector2(ScreenWidth - 50, 100), FontSize.Medium));
                    text.Add(new TextBox("Music Volume", new Vector2(25, 260), new Vector2(ScreenWidth - 50, 100), FontSize.Medium));
                    text.Add(new TextBox("Control Scheme", new Vector2(25, 390), new Vector2(ScreenWidth - 50, 100), FontSize.Medium));
                    text.Add(ControlSchemeDescription);

                    buttons.Add(WASDControlScheme);
                    buttons.Add(ArrowKeysControlScheme);
                    break;

                case MenuPages.Help:
                    text.Add(new TextBox("Help", new Vector2(25, 40), new Vector2(ScreenWidth - 50, 50), FontSize.Large));

                    string controlType;
                    string rollKey;
                    if (controlScheme == ControlScheme.WASD)
                    {
                        controlType = "WASD";
                        rollKey = "the spacebar";
                    }
                    else
                    {
                        controlType = "the arrow keys";
                        rollKey = "RCTRL";
                    }
                    text.Add(new TextBox("Use " + controlType + " to control the Arwing! Click anywhere on screen to shoot lasers. Destroy enemies and obstacles to get points.",
                        new Vector2(25, 110), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("Running into enemies, obstacles, or their bullets will deplete your shield health. If your shield drops to 0, it's game over!",
                        new Vector2(25, 200), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("Press " + rollKey + " to do a barrel roll, which makes you invincible for a short time. You can't shoot while you're rolling though, and you'll need to wait before you can roll again.",
                        new Vector2(25, 290), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("If you see any rings, you should run into them. They will give you points and restore your shield, and some can have other effects. Find out what they are!",
                        new Vector2(25, 405), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("There is a boss battle at the end of each level. While in this battle, the Arwing enters All-Range mode, meaning that you can move freely around the screen. " +
                        "However, while not in All-Range Mode, you can only move left and right.",
                        new Vector2(25, 520), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("Make sure to read the background story if you're not familiar with Star Fox. It will clear some things up.",
                        new Vector2(25, 730), new Vector2(ScreenWidth - 50, 100), FontSize.Small, true));
                    break;

                case MenuPages.About:
                    text.Add(new TextBox("About", new Vector2(25, 40), new Vector2(ScreenWidth - 50, 100), FontSize.Large));

                    text.Add(new TextBox("Written by Jonathan Wang, Jan. 1 - 2021. Original version written Mar. 9 - May 24, 2017.", 
                        new Vector2(25, 150), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("Thanks to Ms. Stusiak, Richard Gan, and Andrew Luo for help with many various issues in the original!",
                        new Vector2(25, 215), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("This game was (re)written as a technical exercise to learn Monogame. I do not own any of the images, characters, music, or any other IP that appears in this game. " +
                        "All rights belong to their rightful owners, namely Nintendo.",
                        new Vector2(25, 310), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("There is no auto-click option in this game as it would make it too easy. Yes, I did extensive testing. Enjoy tendinitis. :)",
                        new Vector2(25, 460), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    break;

                case MenuPages.LevelSelect:
                    text.Add(new TextBox("Level Select", new Vector2(25, 30), new Vector2(ScreenWidth - 50, 50), FontSize.Large));

                    buttons.Add(new Button(new Vector2(ScreenWidth / 2, 125), 300, 40, Color.Blue, Color.CornflowerBlue, () => ChangeMenu(MenuPages.BackgroundStory), Textures.Button, text: "Background Story"));

                    buttons.Add(new Button(new Vector2(150, 225), 75, 75, Color.Blue, Color.CornflowerBlue, () => StartLevel(LevelID.Corneria), Textures.Button, text: "1"));
                    text.Add(new TextBox("Corneria", new Vector2(75, 260), new Vector2(150, 50), FontSize.Small));

                    // TODO check that each level being added can be played - if not, set colours to gray and don't do anything on click/hover
                    buttons.Add(new Button(new Vector2(ScreenWidth - 150, 225), 75, 75, Color.Gray, Color.Gray, () => Debug.WriteLine("Level 2"), Textures.Button, text: "2"));
                    text.Add(new TextBox("Asteroid", new Vector2(ScreenWidth - 225, 260), new Vector2(150, 50), FontSize.Small));

                    buttons.Add(new Button(new Vector2(150, 400), 75, 75, Color.Gray, Color.Gray, () => Debug.WriteLine("Level 3"), Textures.Button, text: "3"));
                    text.Add(new TextBox("Space Armada", new Vector2(70, 435), new Vector2(150, 50), FontSize.Small));

                    buttons.Add(new Button(new Vector2(ScreenWidth - 150, 400), 75, 75, Color.Gray, Color.Gray, () => Debug.WriteLine("Level 4"), Textures.Button, text: "4"));
                    text.Add(new TextBox("Meteor", new Vector2(ScreenWidth - 225, 435), new Vector2(150, 50), FontSize.Small));

                    buttons.Add(new Button(new Vector2(ScreenWidth / 2, 575), 75, 75, Color.Gray, Color.Gray, () => Debug.WriteLine("Level 5"), Textures.Button, text: "5"));
                    text.Add(new TextBox("Venom", new Vector2(ScreenWidth / 2 - 75, 610), new Vector2(150, 50), FontSize.Small));
                    break;

                case MenuPages.BackgroundStory:
                    text.Add(new TextBox("Background Story", new Vector2(25, 40), new Vector2(ScreenWidth - 50, 50), FontSize.Large));

                    text.Add(new TextBox("The evil lord Andross is threatening the planet Corneria with total destruction. As the elite pilot Fox McCloud, you have been hired to stop him. Take the fight to his lair on the planet " +
                        "Venom and destroy Andross before it's too late.",
                        new Vector2(25, 150), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("You will meet servants of Andross and mercenaries hired by him along the way.",
                        new Vector2(25, 300), new Vector2(ScreenWidth - 50, 100), FontSize.Medium, true));
                    text.Add(new TextBox("Good luck!",
                        new Vector2(25, 375), new Vector2(ScreenWidth - 50, 100), FontSize.Regular));

                    text.Add(new TextBox("\"I'll do my best. Andross won't have his way with me!\"",
                        new Vector2(ScreenWidth - 250, 500), new Vector2(200, 100), FontSize.Medium));

                    backButtonAction = () => ChangeMenu(MenuPages.LevelSelect);
                    break;
            }

            if (page != MenuPages.BackgroundStory)
                text.Add(new TextBox("Star Fox 2D v2.0", new Vector2(10, ScreenHeight - 25), FontSize.Small));
            buttons.Add(new Button(backButtonPosition, 150, 50, Color.Gray, Color.DarkGray, backButtonAction, Textures.Button, text: backButtonText));

            menuPage = page;
        }

        private void StartLevel(LevelID level)
        {
            text.Clear();
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
            else
            {
                foreach (Slider s in settingsSliders)
                {
                    if (s.MouseHoversButton(mouseState.Position.ToVector2()))
                    {
                        Slider.ActiveSlider = s;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Called while the left mouse button is being held down, but never at the same time as MouseLeftClicked or MouseLeftReleased.
        /// </summary>
        private void MouseLeftHold()
        {
            if (Slider.ActiveSlider != null)
            {
                Slider.ActiveSlider.ChangePosition(mouseState.Position.ToVector2());
            }
        }

        /// <summary>
        /// Called once when the left mouse button is released.
        /// </summary>
        private void MouseLeftReleased()
        {
            // check for button presses here
            if (Slider.ActiveSlider == null)
            {
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
            Slider.ActiveSlider = null;
        }

        private void SetMusicVolume(float volume)
        {
            Sounds.MusicVolume = volume;
            currentSong.ChangeVolume();
        }

        private void SetControlScheme(ControlScheme scheme)
        {
            controlScheme = scheme;
            if (scheme == ControlScheme.WASD)
            {
                WASDControlSchemeButtonColour = Color.Blue;
                ArrowKeysControlSchemeButtonColour = Color.Gray;
                ControlSchemeDescription.SetText("WASD to move, spacebar to roll");
            }
            else
            {
                WASDControlSchemeButtonColour = Color.Gray;
                ArrowKeysControlSchemeButtonColour = Color.Blue;
                ControlSchemeDescription.SetText("Arrow keys to move, RCTRL to roll");
            }
            WASDControlScheme.Colour = WASDControlSchemeButtonColour;
            ArrowKeysControlScheme.Colour = ArrowKeysControlSchemeButtonColour;
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

    enum ControlScheme
    {
        WASD,
        ArrowKeys
    }
}
