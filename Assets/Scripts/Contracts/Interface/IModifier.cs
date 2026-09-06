using System.Collections.Generic;

namespace MagicSchool.Contracts
{
    // It is a list of pieces build together into 1 modifier.
    // It is to answer: to make list of IModifierPiece shared the same duration.
    // e.g. Vharn's "WorldEnder" contain like 5 pieces, but all of them have the same duration.
    public interface IModifier
    {
        float GetDuration();                        // All piece in the list shared a same duration.
        IReadOnlyList<IModifierPiece> GetPieces();  // Get all piece inside
    }
}
