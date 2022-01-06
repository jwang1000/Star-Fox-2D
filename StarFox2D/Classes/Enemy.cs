using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class Enemy : RoundObject
    {
        public Enemy(int health, ObjectID id, int damage, int score, int radius, Texture2D mainTexture, List<Texture2D> additionalTextures = null, Effects bulletEffects = null)
            : base(health, id, damage, score, radius, mainTexture, additionalTextures, bulletEffects)
        {
            Radius = radius;
        }

        public override void Update(TimeSpan levelTime)
        {
            // TODO
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(MainTexture, Position, null, Color.White, MainTextureRotation, TexturePosition, new Vector2((float)Radius * 2 / MainTexture.Width), SpriteEffects.None, 0f);

            // TODO draw shield and effects
        }

        public override void CheckBulletCollision(Bullet bullet)
        {
            throw new NotImplementedException();
        }

        protected override bool IsWithinBoundaries(Object other)
        {
            throw new NotImplementedException();
        }

        protected override void Death()
        {
            throw new NotImplementedException();
        }
    }
}
