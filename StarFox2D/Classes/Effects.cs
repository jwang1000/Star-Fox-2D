using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class Effects
    {
        public int Count { get; private set; }

        public static TimeSpan StandardDuration = new TimeSpan(0, 0, 3);

        /// <summary>
        /// Maps effects to the remaining time of the effect.
        /// </summary>
        private Dictionary<EffectType, TimeSpan> effects;

        public Effects()
        {
            effects = new Dictionary<EffectType, TimeSpan>();
        }

        public void AddEffect(EffectType type, TimeSpan? duration = null)
        {
            if (duration == null)
                duration = StandardDuration;

            if (effects.ContainsKey(type))
            {
                effects[type] = (TimeSpan) duration;
            }
            else
            {
                effects.Add(type, (TimeSpan) duration);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var pair in effects)
            {
                effects[pair.Key] -= gameTime.ElapsedGameTime;
                if (pair.Value.TotalSeconds <= 0)
                {
                    effects.Remove(pair.Key);
                }
            }
        }

        public bool HasEffectApplied(EffectType type)
        {
            return effects.ContainsKey(type);
        }
    }

    /// <summary>
    /// All types of effects that can be applied.
    /// </summary>
    public enum EffectType
    {
        Slow,
        Depletion,
        Target
    }
}
