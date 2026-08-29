
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Reaver's Edge: +12 Atk, +150 HP
    internal static class ReaversEdge
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.ATK, (StatEnum.None, 12f)),
                ItemFactory.Buff(ModifierEnum.BonusHP, (StatEnum.None, 150f)));
        }
    }
}
