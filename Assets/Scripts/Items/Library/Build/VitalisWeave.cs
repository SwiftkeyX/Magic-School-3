
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Vitalis Weave: +200 HP, +5% Damage Reduction
    internal static class VitalisWeave
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Flat(ModifierEnum.BonusHP, 200f),
                ItemFactory.Flat(ModifierEnum.DamageReduction, 5f));
        }
    }
}
