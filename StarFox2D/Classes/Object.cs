using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        public Color Colour { get; private set; }

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
        /// The effects applied by the bullets fired from the object.
        /// </summary>
        public Effects BulletEffects { get; private set; }

        /// <summary>
        /// The effects that affect the current object. Must call AppliedEffects.Update in Object.Update!
        /// </summary>
        public Effects AppliedEffects { get; private set; }

        public ObjectID ID { get; private set; }

        public Texture2D MainTexture { get; private set; }

        public List<Texture2D> AdditionalTextures { get; private set; }

        /// <summary>
        /// The origin position of the texture. Should be set to the centre of the texture.
        /// </summary>
        public Vector2 TextureOriginPosition { get; protected set; }

        public List<Vector2> AdditionalTextureOriginPositions { get; protected set; }

        /// <summary>
        /// The speed of the rotation of the texture.
        /// </summary>
        public float MainTextureRotationSpeed { get; set; }

        public List<float> AdditionalTexturesRotationSpeed { get; set; }

        protected float MainTextureRotation { get; set; }

        protected List<float> AdditionalTexturesRotation { get; set; }


        public Object(int health, ObjectID id, int damage, int score, Texture2D mainTexture, List<Texture2D> additionalTextures = null, Effects bulletEffects = null)
        {
            Health = health;
            MaxHealth = health;
            ID = id;
            Damage = damage;
            Score = score;
            IsAlive = true;
            MainTexture = mainTexture;
            MainTextureRotationSpeed = 0f;
            AdditionalTexturesRotationSpeed = new List<float>();

            TextureOriginPosition = new Vector2(MainTexture.Width / 2, MainTexture.Height / 2);

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

            if (bulletEffects is null)
                BulletEffects = new Effects();
            else
                BulletEffects = bulletEffects;
        }


        /// <summary>
        /// All movement updating is done in the MainGame.Update method - do not implement in object Update methods.
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(TimeSpan levelTime);

        /// <summary>
        /// Draws the texture. NOTE: the base Object.Draw implementation does not handled any colours or additional textures!
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(MainTexture, TextureOriginPosition, null, Color.White, MainTextureRotation, new Vector2(MainTexture.Width / 2, MainTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Given a bullet, checks if it is within the collision boundaries for the object and performs the appropriate logic is needed. (Reduce health, apply effect, die)
        /// </summary>
        public abstract void CheckBulletCollision(Bullet bullet);

        /// <summary>
        /// Returns true if the object is fully outside the screen and should be despawned.
        /// Only needs to be implemented in RoundObject and SquareObject.
        /// </summary>
        public abstract bool ObjectIsOutsideScreen();

        /// <summary>
        /// Given the other object, checks if the hitboxes are overlapping.
        /// The entire Object needs to be passed in for the Visitor design pattern.
        /// TODO is this necessary for the base class? Maybe only need to include in Player class.
        /// </summary>
        protected abstract bool OtherObjectIsWithinBoundaries(Object other);

        /// <summary>
        /// Adds the object's score to the score of the game level.
        /// Destruction of the objects is handled in MainGame.Update as one or both objects may be destroyed.
        /// </summary>
        protected virtual void Death()
        {
            MainGame.CurrentScore += Score;
            IsAlive = false;
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
        StarWolfWolf,
        StarWolfPigma,
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
