using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StarFox2D.Classes
{
    /// <summary>
    /// Base class for all objects (player, enemies, bosses, buildings, bullets).
    /// </summary>
    public abstract class Object
    {
        /// <summary>
        /// The position of the object marks its centre, regardless if it is square or round.
        /// The size of the object is determined by its radius or side length.
        /// </summary>
        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; }

        public int Health { get; protected set; }

        public int MaxHealth { get; protected set; }

        /// <summary>
        /// The colour of the bullets fired or the colour of the building (rings).
        /// </summary>
        public Color Colour { get; protected set; }

        /// <summary>
        /// The damage done by the bullets fired from the object.
        /// </summary>
        public int Damage { get; private set; }

        /// <summary>
        /// The score received from destroying the object.
        /// </summary>
        public int Score { get; private set; }

        public bool IsAlive { get; protected set; }

        /// <summary>
        /// The effect applied by the bullets fired from the object.
        /// </summary>
        public EffectType? BulletEffect { get; private set; }

        /// <summary>
        /// The effects that affect the current object. Must call AppliedEffects.Update in Object.Update!
        /// </summary>
        public Effects AppliedEffects { get; private set; }

        public ObjectID ID { get; private set; }

        public Texture2D Texture { get; private set; }

        /// <summary>
        /// The origin position of the texture. Should be set to the centre of the texture.
        /// </summary>
        public Vector2 TextureOriginPosition { get; protected set; }

        /// <summary>
        /// The speed of the rotation of the texture.
        /// </summary>
        public float TextureRotationSpeed { get; set; }

        protected float TextureRotation { get; set; }


        public Object(int health, ObjectID id, int damage, int score, Texture2D texture, EffectType? bulletEffect = null)
        {
            Health = health;
            MaxHealth = health;
            ID = id;
            Damage = damage;
            Score = score;
            IsAlive = true;
            Texture = texture;
            TextureRotationSpeed = 0f;
            Colour = Color.White;
            BulletEffect = bulletEffect;
            AppliedEffects = new Effects();

            TextureOriginPosition = new Vector2(Texture.Width / 2, Texture.Height / 2);

            /*
            AdditionalTextureOriginPositions = new List<Vector2>();
            if (additionalTextures is null)
            {
                AdditionalTextures = new List<Texture2D>();
            }
            else
            {
                AdditionalTextures = additionalTextures;
                foreach (Texture2D t in AdditionalTextures)
                {
                    AdditionalTextureOriginPositions.Add(new Vector2(t.Width / 2, t.Height / 2));
                }
            }
            */
        }


        /// <summary>
        /// All movement updating is done in the MainGame.Update method - do not implement in object Update methods.
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime, TimeSpan levelTime);

        /// <summary>
        /// Draws the texture. NOTE: the base Object.Draw implementation does not handled any colours or additional textures!
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, TextureOriginPosition, null, Colour, TextureRotation, new Vector2(Texture.Width / 2, Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Given a bullet, checks if it is within the collision boundaries for the object and performs the appropriate logic is needed. (Reduce health, apply effect, die)
        /// Returns true if the bullet collided with something and should be removed. Sets the bullet's IsAlive to false if so.
        /// </summary>
        public abstract bool CheckBulletCollision(Bullet bullet);

        /// <summary>
        /// Returns true if the object is fully outside the screen and should be despawned.
        /// Only needs to be implemented in RoundObject and SquareObject.
        /// </summary>
        public abstract bool ObjectIsOutsideScreen();

        /// <summary>
        /// Inflicts damage on the object and applies effects if appropriate. Calls Death() if necessary.
        /// WARNING: Must be overridden to display the shield! Slowness will be applied here, but will not have an effect on non-enemies/players/bosses.
        /// </summary>
        public virtual void TakeDamage(int damage, EffectType? effect = null)
        {
            Health -= damage;
            Debug.WriteLine("health is now " + Health + " after taking " + damage + " damage");
            if (Health <= 0)
            {
                Death();
            }
            else if (effect != null && effect != EffectType.Target)
            {
                // TODO apply statuses once implemented
                AppliedEffects.AddEffect((EffectType) effect);
            }
        }

        /// <summary>
        /// Adds the object's score to the score of the game level.
        /// Destruction of the objects is handled in MainGame.Update as one or both objects may be destroyed.
        /// </summary>
        protected virtual void Death()
        {
            MainGame.CurrentScore += Score;
            IsAlive = false;
        }

        /// <summary>
        /// Given the centres of two <b>round</b> objects, calculates the distance between them.
        /// </summary>
        public static float CalculateRoundObjectDistance(Vector2 pos1, Vector2 pos2)
        {
            return (float)Math.Sqrt(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2));
        }
    }


    /// <summary>
    /// IDs for all objects in the game.
    /// Bullets take on the ID of the object that fired it.
    /// </summary>
    public enum ObjectID
    {
        Player,
        Granga, 
        MechaTurret,
        GrangaRematch,
        StarWolfTeam,
        StarWolfWolf,
        StarWolfPigma,
        Andross,
        AndrossHead,
        AndrossLH,
        AndrossRH,
        Fly,
        Mosquito,
        Hornet,
        QueenFly,
        MiniAndross,
        Asteroid,
        SmallAsteroid,
        Debris,
        Satellite,
        Turret,
        RingWhite,
        RingYellow,
        RingGreen,
        RingRed,
        PlayerBullet,
        EnemyBullet
    }
}
