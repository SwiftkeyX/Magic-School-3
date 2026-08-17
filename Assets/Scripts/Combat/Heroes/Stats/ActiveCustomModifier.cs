
using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Combat.Heroes.Stats
{
    // Contain current active group of modifiers on the hero. 
    // To track how long this group of modifiers last.
    public class ActiveCustomModifier
    {
        private const float Permanent = -1f;
        public readonly ICustomModifier CustomModifier; // the group of modifier - buff, debuff, status, etc...
        public readonly float Amplifier;
        public readonly float[] BonusStat;              //the amount of total stat that will be added to hero
        public float Remaining;                         // remember its remaining duration of the modifier - The group share the same remaining
        private float _maxDuration;                     // Max duration

        public ActiveCustomModifier(ICustomModifier source, float amplifier, IHeroStats casterStats, IHeroStats recipientStats)
        {
            CustomModifier = source;
            Amplifier = amplifier;

            // get bonus stat from each modifier
            IReadOnlyList<IModifier> modifiers = source.GetModifiers();
            BonusStat = new float[modifiers.Count];
            for (int i = 0; i < modifiers.Count; i++)
            {
                // the bonus stat could derive from caster itself or others hero that being hit by the skill.
                IHeroStats from;
                if (modifiers[i].GetScalingSource() == ScalingSourceEnum.Caster)
                {
                    from = casterStats;
                }
                else if (modifiers[i].GetScalingSource() == ScalingSourceEnum.Recipient)
                {
                    from = recipientStats;
                }
                // fallback
                else from = casterStats;

                // get bonus stat
                BonusStat[i] = modifiers[i].GetBonusAmount(from);
            }

            // get duration of the modifier
            _maxDuration = source.GetDuration();

            Remaining = StartingRemaining(_maxDuration);
        }

        public void RefreshModifierDuration() => Remaining = StartingRemaining(_maxDuration);

        private static float StartingRemaining(float duration)
            => (duration == Permanent) ? float.PositiveInfinity : duration;
    }

}