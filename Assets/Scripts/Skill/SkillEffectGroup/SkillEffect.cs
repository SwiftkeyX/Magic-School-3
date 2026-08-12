using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    [Serializable]
    public abstract class SkillEffect
    {
        [SerializeField] protected EffectRecipientEnum _recipient;        // the list of recipients who get effect
        [SerializeField] protected Cadence _cadence = new Cadence();      // Is effect reapply over time?
        [SerializeReference] protected List<SkillCondition> _conditions;  // conditions for this effect. => if all satisfied, effect is amplified.
        [ShowIf(nameof(_conditions))]
        [SerializeField] protected float _amplifier = 0.3f;               // e.g. 0.3 = +30% when the conditions hold

        // ================================== getter ==================================
        public EffectRecipientEnum Recipient => _recipient;
        public Cadence Cadence => _cadence;
        public List<SkillCondition> Conditions => _conditions;

        // check condition to each recipients.
        // If condition is met, the effect is amplified.
        // FIXNOW: this should only know recipient too.
        protected float AmplifierFor(IDamageable caster, IDamageable recipient)
        {
            if (SkillCondition.Ask(_conditions, caster, recipient) == ConditionResultEnum.ConditionIsMet)
            {
                return 1f + _amplifier;
            }

            return 1f;
        }

        // FIXNOW: apply effect should only know recipients. It shouldn't know caster too.
        // if caster was recipients, it should be send in as recipients.
        public abstract void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients);
    }
}
