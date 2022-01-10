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

        public SquareObject(int health, ObjectID id, int damage, int score, int sideLength, Texture2D mainTexture, List<Texture2D> additionalTextures = null, Effects bulletEffects = null)
            : base(health, id, damage, score, mainTexture, additionalTextures, bulletEffects) 
        {
            SideLength = sideLength;
        }

        public override void Update(TimeSpan levelTime)
        {
            // Regular square objects (non-enemies) shouldn't do anything
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(MainTexture, Position, null, Color.White, MainTextureRotation, TextureOriginPosition, new Vector2((float)SideLength / MainTexture.Width), SpriteEffects.None, 0f);

            // TODO draw effects
        }

        public override void CheckBulletCollision(Bullet bullet)
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
