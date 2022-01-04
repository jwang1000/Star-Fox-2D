using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    public class Effects
    {
        public int Count { get; private set; }

        /// <summary>
        /// Maps effects to the time when the effect should be removed.
        /// </summary>
        private Dictionary<EffectType, TimeSpan> effects;

        public Effects()
        {
            effects = new Dictionary<EffectType, TimeSpan>();
        }

        public void AddEffect(EffectType type, TimeSpan currentTime, TimeSpan duration)
        {
            if (effects.ContainsKey(type))
            {
                effects[type] = currentTime + duration;
            }
            else
            {
                effects.Add(type, currentTime + duration);
            }
        }

        public void Update(TimeSpan levelTime)
        {
            foreach (var pair in effects)
            {
                if (pair.Value < levelTime)
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
