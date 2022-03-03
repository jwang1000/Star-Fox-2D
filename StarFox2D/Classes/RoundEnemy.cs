using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class RoundEnemy : RoundObject
    {
        private Shield Shield;

        public RoundEnemy(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, Effects bulletEffects = null)
            : base(health, id, damage, score, radius, texture, bulletEffects)
        {
            Radius = radius;

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

        public override void TakeDamage(int damage, Effects effects = null)
        {
            base.TakeDamage(damage, effects);
            Shield.SetDamageTime();
        }
    }
}
