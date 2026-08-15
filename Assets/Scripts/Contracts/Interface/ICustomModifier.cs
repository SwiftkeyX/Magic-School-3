using System.Collections.Generic;

namespace MagicSchool.Contracts
{
    // It is a list of modifier build together into 1 custom modifier.
    public interface ICustomModifier
    {
        float GetDuration();     // All modifier in the list shared a same duration.

        IReadOnlyList<IModifier> GetModifiers();
    }
}
