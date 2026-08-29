
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Bulwark Crest: +15 DF, +150 HP
    internal static class BulwarkCrest
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.DF, (StatEnum.None, 15f)),
                ItemFactory.Buff(ModifierEnum.BonusHP, (StatEnum.None, 150f)));
        }
    }
}
