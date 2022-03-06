using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class Player : RoundObject
    {
        public float BaseVelocity { get; private set; }

        private bool overlappingOtherObj;
        private TimeSpan lastOverlapDamageTime;
        private TimeSpan minTimeBetweenOverlapDamage;
        private Object otherObjBeingOverlapped;
        private Shield Shield;

        public Player(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, Effects bulletEffects = null)
            : base(health, id, damage, score, radius, texture, bulletEffects) 
        {
            BaseVelocity = 300;
            Shield = new Shield(Position, Radius);
            overlappingOtherObj = false;
            lastOverlapDamageTime = new TimeSpan();
            minTimeBetweenOverlapDamage = new TimeSpan(0, 0, 0, 0, 10);
        }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            if (overlappingOtherObj)
            {
                if (levelTime - lastOverlapDamageTime > minTimeBetweenOverlapDamage)
                {
                    TakeDamage(1);
                    lastOverlapDamageTime = levelTime;
                    otherObjBeingOverlapped.TakeDamage(1);
                }
                overlappingOtherObj = false;
                otherObjBeingOverlapped = null;
            }

            Shield.Update(gameTime, Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Colour, TextureRotation, TextureOriginPosition, new Vector2((float)Radius * 2 / Texture.Width), SpriteEffects.None, 0f);

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
            overlappingOtherObj = false;
            if (!other.IsAlive)
            {
                otherObjBeingOverlapped = null;
                return;
            }

            if (other is RoundObject roundObj)
            {
                overlappingOtherObj = CalculateRoundObjectDistance(Position, roundObj.Position) <= Radius + roundObj.Radius;

                if (overlappingOtherObj && roundObj is Ring ring)
                {
                    overlappingOtherObj = false;

                    // add health accordingly, destroy ring
                    MaxHealth += ring.ShieldIncrease;
                    Health = Math.Min(MaxHealth, Health + ring.HealthRestored);

                    ring.TakeDamage(ring.MaxHealth);
                    Shield.ClearDamageTime();
                }
                else
                    otherObjBeingOverlapped = other;
            }
            else if (other is SquareObject squareObj)
            {
                // find coordinate that is orthogonal to the direction the side points in (left wall is vertical, so find horizontal (x) coord)
                float leftSideX = squareObj.Position.X - squareObj.SideLength / 2;
                float bottomSideY = squareObj.Position.Y + squareObj.SideLength / 2;
                float rightSideX = squareObj.Position.X + squareObj.SideLength / 2;
                float topSideY = squareObj.Position.Y - squareObj.SideLength / 2;

                overlappingOtherObj = Position.X + Radius >= leftSideX && Position.X - Radius <= rightSideX
                    && Position.Y - Radius <= bottomSideY && Position.Y + Radius >= topSideY;
                otherObjBeingOverlapped = other;
            }
        }

        public override void TakeDamage(int damage, Effects effects = null)
        {
            base.TakeDamage(damage, effects);
            Shield.SetDamageTime();
        }

        protected override void Death()
        {
            IsAlive = false;
            MainGame.CurrentLevel.PlayerDeath();
        }
    }
}
