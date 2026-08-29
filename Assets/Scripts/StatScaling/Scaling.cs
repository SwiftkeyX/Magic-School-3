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
    public class Scaling : IScaling
    {
        private readonly IReadOnlyList<StatRatio> _ratios;
        private readonly ScalingSourceEnum _source;     // whose stat will be scaling from

        // =================================== getter ===================================
        public ScalingSourceEnum GetScalingSource() => _source;

        public Scaling(IReadOnlyList<StatRatio> ratios, ScalingSourceEnum source = ScalingSourceEnum.Caster)
        {
            _ratios = ratios;
            _source = source;
        }

        // scale the ratio base on the consuming stats.
        // stats could be from the caster itself or other heroes. (most of the time, is "caster")
        public float GetTotalAfterScaling(IHeroStats stats)
        {
            // guard
            if (_ratios == null || _ratios.Count == 0) return 0f;

            float total = 0f;

            foreach (StatRatio ratio in _ratios)
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
                        UnityEngine.Debug.LogError($"[Scaling] scales off the {_source}'s stats but was asked without any. " +
                                                   "SkillDefinition.Init() has to reach every effect it holds.");
                        continue;
                    }

                    // get bonus stat by deriving from specify stat
                    total += stats.GetStat(ratio.Stat.Value) * ratio.Amount / 100f;
                }

            }

            return total;
        }
    }

}
