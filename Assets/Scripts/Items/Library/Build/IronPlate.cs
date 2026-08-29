
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    internal static class IronPlate
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(ItemFactory.Flat(ModifierEnum.DF, 20f));
        }
    }
}