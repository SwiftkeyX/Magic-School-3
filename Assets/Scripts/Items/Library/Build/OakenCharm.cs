
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Oaken Charm: +250 HP
    internal static class OakenCharm
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Flat(ModifierEnum.BonusHP, 250f));
        }
    }
}
