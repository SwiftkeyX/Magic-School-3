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

        // FIXLATER: the name suck
        // a modifier that gives a plain stat bonus, e.g. Flat(DF, 20f) is "+20 DF".
        // The number is what the hero gets, whoever the hero is - it reads no stat off anyone.
        // This is what every item is built from today.
        public static IModifier Flat(ModifierEnum modifier, float amount)
            => new StatModifier(modifier, new Scaling(amount));

        // a modifier that gives a share of ANOTHER stat, e.g. Buff(ATK, (StatEnum.AP, 50f)) is
        // "+ATK worth 50% of the wearer's AP".
        // 1) If have several ratios, it add up.
        // 2) For a number written straight in, reach for Flat above rather than a ratio.
        public static IModifier Buff(ModifierEnum modifier, params StatRatio[] ratios)
            => new StatModifier(modifier, new Scaling(ScalingEnum.Percentage, ratios));
    }
}
