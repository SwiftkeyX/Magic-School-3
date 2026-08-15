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
    }
}
