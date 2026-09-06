
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Hunter's Cord: +0.30 Attack Speed
    internal static class HuntersCord
    {
        internal static IModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.AS, 0.3f));
        }
    }
}
