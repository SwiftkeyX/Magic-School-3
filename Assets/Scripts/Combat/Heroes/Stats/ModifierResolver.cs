using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Combat.Heroes.Stats
{
    // 1) ModifierResolver are used to calculate final stat from modifier:
    // 1.1) stat modifier: they increase/decrease stat e.g. atk, def, as, mana, hp, etc...
    // 1.2) status modifier: they don't relate to stat e.g. stun, wound, untargetable, disarm, etc...
    // 2) ModifierResolver track which modifier will expired
    internal class ModifierResolver
    {
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
                _activeModifiers[i].Tick(deltaTime);

                // if the group expired, every modifier in it goes at once
                if (_activeModifiers[i].Remaining <= 0f) _activeModifiers.RemoveAt(i);
            }
        }

        // =================================== setter ===================================
        // add new modifier
        public void AddModifier(ICustomModifier modifier, float amplifier, IHeroStats casterStats, IHeroStats recipientStats)
        {
            // if the same modifier is added again, refresh the modifier
            for (int i = 0; i < _activeModifiers.Count; i++)
            {
                // is new added modifier the same to current active one?
                if (!ReferenceEquals(_activeModifiers[i].CustomModifier, modifier)) continue;

                // FLAGGING: Depend on skill, some skill can be stacked, while some skill refresh cleanly.
                // Now only refresh exist. We'll do this later when pattern is more clear.
                // refresh modifier
                _activeModifiers.RemoveAt(i);
                break;
            }

            // add modifier
            _activeModifiers.Add(new ActiveCustomModifier(modifier, amplifier, casterStats, recipientStats));
        }

        // =================================== getter ===================================
        // Compute final stat after calculating modifier.
        // Consume base stat. Spit the final stat out.
        // FLAGGING: The stat are re-compute single time hero attack. Let cache this later.
        public float GetStatModifier(StatEnum type, float baseStat)
        {
            float pureStatFromPercentageBonus = 0f;

            foreach (ActiveCustomModifier active in _activeModifiers)
            {
                IReadOnlyList<IModifier> modifiers = active.CustomModifier.GetModifiers();

                for (int i = 0; i < modifiers.Count; i++)
                {
                    IModifier modifier = modifiers[i];

                    // lookup modifier table - what stat is increase?
                    if (!_lookup.TryGetValue(modifier.GetModifierEnum(), out StatEnum target)) continue;

                    // let the modifier with correct stat pass.
                    if (target != type) continue;

                    // scale stat e.g. flat +50, percentage +100%
                    // the percentage is derived from StatRatio 
                    ScalingEnum scalingEnum = modifier.GetScalingEnum();

                    if (scalingEnum == ScalingEnum.Percentage)
                        pureStatFromPercentageBonus += active.BonusStat[i];

                    else { }
                }
            }

            return (baseStat + pureStatFromPercentageBonus);
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
    }
}
