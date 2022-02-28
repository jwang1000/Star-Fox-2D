using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    /// <summary>
    /// Class for handling drawing the flashing shield when damage is taken or effects are applied.
    /// </summary>
    public class Shield
    {
        public int Radius { get; private set; }

        /// <summary>
        /// The centre of the object the shield applies to.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Optionally exposed for testing purposes. In normal usage, call SetDamageTime().
        /// </summary>
        public bool ShowDamage { get; set; }

        // target effect should not have a colour
        public Dictionary<EffectType, bool> ShowEffects { get; private set; }

        /// <summary>
        /// The fade in/out of the shield is based on the current frame (out of 60 per second)
        /// </summary>
        private double frameExact;

        private int frame;
        private double damageTime;

        private ColourOpacityPair DamageColour;
        private Dictionary<EffectType, ColourOpacityPair> EffectColours;

        public Shield(Vector2 pos, int radius)
        {
            Position = pos;
            Radius = radius;
            frameExact = 0;
            damageTime = 0;
            DamageColour = new ColourOpacityPair(Color.Red, 1);
            ShowEffects = new Dictionary<EffectType, bool>()
            {
                { EffectType.Depletion, false },
                { EffectType.Slow, false }
            };
            EffectColours = new Dictionary<EffectType, ColourOpacityPair>()
            {
                { EffectType.Depletion, new ColourOpacityPair(Color.Magenta, 1) },
                { EffectType.Slow, new ColourOpacityPair(Color.Yellow, 1) }
            };
        }

        public void Update(GameTime gameTime, Vector2 pos)
        {
            Position = pos;

            // typically 60fps, but setting this to 100 in order to have the shield flash faster (while the math stays nice)
            frameExact += gameTime.ElapsedGameTime.TotalSeconds * 100;
            if (frameExact > 60)
                frameExact -= 60;
            frame = (int)frameExact;

            if (damageTime > 0)
            {
                damageTime -= gameTime.ElapsedGameTime.TotalSeconds;
                ShowDamage = damageTime > 0;
            }

            // set opacity of colours according to the frame
            if (frame < 30)
                DamageColour.Opacity = (float)frame / 30;
            else
                DamageColour.Opacity = 1 - (float)(frame - 30) / 30;

            // slow and depletion should alternate, offset from damage
            float depletion = (frame + 15) % 60;
            if (depletion < 30)
                depletion /= 30;
            else
                depletion = 1 - (depletion - 30) / 30;

            EffectColours[EffectType.Depletion].Opacity = depletion;
            EffectColours[EffectType.Slow].Opacity = 1 - depletion;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (ShowDamage)
            {
                spriteBatch.Draw(Textures.Ring, Position, null, DamageColour.Colour * DamageColour.Opacity, 0,
                    new Vector2(Textures.Ring.Width / 2, Textures.Ring.Height / 2),
                    new Vector2((float)Radius * 2 / Textures.Ring.Width), SpriteEffects.None, 0f);
            }
            foreach (EffectType type in ShowEffects.Keys)
            {
                if (ShowEffects[type])
                {
                    spriteBatch.Draw(Textures.Ring, Position, null, EffectColours[type].Colour * EffectColours[type].Opacity, 0,
                        new Vector2(Textures.Ring.Width / 2, Textures.Ring.Height / 2),
                        new Vector2(Radius * 2.2f / Textures.Ring.Width), SpriteEffects.None, 0f);
                }
            }
        }

        public void SetDamageTime(double seconds = 3)
        {
            damageTime = Math.Max(damageTime, seconds);
        }
    }
}

/// <summary>
/// Yes, the Color object already has opacity built in, but for some reason updating the A field doesn't do anything.
/// Multiplying the Color object by the opacity works.
/// </summary>
public class ColourOpacityPair
{
    public Color Colour;

    /// <summary>
    /// Opacity must be between 0 and 1.
    /// </summary>
    public float Opacity;

    public ColourOpacityPair(Color colour, float opacity)
    {
        Colour = colour;
        Opacity = opacity;
    }
}
