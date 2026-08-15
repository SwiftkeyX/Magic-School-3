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

        public CustomModifier(float duration, IReadOnlyList<ModifierSpec> modifiers)
        {
            _duration = duration;
            _modifiers = modifiers ?? new List<ModifierSpec>();
        }

    }
}
