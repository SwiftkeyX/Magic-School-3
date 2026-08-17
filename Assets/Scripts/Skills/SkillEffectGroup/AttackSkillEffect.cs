using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // effect that apply damage to recipients
    internal class AttackSkillEffect : SkillEffect
    {
        private readonly IScaling _scaling;

        public AttackSkillEffect(EffectRecipientEnum recipient, IScaling scaling, Cadence cadence = null,
                                 List<SkillCondition> conditions = null, float amplifier = 0f)
            : base(recipient, cadence, conditions, amplifier)
        {
            _scaling = scaling;
        }

        public override void ApplyEffect(IReadOnlyList<IEffectable> recipients)
        {
            // scale the damage e.g. skill damage = 500% AP
            float damageAmount = _scaling.GetTotalAfterScaling(_caster as IHeroStats);

            foreach (IEffectable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                // if the damage was cadence, calculate tick damage instead
                float dmg;
                if (_cadence.isCadence) dmg = damageAmount * _cadence.cadenceInterval / _cadence.cadenceDuration;

                // if not cadence, apply the whole amount at once
                else dmg = damageAmount;

                // if pass specify condition, amplify the effect
                dmg *= AmplifierFor(recipient);

                // apply damage
                recipient.TakeDamage(Mathf.RoundToInt(dmg));
            }
        }
    }
}
