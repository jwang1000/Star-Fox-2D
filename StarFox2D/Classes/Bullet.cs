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
        public Bullet(int health, ObjectID id, int damage, int score, int radius, Texture2D texture, Effects bulletEffects = null) 
            : base(health, id, damage, score, radius, texture, bulletEffects) { }

        public override void Update(TimeSpan levelTime)
        {
            // update velocity of the bullet if it has the targeting effect
            if (BulletEffects.Count > 0 && BulletEffects.HasEffectApplied(EffectType.Target))
            {
                // TODO
                /*
                    if round_dist(self.pos, player.pos) <= 150 and level_time % 15 == 0:
                        # start pos, dest_pos, vel (default 9 for granga rematch)
                        vel = bullet_target(self.pos, player.pos, self.start_vel)
                        self.vel[0]  = self.vel[0] * 0.75 + vel[0] * 0.25
                        self.vel[1]  = self.vel[1] * 0.75 + vel[1] * 0.25
                 */
            }
            return;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

        public override void CheckBulletCollision(Bullet bullet)
        {
            // nothing happens for bullets colliding with bullets
            return;
        }

        protected override bool OtherObjectIsWithinBoundaries(Object other)
        {
            // method should never be called for a bullet, only for buildings, players, enemies, bosses
            Debug.WriteLine("ERROR: Bullet class IsWithinBoundaries was called with argument " + other.ID);
            return false;
        }
    }
}
