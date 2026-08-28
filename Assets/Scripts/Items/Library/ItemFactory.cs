using MagicSchool.Contracts;
using MagicSchool.StatScaling;
using MagicSchool.Modifiers;

namespace MagicSchool.Items
{
    internal static class ItemFactory
    {
        private const float Permanent = -1f;

        // FIXLATER: item doesn't fix to grant a permanent buff. e.g. item could say OnKill, grant +50% amp dmg for 3 sec.
        // the group of modifiers - everything in it shares one duration. -1f is permanent.
        // An item's grant is permanent: it lasts as long as the item is worn.
        public static ICustomModifier Bundle(params IModifier[] modifiers)
            => new CustomModifier(Permanent, modifiers);

        // a modifier that gives a stat bonus, e.g. Buff(Defend, (StatEnum.Defence, 20f)) is "+20 DF".
        // 1) If have several ratios, it add up.
        // 2) StatEnum.None is a number written straight in rather than derived from any stat.
        // 2.1) Buff(DefendShred, (StatEnum.None, 20f), (StatEnum.MG, 20f)) is "20 flat, plus 20% AP on top".
        public static IModifier Buff(ModifierEnum modifier, params StatRatio[] ratios)
            => new StatModifier(modifier, new Scaling(ScalingEnum.Percentage, ratios));
    }
}
