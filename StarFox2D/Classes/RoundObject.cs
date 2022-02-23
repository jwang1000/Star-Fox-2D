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

        public RoundObject(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, Effects bulletEffects = null) 
            : base(health, id, damage, score, texture, bulletEffects) 
        {
            Radius = radius;
        }

        public override void Update(TimeSpan levelTime)
        {
            // Regular round objects (non-enemies) shouldn't do anything
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, TextureRotation, TextureOriginPosition, new Vector2((float)Radius * 2 / Texture.Width), SpriteEffects.None, 0f);

            // TODO draw effects
        }

        public override bool CheckBulletCollision(Bullet bullet)
        {
            if (bullet.Position.X + bullet.Radius >= Position.X - Radius &&
                bullet.Position.X - bullet.Radius <= Position.X + Radius &&
                bullet.Position.Y + bullet.Radius >= Position.Y - Radius &&
                bullet.Position.Y - bullet.Radius <= Position.Y + Radius)
            {
                bullet.IsAlive = false;
                Health -= bullet.Damage;
                Debug.WriteLine("health is now " + Health + " after taking " + bullet.Damage + " damage");
                if (Health <= 0)
                {
                    Death();
                }
                else if (bullet.BulletEffects.Count > 0)
                {
                    // TODO apply statuses once implemented
                }
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
