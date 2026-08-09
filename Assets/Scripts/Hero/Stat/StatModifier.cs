using System.Collections.Generic;

namespace MagicSchool
{
    public interface Modifier
    {
        public float GetAmount();
        public ModifierEnum GetModifier();
        public float GetDuration();
    }

    /// <summary>
    /// StatModifier are a helper for Stat:
    /// 1) It contain all logic for calculating those modifier into final stat
    /// 2) Update those modifier duration
    /// 3) Remember all modifier for this hero
    /// </summary>
    public class StatModifier
    {
        private const float Permanent = -1f;

        private readonly List<ActiveModifier> _modifiers = new List<ActiveModifier>();

        // Only modifiers that add a flat amount to a base stat belong here. Healing does NOT:
        // it moves current HP, which is not a stat, so it stays HealSkillEffect's job.
        // What this entry is really for is the max-HP buff - i.e. BonusHP.
        private static readonly Dictionary<ModifierEnum, StatType> FlatBonusTarget = new Dictionary<ModifierEnum, StatType>
        {
            { ModifierEnum.BonusHP, StatType.HP },
        };

        // =================================== life cycle ===================================
        // update all current modifier duration, removing any that just expired
        public void Tick(float deltaTime)
        {
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                // if permanent, skip timer
                if (float.IsPositiveInfinity(_modifiers[i].Remaining)) continue;

                // update timer
                _modifiers[i].Remaining -= deltaTime;

                // if timer is expired, remove modifier
                if (_modifiers[i].Remaining <= 0f) _modifiers.RemoveAt(i);
            }
        }

        // =================================== setter ===================================
        // add new modifier
        public void AddModifier(Modifier modifier)
        {
            _modifiers.Add(new ActiveModifier(modifier));
        }

        // =================================== getter ===================================
        // Consume base stat. Spit the final stat out.
        public float Apply(StatType type, float baseValue)
        {
            float total = baseValue;    // get base stat from hero

            foreach (var modifier in _modifiers)
            {
                if (!FlatBonusTarget.TryGetValue(modifier.Source.GetModifier(), out StatType target)) continue;
                if (target != type) continue;

                total += modifier.Source.GetAmount();   // increase base stat by modifier
            }

            return total;
        }

        // FIXNOW: Apply and SumModifier have the same purpose. To be getter for the modifier. But the name is not justified that.
        public float SumModifier(ModifierEnum type)
        {
            float sum = 0f;
            foreach (var modifier in _modifiers)
                if (modifier.Source.GetModifier() == type) sum += modifier.Source.GetAmount();
            return sum;
        }

        // =================================== modifier helper ===================================
        public bool HasModifier(ModifierEnum type)
        {
            foreach (var modifier in _modifiers)
                if (modifier.Source.GetModifier() == type) return true;
            return false;
        }

        // =================================== active modifier ===================================
        private class ActiveModifier
        {
            public readonly Modifier Source;    // Modifier = heal, buff, debuff, status, etc...
            public float Remaining;             // remember its duration

            public ActiveModifier(Modifier source)
            {
                Source = source;

                float duration = source.GetDuration();

                Remaining = (duration == Permanent) ? float.PositiveInfinity : duration;
            }
        }
    }
}
