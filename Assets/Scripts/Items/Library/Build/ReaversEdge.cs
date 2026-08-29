
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Reaver's Edge: +12 Atk, +150 HP
    internal static class ReaversEdge
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Flat(ModifierEnum.ATK, 12f),
                ItemFactory.Flat(ModifierEnum.BonusHP, 150f));
        }
    }
}
