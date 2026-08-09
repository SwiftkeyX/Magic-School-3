using System.Collections.Generic;

namespace MagicSchool
{
    public interface Modifier
    {
        public float GetAmount();
        public ModifierEnum GetModifierEnum();
        public float GetDuration();
    }

    // 1) ModifierResolver are used to calculate final stat from modifier:
    // 1.1) stat modifier: they increase/decrease stat e.g. atk, def, as, mana, hp, etc...
    // 1.2) status modifier: they don't relate to stat e.g. stun, wound, untargetable, disarm, etc... 
    // 2) ModifierResolver track which modifier will expired
    public class ModifierResolver
    {
        private const float Permanent = -1f;

        private readonly Dictionary<ModifierEnum, ModifierTracker> _modifierTracker = new Dictionary<ModifierEnum, ModifierTracker>();

        // to track modifier that was expired
        private readonly List<ModifierEnum> _expired = new List<ModifierEnum>();

        // pair of modifier & stat - tell which stat is increase by this modifier
        private static readonly Dictionary<ModifierEnum, StatType> _lookup = new Dictionary<ModifierEnum, StatType>
        {
            { ModifierEnum.BonusHP, StatType.HP },
            { ModifierEnum.Attack, StatType.Atk },
            { ModifierEnum.DamageReduction, StatType.DamageReduction },
            // ...
        };

        // =================================== life cycle ===================================
        // update all current modifier duration, removing any that just expired
        public void Tick(float deltaTime)
        {
            _expired.Clear();

            foreach (var tracker in _modifierTracker)
            {
                // if permanent, skip timer
                if (float.IsPositiveInfinity(tracker.Value.Remaining)) continue;

                // update timer
                tracker.Value.Remaining -= deltaTime;

                // if modifier expired, keep it in expired list
                if (tracker.Value.Remaining <= 0f) _expired.Add(tracker.Key);
            }

            // remove expired modifier in 1 go
            foreach (ModifierEnum type in _expired) _modifierTracker.Remove(type);
        }

        // =================================== setter ===================================
        // add new modifier
        public void AddModifier(Modifier modifier)
        {
            _modifierTracker[modifier.GetModifierEnum()] = new ModifierTracker(modifier);
        }

        // =================================== getter ===================================
        // Compute final stat after modifier.
        // Consume base stat. Spit the final stat out.
        public float GetStatModifier(StatType type, float baseValue)
        {
            float total = baseValue;    // get base stat from hero

            foreach (var tracker in _modifierTracker)
            {
                // lookup modifier table - what stat is increase?
                if (!_lookup.TryGetValue(tracker.Value.Modifier.GetModifierEnum(), out StatType target)) continue;

                // guard
                if (target != type) continue;

                // increase base stat by modifier
                total += tracker.Value.Modifier.GetAmount();
            }

            return total;
        }

        // Return available status modifier
        public bool GetStatusModifier(ModifierEnum type) => _modifierTracker.ContainsKey(type);

        // =================================== active modifier ===================================
        // Current active modifer on the hero. To track how long this modifer last.
        private class ModifierTracker
        {
            public readonly Modifier Modifier;      // Modifier = heal, buff, debuff, status, etc...
            public float Remaining;                 // remember its duration

            public ModifierTracker(Modifier source)
            {
                Modifier = source;

                float duration = source.GetDuration();

                Remaining = (duration == Permanent) ? float.PositiveInfinity : duration;
            }
        }
    }
}
