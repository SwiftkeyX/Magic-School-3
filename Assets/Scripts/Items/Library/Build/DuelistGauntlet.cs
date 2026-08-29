
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Duelist's Gauntlet: +10 Atk, +0.12 Attack Speed
    internal static class DuelistGauntlet
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Flat(ModifierEnum.ATK, 10f),
                ItemFactory.Flat(ModifierEnum.AS, 0.12f));
        }
    }
}
