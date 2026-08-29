using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.StatScaling
{
    // Scaling Give answer to this question, "how much total stat do this scaling give?".
    // e.g. a modifier ask scaling the same question, so modifier itself know how much stat will it give to the hero.
    // 
    // read StatRatio.cs for how the scale work?
    //
    // warnning: but it doesn't specify where the total stat go into.
    // HeroA's buff derived from atk, but it doesn't mean total stat have to be added into ATK,
    // the skill may specify to add the total stat into AS instead.
    // e.g. currently the ModifierEnum use to specify where the total stat go into, ModifierEnum.ATK/ModiferEnum.DF/etc...
    public static class Scaling
    {
        // scaling based on the final stat of a unit, this ensure hero who hold item hit harder.
        // usage: skill dmg
        // e.g. Quatre's skill dmg = 744% of AD.
        public static float Total(IReadOnlyList<StatRatio> ratios, IHeroStats stats)
            => Sum(ratios, stats, fromBase: false);

        // scaling based on the base stat of a unit, so the buff is worth the same whenever it
        // lands rather than more for having landed later.
        // usage: item's passive, skill that buff stat
        // e.g. Fang's +100% attack speed = 100% of his BASE attack speed.
        public static float TotalOfBase(IReadOnlyList<StatRatio> ratios, IHeroStats stats)
            => Sum(ratios, stats, fromBase: true);

        // sum the total amount from a specify stats
        private static float Sum(IReadOnlyList<StatRatio> ratios, IHeroStats stats, bool fromBase)
        {
            // guard
            if (ratios == null || ratios.Count == 0) return 0f;

            float total = 0f;

            foreach (StatRatio ratio in ratios)
            {
                bool flatAmountScaling = !ratio.Stat.HasValue;

                // flat scaling
                if (flatAmountScaling)
                {
                    total += ratio.Amount;
                    continue;
                }

                // percentage scaling 
                else
                {
                    // guard
                    if (stats == null)
                    {
                        UnityEngine.Debug.LogError($"[Scaling] a ratio scales off {ratio.Stat.Value} but was asked without any stats " +
                                                   "to read it from. SkillDefinition.Init() has to reach every effect it holds.");
                        continue;
                    }

                    // get bonus stat by deriving from specify stat
                    float from = fromBase ? stats.GetBaseStat(ratio.Stat.Value)
                                          : stats.GetStat(ratio.Stat.Value);

                    total += from * ratio.Amount / 100f;
                }

            }

            return total;
        }
    }

}
