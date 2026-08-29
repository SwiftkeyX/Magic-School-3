
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Scholar's Sash: +15 Starting Mana, +200 HP
    internal static class ScholarsSash
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Flat(ModifierEnum.StartMana, 15f),
                ItemFactory.Flat(ModifierEnum.BonusHP, 200f));
        }
    }
}
