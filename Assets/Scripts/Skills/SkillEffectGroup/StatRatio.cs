using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // This is the stat ratio of the skill. Use in calculation.
    // e.g. Jhin's skill = 744% AD damage
    public readonly struct StatRatio
    {
        public readonly StatEnum Stat;      // which of the caster's stats to read, e.g. Atk for AD, MG for AP
        public readonly float Percent;      // 200f = 200% of it

        public StatRatio(StatEnum stat, float percent)
        {
            Stat = stat;
            Percent = percent;
        }

        // conversion rule NOT constructor.
        // it change (stat, percent) to new type StatRatio(). It's here so the skill builder doesn't get too messy.
        // E.g. { (StatEnum.Atk, 100f), (StatEnum.MG, 50f) } => +100% atk & +50% mg 
        public static implicit operator StatRatio((StatEnum stat, float percent) pair)
        {
            return new StatRatio(pair.stat, pair.percent);
        }
    }
}
