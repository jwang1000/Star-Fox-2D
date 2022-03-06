using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class Ring : RoundObject
    {
        /// <summary>
        /// The amount of health restored by running into this ring.
        /// </summary>
        public int HealthRestored { get; private set; }

        /// <summary>
        /// The amount the maximum shield increases by.
        /// </summary>
        public int ShieldIncrease { get; private set; }

        public Ring(int health, ObjectID id, int damage, int score, int radius, int healthRestored, int shieldIncrease, Texture2D texture, Color colour)
            : base(health, id, damage, score, radius, texture, null)
        {
            HealthRestored = healthRestored;
            ShieldIncrease = shieldIncrease;
            Colour = colour;
        }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            // rings should not need to update
            return;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Colour, TextureRotation, TextureOriginPosition, new Vector2((float)Radius * 2 / Texture.Width), SpriteEffects.None, 0f);
        }

        public override bool CheckBulletCollision(Bullet bullet)
        {
            // nothing should happen for rings
            return false;
        }

        protected override void Death()
        {
            base.Death();
            // TODO play sound for using ring
        }
    }
}
