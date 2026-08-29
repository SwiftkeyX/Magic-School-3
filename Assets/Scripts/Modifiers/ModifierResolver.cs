using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Modifiers
{
    // 1) ModifierResolver are used to calculate final stat from modifier:
    // 1.1) stat modifier: they increase/decrease stat e.g. atk, def, as, mana, hp, etc...
    // 1.2) status modifier: they don't relate to stat e.g. stun, wound, untargetable, disarm, etc...
    // 2) ModifierResolver track which modifier will expired
    public class ModifierResolver
    {
        private readonly List<ActiveCustomModifier> _activeModifiers = new List<ActiveCustomModifier>();

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
            // FLAGGING: we shouldn't have to loop all the modifiers, but RefernceEqual is still need, 
            // we can use dict to help this.
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

        // force remove modifier before it expired
        public bool RemoveModifier(ICustomModifier modifier)
        {
            if (modifier == null) return false;

            // FLAGGING: we shouldn't have to loop all the modifiers.
            for (int i = 0; i < _activeModifiers.Count; i++)
            {
                if (!ReferenceEquals(_activeModifiers[i].CustomModifier, modifier)) continue;

                _activeModifiers.RemoveAt(i);
                return true;
            }

            return false;
        }

        // =================================== getter ===================================
        // Compute final stat after calculating modifier.
        // Consume base stat. Spit the final stat out.
        // FIXLATER: now this function always recompute every single time hero called its atk, let change that.
        // how about we compute it 1 time , and use it forever.
        public float GetStatModifier(StatEnum type, float baseStat)
        {
            float additive = 0f;
            float multiplierPercent = 0f;

            foreach (ActiveCustomModifier active in _activeModifiers)
            {
                IReadOnlyList<IModifier> modifiers = active.CustomModifier.GetModifiers();

                for (int i = 0; i < modifiers.Count; i++)
                {
                    IModifier modifier = modifiers[i];

                    // lookup modifier table - what stat is increase?
                    if (!ModifierTarget.TryGet(modifier.GetModifierEnum(), out StatEnum target)) continue;

                    // let the modifier with correct stat pass.
                    if (target != type) continue;

                    additive += active.BonusStat[i].Additive;
                    multiplierPercent += active.BonusStat[i].MultiplierPercent;
                }
            }

            return (baseStat + additive) * (1f + multiplierPercent / 100f);
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
