
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Duelist's Gauntlet: +10 Atk, +0.12 Attack Speed
    internal static class DuelistGauntlet
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.ATK, (StatEnum.None, 10f)),
                ItemFactory.Buff(ModifierEnum.AS, (StatEnum.None, 0.12f)));
        }
    }
}
