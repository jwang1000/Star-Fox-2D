using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class Player : RoundObject
    {
        public float BaseVelocity { get; private set; }

        private Shield Shield;

        public Player(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, Effects bulletEffects = null)
            : base(health, id, damage, score, radius, texture, bulletEffects) 
        {
            BaseVelocity = 300;
            Shield = new Shield(Position, Radius);
        }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            // TODO
            Shield.Update(gameTime, Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, TextureRotation, TextureOriginPosition, new Vector2((float)Radius * 2 / Texture.Width), SpriteEffects.None, 0f);

            // draw shield and effects
            Shield.Draw(spriteBatch);
        }

        public override bool ObjectIsOutsideScreen()
        {
            // Player will never be outside the screen
            return false;
        }

        /// <summary>
        /// Given the other object, checks if the hitboxes are overlapping and applies damage if needed. Should be called in MainGame.Update.
        /// </summary>
        public void CheckOtherObjectIsWithinBoundaries(Object other)
        {
            bool inBoundaries = false;
            if (other is RoundObject)
            {

            }
            else if (other is SquareObject)
            {

            }

            if (inBoundaries)
            {
                // TODO apply damage per update
            }
        }

        public override void TakeDamage(int damage, Effects effects)
        {
            base.TakeDamage(damage, effects);
            Shield.SetDamageTime();
        }

        protected override void Death()
        {
            MainGame.CurrentLevel.PlayerDeath();
        }
    }
}
