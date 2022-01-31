using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class Ring : RoundObject
    {
        /// <summary>
        /// The amount of health restored by running into this ring.
        /// </summary>
        public int HealthRestored { get; private set; }

        /// <summary>
        /// The amount the maximum shield increases by.
        /// </summary>
        public int ShieldIncrease { get; private set; }

        public Ring(int health, ObjectID id, int damage, int score, int radius, int healthRestored, int shieldIncrease, Texture2D texture, Effects bulletEffects = null)
            : base(health, id, damage, score, radius, texture, bulletEffects)
        {
            HealthRestored = healthRestored;
            ShieldIncrease = shieldIncrease;
        }

        public override void Update(TimeSpan levelTime)
        {
            // rings should not need to update
            return;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

        public override bool CheckBulletCollision(Bullet bullet)
        {
            // nothing should happen for rings
            return false;
        }

        protected override bool OtherObjectIsWithinBoundaries(Object other)
        {
            throw new NotImplementedException();
        }
    }
}
