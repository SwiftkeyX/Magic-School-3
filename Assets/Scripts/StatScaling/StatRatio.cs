using MagicSchool.Contracts;

namespace MagicSchool.StatScaling
{
    // This is the stat ratio of the skill. Use in calculation.
    // e.g. Quatre's skill = 744% AD damage
    public readonly struct StatRatio
    {
        public readonly StatEnum Stat;      // which stat to read, e.g. ATK for AD, AP for ability power
        public readonly float Percent;      // 200f = 200% of it

        public StatRatio(StatEnum stat, float percent)
        {
            Stat = stat;
            Percent = percent;
        }

        // conversion rule NOT constructor.
        // it change (stat, percent) to new type StatRatio(). It's here so the skill builder doesn't get too messy.
        // E.g. { (StatEnum.ATK, 100f), (StatEnum.AP, 50f) } => +100% atk & +50% ap 
        public static implicit operator StatRatio((StatEnum stat, float percent) pair)
        {
            return new StatRatio(pair.stat, pair.percent);
        }
    }
}
