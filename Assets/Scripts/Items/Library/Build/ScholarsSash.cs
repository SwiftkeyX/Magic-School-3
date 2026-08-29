
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Scholar's Sash: +15 Starting Mana, +200 HP
    internal static class ScholarsSash
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.StartMana, (StatEnum.None, 15f)),
                ItemFactory.Buff(ModifierEnum.BonusHP, (StatEnum.None, 200f)));
        }
    }
}
