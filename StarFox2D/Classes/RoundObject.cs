using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class RoundObject : Object
    {
        public int Radius { get; protected set; }

        public RoundObject(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, EffectType? bulletEffect = null) 
            : base(health, id, damage, score, texture, bulletEffect) 
        {
            Radius = radius;
        }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            // Regular round objects (non-enemies) shouldn't do anything, effects shouldn't do anything either
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Colour, TextureRotation, TextureOriginPosition, new Vector2((float)Radius * 2 / Texture.Width), SpriteEffects.None, 0f);

            if (this is RoundEnemy)
            {
                // TODO draw effects
            }
        }

        public override bool CheckBulletCollision(Bullet bullet)
        {
            if (bullet.Position.X + bullet.Radius >= Position.X - Radius &&
                bullet.Position.X - bullet.Radius <= Position.X + Radius &&
                bullet.Position.Y + bullet.Radius >= Position.Y - Radius &&
                bullet.Position.Y - bullet.Radius <= Position.Y + Radius)
            {
                bullet.IsAlive = false;
                TakeDamage(bullet.Damage, bullet.BulletEffect);
                return true;
            }
            return false;
        }

        public override bool ObjectIsOutsideScreen()
        {
            return Position.Y + Radius >= MainGame.ScreenHeight + MainGame.DespawnBuffer ||
                Position.X - Radius <= -MainGame.DespawnBuffer ||
                Position.X + Radius >= MainGame.ScreenWidth + MainGame.DespawnBuffer;
        }
    }
}
