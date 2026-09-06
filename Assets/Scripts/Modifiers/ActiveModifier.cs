
using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Modifiers
{
    // Contain current active group of modifiers on the hero. 
    // 1) To track how long this group of modifiers last.
    // 2) To contain how much bonus stat this modifier'll give. 
    internal class ActiveModifier
    {
        private const float Permanent = -1f;
        private float _remaining;

        // ============================================ getter ============================================
        public readonly IModifier Modifier;                 // the modifier being tracked - buff, debuff, status, etc...
        public readonly float[] BonusStat;                  //the amount of total stat that will be added to hero, amplifier included
        public float Remaining => _remaining;               // remember its remaining duration of the modifier - The group share the same remaining

        public ActiveModifier(IModifier source, float amplifier, IHeroStats casterStats, IHeroStats recipientStats)
        {
            Modifier = source;

            // get bonus stat from each piece
            IReadOnlyList<IModifierPiece> pieces = source.GetPieces();
            BonusStat = new float[pieces.Count];
            for (int i = 0; i < pieces.Count; i++)
            {
                // the bonus stat could derive from caster itself or others hero that being hit by the skill.
                IHeroStats from;
                if (pieces[i].GetScalingSource() == ScalingSourceEnum.Caster)
                {
                    from = casterStats;
                }
                else if (pieces[i].GetScalingSource() == ScalingSourceEnum.Recipient)
                {
                    from = recipientStats;
                }
                // fallback
                else from = casterStats;

                // get bonus stat, amplified if the effect's conditions is true.
                // e.g. +30% when the target was wounded.
                BonusStat[i] = pieces[i].GetBonusAmount(from) * amplifier;
            }

            // start the modifier's timer
            _remaining = StartingRemaining(source.GetDuration());
        }

        public void Tick(float deltaTime)
        {
            _remaining -= deltaTime;
        }

        private static float StartingRemaining(float duration)
            => (duration == Permanent) ? float.PositiveInfinity : duration;
    }

}