
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Bulwark Crest: +15 DF, +150 HP
    internal static class BulwarkCrest
    {
        internal static IModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.DF, 15f),
                ItemFactory.Buff(ModifierEnum.BonusHP, 150f));
        }
    }
}
