using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Modifiers
{
    // a modifer that was granted to hero to boost their combat performance.
    // modifier could be grated by skill or items.
    public class Modifier : IModifier
    {
        // modifier is a list of smaller modifier piece combine together.
        private readonly IReadOnlyList<IModifierPiece> _pieces;
        private readonly float _duration;
        public float GetDuration() => _duration;
        public IReadOnlyList<IModifierPiece> GetPieces() => _pieces;

        public Modifier(float duration, IReadOnlyList<IModifierPiece> pieces)
        {
            _duration = duration;
            _pieces = pieces ?? new List<IModifierPiece>();
        }

    }
}
