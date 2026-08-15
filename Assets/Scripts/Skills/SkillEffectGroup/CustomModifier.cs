using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    public class CustomModifier : ICustomModifier
    {
        private readonly IReadOnlyList<ModifierSpec> _modifiers;
        private readonly float _duration;
        public float GetDuration() => _duration;
        public IReadOnlyList<IModifier> GetModifiers() => _modifiers;

        // FIXNOW: use list
        public CustomModifier(float duration, params ModifierSpec[] modifiers)
        {
            _duration = duration;
            _modifiers = modifiers ?? new ModifierSpec[0];
        }

    }
}
