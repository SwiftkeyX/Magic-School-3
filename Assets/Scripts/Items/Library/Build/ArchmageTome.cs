
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Archmage Tome: +55 AP
    internal static class ArchmageTome
    {
        internal static IModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.AP, 55f));
        }
    }
}
