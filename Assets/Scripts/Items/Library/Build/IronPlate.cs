
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    internal static class IronPlate
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(ItemFactory.Buff(ModifierEnum.DF, (StatEnum.None, 20f)));
        }
    }
}