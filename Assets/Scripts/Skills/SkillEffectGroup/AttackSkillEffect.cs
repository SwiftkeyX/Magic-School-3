using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // effect that apply damage to recipients
    public class AttackSkillEffect : SkillEffect
    {
        private readonly IReadOnlyList<StatRatio> _damage;

        public AttackSkillEffect(EffectRecipientEnum recipient, IReadOnlyList<StatRatio> damage, Cadence cadence = null,
                                 List<SkillCondition> conditions = null, float amplifier = 0.3f)
            : base(recipient, cadence, conditions, amplifier)
        {
            _damage = damage;
        }

        public override void ApplyEffect(IReadOnlyList<IEffectable> recipients)
        {
            //' scale the damage
            float damageAmount = GetAmountAfterScaling(_damage);

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
