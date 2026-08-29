
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // Ley Battery: +20 AP, +15 Starting Mana
    internal static class LeyBattery
    {
        internal static ICustomModifier Build()
        {
            return ItemFactory.Bundle(
                ItemFactory.Buff(ModifierEnum.AP, (StatEnum.None, 20f)),
                ItemFactory.Buff(ModifierEnum.StartMana, (StatEnum.None, 15f)));
        }
    }
}
