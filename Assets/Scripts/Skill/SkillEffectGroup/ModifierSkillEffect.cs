using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    // effect that apply modifer to a recipients
    // e.g. apply wound, apply buff, apply stun, etc...
    [Serializable]
    public class ModifierSkillEffect : SkillEffect
    {
        [SerializeField] private List<ModifierSpec> _modifiers = new List<ModifierSpec>();

        public IReadOnlyList<ModifierSpec> Modifiers => _modifiers;


        public ModifierSkillEffect(EffectRecipientEnum recipient, List<ModifierSpec> modifiers, Cadence cadence = null,
                                   List<SkillCondition> conditions = null, float amplifier = 0f)
            : base(recipient, cadence, conditions, amplifier)
        {
            _modifiers = modifiers ?? new List<ModifierSpec>();
        }

        public override void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients)
        {
            foreach (IDamageable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                // asked once per recipient BC each recipient may have different status which effect amplifier
                float amplifier = AmplifierFor(caster, recipient);

                // add every modifier to recipient
                foreach (ModifierSpec modifier in _modifiers)
                {
                    if (modifier == null) continue;

                    IModifier modifierAfterScale = modifier.Scaled(amplifier);

                    recipient.AddModifier(modifierAfterScale);
                }
            }
        }
    }
}
