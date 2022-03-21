using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class Granga : RoundEnemy
    {
        private float healthBarPxPerHealth;
        private TextBox bossName;

        public Granga(int health, ObjectID id, int damage, int score, int radius, Texture2D texture)
            : base(health, id, damage, score, radius, texture, null)
        {
            TimeBetweenShots = 0;
            MisfireChance = 0;

            healthBarPxPerHealth = (float)MainGame.ScreenWidth / MaxHealth;
            bossName = new TextBox("GRANGA", new Vector2(0, 30), new Vector2(MainGame.ScreenWidth, 30));
        }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            // update velocity
            if (Position.X - Radius <= 0 || Position.X + Radius >= MainGame.ScreenWidth)
                Velocity = new Vector2(-Velocity.X, Velocity.Y);
            if (Position.Y - Radius <= 0 || Position.Y + Radius >= MainGame.ScreenHeight / 2)
                Velocity = new Vector2(Velocity.X, -Velocity.Y);

            base.Update(gameTime, levelTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // draw health bar
            //spriteBatch.DrawString(TextBox.FontRegular, "GRANGA", new Vector2(190, 30), Color.White);
            bossName.Draw(spriteBatch);

            // background of health bar, then current health portion
            spriteBatch.Draw(Textures.Button, new Rectangle(0, 0, MainGame.ScreenWidth, 15), Color.Gray);
            spriteBatch.Draw(Textures.Button, new Rectangle(0, 0, (int)(healthBarPxPerHealth * Health), 15), Color.Red);
        }

        public override void Shoot()
        {
            // shooting is random, 3% chance every frame, no misfire chance
            double rand = MainGame.Random.NextDouble();
            if (rand >= 0.97)
            {
                Bullet b = new Bullet(1, ObjectID.EnemyBullet, Damage, 0, 3, Textures.FilledCircle, BulletEffect)
                {
                    Position = new Vector2(Position.X, Position.Y + 15),
                    Velocity = MainGame.CalculateBulletVelocity(Position, MainGame.Player.Position, MainGame.baseBulletSpeed)
                };
                MainGame.Bullets.Add(b);
            }
        }
    }
}
