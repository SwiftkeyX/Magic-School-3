
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Vitalis Weave: +200 HP, +5% Damage Reduction
    internal static class VitalisWeave
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.BonusHP, (StatEnum.None, 200f)),
                ItemFactory.Buff(ModifierEnum.DamageReduction, (StatEnum.None, 5f)));
        }
    }
}
