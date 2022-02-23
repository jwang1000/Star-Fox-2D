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

        public Player(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, Effects bulletEffects = null)
            : base(health, id, damage, score, radius, texture, bulletEffects) 
        {
            BaseVelocity = 300;
        }

        public override void Update(TimeSpan levelTime)
        {
            // TODO
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, TextureRotation, TextureOriginPosition, new Vector2((float)Radius * 2 / Texture.Width), SpriteEffects.None, 0f);

            // TODO draw shield and effects
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

        protected override void Death()
        {
            MainGame.CurrentLevel.PlayerDeath();
        }
    }
}
