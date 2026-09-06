
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    internal static class IronPlate
    {
        internal static IModifier Build()
        {
            return ItemFactory.Bundle(ItemFactory.Buff(ModifierEnum.DF, 20f));
        }
    }
}