
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Stormglass: +25 AP, +0.12 Attack Speed
    internal static class Stormglass
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.AP, (StatEnum.None, 25f)),
                ItemFactory.Buff(ModifierEnum.AS, (StatEnum.None, 0.12f)));
        }
    }
}
