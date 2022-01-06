using Microsoft.Xna.Framework;
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
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


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



        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            playingLevel = false;
            page = MenuPages.TitleScreen;
            playerVelocity = Vector2.Zero;
        }

        protected override void Initialize()
        {
            // set window size
            _graphics.PreferredBackBufferWidth = 500;
            _graphics.PreferredBackBufferHeight = 800;
            _graphics.ApplyChanges();

            base.Initialize();

            // initialize classes that require content (i.e. sprites)
            backgroundImagePosition = new Vector2(Textures.Background.Width / 2, 0);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // load sprites
            FontRegular = Content.Load<SpriteFont>("FontRegular");
            FontLarge = Content.Load<SpriteFont>("FontLarge");
            FontTitle = Content.Load<SpriteFont>("FontTitle");
            LoadTextures();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if (playingLevel)
            {
                CurrentTime += gameTime.ElapsedGameTime;

                // update level (spawns objects)
                CurrentLevel.Update(CurrentTime);


                // move all objects (players, enemies, etc)
                // TODO movement multipliers from effects
                // TODO constrain movement for player so it can't go off screen
                kstate = Keyboard.GetState();

                playerVelocity = Vector2.Zero;
                if (kstate.IsKeyDown(Keys.W) && CurrentLevel.InBossFight)
                    playerVelocity.Y -= Player.BaseVelocity;
                if (kstate.IsKeyDown(Keys.S) && CurrentLevel.InBossFight)
                    playerVelocity.Y += Player.BaseVelocity;
                if (kstate.IsKeyDown(Keys.A))
                    playerVelocity.X -= Player.BaseVelocity;
                if (kstate.IsKeyDown(Keys.D))
                    playerVelocity.X += Player.BaseVelocity;


                elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Player.Position += playerVelocity * elapsedSeconds;
                foreach (var enemy in Enemies)
                    enemy.Position += enemy.Velocity * elapsedSeconds;
                foreach (var building in Buildings)
                    building.Position += building.Velocity * elapsedSeconds;
                foreach (var bullet in Bullets)
                    bullet.Position += bullet.Velocity * elapsedSeconds;


                // then call update methods for all objects for additional logic
                Player.Update(CurrentTime);
                foreach (var enemy in Enemies)
                    enemy.Update(CurrentTime);
                foreach (var building in Buildings)
                    building.Update(CurrentTime);
                foreach (var bullet in Bullets)
                    bullet.Update(CurrentTime);


                // delete objects once a second TODO
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
            _spriteBatch.Begin();

            // Draw background
            _spriteBatch.Draw(Textures.Background, backgroundImagePosition, null, Color.White, 0f, new Vector2(Textures.Background.Width/2, Textures.Background.Height/2), Vector2.One, SpriteEffects.None, 0f);

            if (playingLevel)
            {
                Player.Draw(_spriteBatch);
                foreach (var enemy in Enemies)
                    enemy.Draw(_spriteBatch);
                foreach (var building in Buildings)
                    building.Draw(_spriteBatch);
                foreach (var bullet in Bullets)
                    bullet.Draw(_spriteBatch);

                // update background
                backgroundImagePosition.Y = (backgroundImagePosition.Y + 5) % (Textures.Background.Height / 2);
            }
            else
            {
                _spriteBatch.DrawString(FontRegular, "v2.0", new Vector2(10, _graphics.PreferredBackBufferHeight - 25), Color.White);
                switch(page)
                {
                    case MenuPages.TitleScreen:
                        _spriteBatch.DrawString(FontTitle, "STAR FOX 2D", new Vector2(50, 50), Color.White);
                        break;
                }
            }

            _spriteBatch.End();

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
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error in loading textures:");
                Debug.WriteLine(e);
                Exit();
            }

            Textures.TexturesAreLoaded = true;
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
