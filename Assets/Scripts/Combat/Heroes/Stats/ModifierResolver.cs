using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Combat.Heroes.Stats
{
    // 1) ModifierResolver are used to calculate final stat from modifier:
    // 1.1) stat modifier: they increase/decrease stat e.g. atk, def, as, mana, hp, etc...
    // 1.2) status modifier: they don't relate to stat e.g. stun, wound, untargetable, disarm, etc...
    // 2) ModifierResolver track which modifier will expired
    public class ModifierResolver
    {
        private const float Permanent = -1f;

        private readonly List<ActiveCustomModifier> _activeModifiers = new List<ActiveCustomModifier>();

        // pair of modifier & stat - tell which stat is increase by this modifier
        private static readonly Dictionary<ModifierEnum, StatEnum> _lookup = new Dictionary<ModifierEnum, StatEnum>
        {
            // buff
            { ModifierEnum.BonusHP        , StatEnum.MAXHP }          ,
            { ModifierEnum.Attack         , StatEnum.Atk }            ,
            { ModifierEnum.AttackSpeed    , StatEnum.AttackSpeed}     ,
            { ModifierEnum.Defend         , StatEnum.DF }             ,
            { ModifierEnum.DamageReduction, StatEnum.DamageReduction },
            // ...

            // debuff
            { ModifierEnum.DefendShred    , StatEnum.DF }             ,
            // ...
        };

        // =================================== life cycle ===================================
        // update all current modifier duration, removing any that just expired
        public void Tick(float deltaTime)
        {
            // backwards, so removing one of the modifier does not skip over the next
            for (int i = _activeModifiers.Count - 1; i >= 0; i--)
            {
                // if permanent, skip timer
                if (float.IsPositiveInfinity(_activeModifiers[i].Remaining)) continue;

                // update timer
                _activeModifiers[i].Remaining -= deltaTime;

                // if the group expired, every modifier in it goes at once
                if (_activeModifiers[i].Remaining <= 0f) _activeModifiers.RemoveAt(i);
            }
        }

        // =================================== setter ===================================
        // add new modifier
        // FLAGGING: amplifier should be unique to each modifier. But let leave it for now.
        public void AddModifier(ICustomModifier modifier, float amplifier)
        {
            // if the same modifier is added again, refresh that modifier
            for (int i = 0; i < _activeModifiers.Count; i++)
            {
                // is new added modifier the same to current active one?
                if (!ReferenceEquals(_activeModifiers[i].CustomModifier, modifier)) continue;

                // refresh modifier
                _activeModifiers[i] = new ActiveCustomModifier(modifier, amplifier);
                return;
            }

            // add modifier
            _activeModifiers.Add(new ActiveCustomModifier(modifier, amplifier));
        }

        // =================================== getter ===================================
        // Compute final stat after calculating modifier.
        // Consume base stat. Spit the final stat out.
        public float GetStatModifier(StatEnum type, float baseValue)
        {
            float flat = 0f;        // add flat stat
            float percent = 0f;     // add percentage of the base stat, in percent points: 80f = +80%

            foreach (ActiveCustomModifier tracker in _activeModifiers)
            {
                foreach (IModifier modifier in tracker.CustomModifier.GetModifiers())
                {
                    // lookup modifier table - what stat is increase?
                    if (!_lookup.TryGetValue(modifier.GetModifierEnum(), out StatEnum target)) continue;

                    // let the modifier with correct stat pass.
                    if (target != type) continue;

                    // apply amplifier if exist e.g. +30% when the target was wounded
                    float amount = modifier.GetAmount() * tracker.Amplifier;

                    // scale stat e.g. flat +50, percentage +100%
                    ScalingEnum scalingEnum = modifier.GetScalingEnum();
                    if (scalingEnum == ScalingEnum.Flat)
                        flat += amount;

                    else if (scalingEnum == ScalingEnum.Percentage)
                        percent += amount;

                    else { }
                }
            }

            return (baseValue + flat) * (1f + percent / 100f);
        }

        // Return available status modifier
        public bool GetStatusModifier(ModifierEnum type)
        {
            foreach (ActiveCustomModifier tracker in _activeModifiers)
                foreach (IModifier modifier in tracker.CustomModifier.GetModifiers())
                    if (modifier.GetModifierEnum() == type) return true;

            return false;
        }

        // return the count of active modifier 
        public int ActiveCount => _activeModifiers.Count;

        // get remaining duration of the active modifier
        public float GetRemainingDuration(int index)
        {
            if (index < 0 || index >= _activeModifiers.Count) return 0f;

            ActiveCustomModifier tracker = _activeModifiers[index];

            if (float.IsPositiveInfinity(tracker.Remaining)) return 0f;

            float duration = tracker.CustomModifier.GetDuration();
            if (duration <= 0f) return 0f;

            return tracker.Remaining / duration;
        }

        // =================================== active modifier ===================================
        // Contain current active group of modifiers on the hero. 
        // To track how long this group last.
        private class ActiveCustomModifier
        {
            public readonly ICustomModifier CustomModifier;   // the group of modifier - buff, debuff, status, etc...
            public readonly float Amplifier;            // what this application was scaled by, fixed at the moment it landed
            public float Remaining;                     // remember its remaining duration of the modifier - The group share the same remaining

            public ActiveCustomModifier(ICustomModifier source, float amplifier)
            {
                CustomModifier = source;
                Amplifier = amplifier;

                float duration = source.GetDuration();

                Remaining = (duration == Permanent) ? float.PositiveInfinity : duration;
            }
        }
    }
}
