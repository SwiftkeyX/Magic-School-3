using System.Collections.Generic;

namespace MagicSchool
{
    public abstract class SkillEffect
    {
        protected EffectRecipientEnum _recipient;        // the list of recipients who get effect
        protected Cadence _cadence = new Cadence();      // Is effect reapply over time?
        protected List<SkillCondition> _conditions;  // conditions for this effect. => if all satisfied, effect is amplified.
        protected float _amplifier = 0.3f;               // e.g. 0.3 = +30% when the conditions hold

        protected SkillEffect(EffectRecipientEnum recipient, Cadence cadence = null,
                              List<SkillCondition> conditions = null, float amplifier = 0.3f)
        {
            _recipient = recipient;
            _cadence = cadence ?? new Cadence();
            _conditions = conditions ?? new List<SkillCondition>();
            _amplifier = amplifier;
        }

        // ================================== getter ==================================
        public EffectRecipientEnum Recipient => _recipient;
        public Cadence Cadence => _cadence;
        public List<SkillCondition> Conditions => _conditions;

        // check condition to each recipients.
        // If condition is met, the effect is amplified.
        protected float AmplifierFor(IDamageable recipient)
        {
            if (SkillCondition.Ask(_conditions, recipient) == ConditionResultEnum.ConditionIsMet)
            {
                return 1f + _amplifier;
            }

            return 1f;
        }

        public abstract void ApplyEffect(IReadOnlyList<IDamageable> recipients);
    }
}
