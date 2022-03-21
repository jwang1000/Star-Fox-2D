using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class RoundEnemy : RoundObject
    {
        /// <summary>
        /// The time in seconds between shots.
        /// </summary>
        public double TimeBetweenShots { get; protected set; }

        /// <summary>
        /// The probability that a shot is not aimed directly at the player. Should be between 0 and 1.
        /// </summary>
        public double MisfireChance { get; protected set; }

        protected TimeSpan AliveTime;
        protected TimeSpan LastFiredAliveTime;

        protected Shield Shield;

        public RoundEnemy(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, EffectType? bulletEffect = null)
            : base(health, id, damage, score, radius, texture, bulletEffect)
        {
            Radius = radius;

            Shield = new Shield(Position, Radius);
            AliveTime = TimeSpan.Zero;
            LastFiredAliveTime = TimeSpan.Zero;

            switch (ID)
            {
                case ObjectID.Fly:
                    TimeBetweenShots = 0.6;
                    MisfireChance = 0.1;
                    break;

                case ObjectID.Mosquito:
                    TimeBetweenShots = 0.55;
                    MisfireChance = 0.3;
                    break;

                case ObjectID.Hornet:
                    TimeBetweenShots = 0.5;
                    MisfireChance = 0.5;
                    break;

                case ObjectID.QueenFly:
                    TimeBetweenShots = 0.45;
                    MisfireChance = 0.5;
                    break;

                case ObjectID.MiniAndross:
                    TimeBetweenShots = 0.5;
                    MisfireChance = 0;
                    break;

                default:
                    Debug.WriteLine("Error: creating round enemy with ID " + ID + ", should only occur when creating a boss");
                    TimeBetweenShots = 1;
                    MisfireChance = 0;
                    break;
            }
        }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            AliveTime += gameTime.ElapsedGameTime;
            Shoot();
            AppliedEffects.Update(gameTime);
            Shield.Update(gameTime, Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Colour, TextureRotation, TextureOriginPosition, new Vector2((float)Radius * 2 / Texture.Width), SpriteEffects.None, 0f);

            // draw shield and effects
            Shield.Draw(spriteBatch);
        }

        public override void TakeDamage(int damage, EffectType? effect = null)
        {
            base.TakeDamage(damage, effect);
            Shield.SetDamageTime();
        }

        public virtual void Shoot()
        {
            // only shoot if the enemy is in front of the player and enough time has passed
            if (Position.Y <= MainGame.Player.Position.Y && (AliveTime - LastFiredAliveTime).TotalSeconds > TimeBetweenShots)
            {
                double rand = MainGame.Random.NextDouble();
                if (ID == ObjectID.QueenFly && rand < 0.3)
                {
                    // spawn fly instead of firing bullet
                    Object fly = MainGame.CurrentLevel.CreateObject(ObjectID.Fly);
                    fly.Position = Position;
                    if (rand < 0.15)
                        fly.Position = new Vector2(Position.X - Radius, Position.Y);
                    else
                        fly.Position = new Vector2(Position.X + Radius, Position.Y);

                    fly.Velocity = Velocity;
                    MainGame.Objects.Add(fly);
                }
                else
                {
                    Bullet b = new Bullet(1, ObjectID.EnemyBullet, Damage, 0, 3, Textures.FilledCircle, BulletEffect)
                    {
                        Position = new Vector2(Position.X, Position.Y + Radius)
                    };
                    Vector2 dest = MainGame.Player.Position;
                    if (rand < MisfireChance)
                    {
                        if (rand < MisfireChance / 2)
                            dest.X -= MainGame.Player.Radius / 2;
                        else
                            dest.X += MainGame.Player.Radius / 2;
                    }
                    b.Velocity = MainGame.CalculateBulletVelocity(b.Position, dest, MainGame.baseBulletSpeed);

                    MainGame.Bullets.Add(b);
                }

                LastFiredAliveTime = AliveTime;
            }
        }
    }
}
