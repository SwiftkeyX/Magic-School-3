using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    // effect that apply damage to recipients
    public class AttackSkillEffect : SkillEffect
    {
        private float _damageAmount;

        public AttackSkillEffect(EffectRecipientEnum recipient, float damageAmount, Cadence cadence = null,
                                 List<SkillCondition> conditions = null, float amplifier = 0.3f)
            : base(recipient, cadence, conditions, amplifier)
        {
            _damageAmount = damageAmount;
        }

        public override void ApplyEffect(IReadOnlyList<IDamageable> recipients)
        {
            foreach (IDamageable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                // if the damage was cadence, calculate tick damage instead
                float dmg;
                if (_cadence.isCadence) dmg = _damageAmount * _cadence.cadenceInterval / _cadence.cadenceDuration;

                // if not cadence, apply normal flat damage
                else dmg = _damageAmount;

                // if pass specify condition, amplify the effect
                dmg *= AmplifierFor(recipient);

                // apply damage
                recipient.TakeDamage(Mathf.RoundToInt(dmg));
            }
        }
    }
}
