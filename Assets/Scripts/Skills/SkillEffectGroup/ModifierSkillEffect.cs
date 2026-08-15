using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // effect that apply modifer to a recipients
    // e.g. apply wound, apply buff, apply stun, etc...
    public class ModifierSkillEffect : SkillEffect
    {
        private List<ModifierSpec> _modifiers = new List<ModifierSpec>();

        public IReadOnlyList<ModifierSpec> Modifiers => _modifiers;

        public ModifierSkillEffect(EffectRecipientEnum recipient, List<ModifierSpec> modifiers, Cadence cadence = null,
                                   List<SkillCondition> conditions = null, float amplifier = 0f)
            : base(recipient, cadence, conditions, amplifier)
        {
            _modifiers = modifiers ?? new List<ModifierSpec>();
        }

        public override void ApplyEffect(IReadOnlyList<IEffectable> recipients)
        {
            foreach (IEffectable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                // asked once per recipient BC each recipient may have different status which effect amplifier
                float amplifier = AmplifierFor(recipient);

                // add every modifier to recipient
                foreach (ModifierSpec modifier in _modifiers)
                {
                    if (modifier == null) continue;

                    // the original spec is shared by every cast.
                    // so modifier with scaling need to be new instance.
                    IModifier modifierAfterScale = modifier.WithAmount(modifier.GetAmount() * amplifier);

                    recipient.AddModifier(modifierAfterScale);
                }
            }
        }
    }
}
