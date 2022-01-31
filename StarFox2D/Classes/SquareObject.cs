using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class SquareObject : Object
    {
        public int SideLength { get; protected set; }

        public SquareObject(int health, ObjectID id, int damage, int score, int sideLength, Texture2D texture, Effects bulletEffects = null)
            : base(health, id, damage, score, texture, bulletEffects) 
        {
            SideLength = sideLength;
        }

        public override void Update(TimeSpan levelTime)
        {
            // Regular square objects (non-enemies) shouldn't do anything
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, TextureRotation, TextureOriginPosition, new Vector2((float)SideLength / Texture.Width), SpriteEffects.None, 0f);

            // TODO draw effects
        }

        public override bool CheckBulletCollision(Bullet bullet)
        {
            throw new NotImplementedException();
        }

        public override bool ObjectIsOutsideScreen()
        {
            return Position.Y + (SideLength / 2) >= MainGame.ScreenHeight + MainGame.DespawnBuffer ||
                Position.X - (SideLength / 2) <= -MainGame.DespawnBuffer ||
                Position.X + (SideLength / 2) >= MainGame.ScreenWidth + MainGame.DespawnBuffer;
        }

        protected override bool OtherObjectIsWithinBoundaries(Object other)
        {
            throw new NotImplementedException();
        }
    }
}
