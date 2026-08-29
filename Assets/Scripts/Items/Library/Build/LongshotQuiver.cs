
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Longshot Quiver: +1 Range, +0.12 Attack Speed
    internal static class LongshotQuiver
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.Range, (StatEnum.None, 1f)),
                ItemFactory.Buff(ModifierEnum.AS, (StatEnum.None, 0.12f)));
        }
    }
}
