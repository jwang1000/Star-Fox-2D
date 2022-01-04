using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class RoundObject : Object
    {
        public int Radius { get; protected set; }

        public RoundObject(int health, ID id, int damage, int score, int radius, Texture2D mainTexture, List<Texture2D> additionalTextures = null, Effects bulletEffects = null) 
            : base(health, id, damage, score, mainTexture, additionalTextures, bulletEffects) 
        {
            Radius = radius;
        }

        public override void Update(TimeSpan levelTime)
        {
            throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
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
