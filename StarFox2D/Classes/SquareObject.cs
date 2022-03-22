using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class SquareObject : Object
    {
        public int SideLength { get; protected set; }

        public SquareObject(int health, ObjectID id, int damage, int score, int sideLength, Texture2D texture, EffectType? bulletEffect = null)
            : base(health, id, damage, score, texture, bulletEffect) 
        {
            SideLength = sideLength;
            TextureRotationSpeed = MainGame.Random.Next(-7, 7);
        }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            // Regular square objects (non-enemies) shouldn't do anything, effects shouldn't do anything either

            if (ID == ObjectID.Satellite)
            {
                // Satellites rotate at random speeds
                TextureRotation += TextureRotationSpeed * (float) gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Colour, TextureRotation, TextureOriginPosition, new Vector2((float)SideLength / Texture.Width), SpriteEffects.None, 0f);
            // effects are only displayed if the object has a shield (i.e. enemies)
        }

        public override bool CheckBulletCollision(Bullet bullet)
        {
            if (bullet.Position.X + bullet.Radius >= Position.X - SideLength / 2 &&
                bullet.Position.X - bullet.Radius <= Position.X + SideLength / 2 &&
                bullet.Position.Y + bullet.Radius >= Position.Y - SideLength / 2 &&
                bullet.Position.Y - bullet.Radius <= Position.Y + SideLength / 2)
            {
                bullet.IsAlive = false;
                TakeDamage(bullet.Damage, bullet.BulletEffect);
                return true;
            }
            return false;
        }

        public override bool ObjectIsOutsideScreen()
        {
            return Position.Y + (SideLength / 2) >= MainGame.ScreenHeight + MainGame.DespawnBuffer ||
                Position.X - (SideLength / 2) <= -MainGame.DespawnBuffer ||
                Position.X + (SideLength / 2) >= MainGame.ScreenWidth + MainGame.DespawnBuffer;
        }
    }
}
