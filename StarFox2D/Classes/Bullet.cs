using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class Bullet : RoundObject
    {
        public Bullet(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, EffectType? bulletEffect = null) 
            : base(health, id, damage, score, radius, texture, bulletEffect) { }

        public override void Update(GameTime gameTime, TimeSpan levelTime)
        {
            // update velocity of the bullet if it has the targeting effect
            // only update if bullet is in front of the player and far from the player
            if (BulletEffect != null && BulletEffect == EffectType.Target && CalculateRoundObjectDistance(Position, MainGame.Player.Position) >= 200 && Position.Y < MainGame.Player.Position.Y)
            {
                Velocity = MainGame.CalculateBulletVelocity(Position, MainGame.Player.Position, MainGame.baseBulletSpeed);
            }
            return;
        }

        public override bool CheckBulletCollision(Bullet bullet)
        {
            // nothing happens for bullets colliding with bullets
            return false;
        }
    }
}
