
using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Combat.Heroes.Stats
{
    // =================================== active modifier ===================================
    // Contain current active group of modifiers on the hero. 
    // To track how long this group last.
    public class ActiveCustomModifier
    {
        private const float Permanent = -1f;
        public readonly ICustomModifier CustomModifier;   // the group of modifier - buff, debuff, status, etc...
        public readonly float Amplifier;
        public readonly float[] BonusStat;            //the amount of total stat that will be added to hero
        public float Remaining;                     // remember its remaining duration of the modifier - The group share the same remaining

        public ActiveCustomModifier(ICustomModifier source, float amplifier, IHeroStats casterStats)
        {
            CustomModifier = source;
            Amplifier = amplifier;

            // get bonus stat from each modifier
            IReadOnlyList<IModifier> modifiers = source.GetModifiers();
            BonusStat = new float[modifiers.Count];
            for (int i = 0; i < modifiers.Count; i++) BonusStat[i] = modifiers[i].GetBonusAmount(casterStats);

            // get duration of the modifier
            float duration = source.GetDuration();

            // if modifier is permanent, return permanent 
            Remaining = (duration == Permanent) ? float.PositiveInfinity : duration;
        }
    }

}