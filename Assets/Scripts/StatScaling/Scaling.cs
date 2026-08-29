using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.StatScaling
{
    // Scaling Give answer to this question, "how much total stat do this scaling give?".
    // e.g. a modifier ask scaling the same question, so modifier itself know how much stat will it give to the hero.
    // 
    // usage: HeroA's buff = Scaling(ScalingEnum.Percentage, (StatEnum.ATK, 50f)) 
    // which mean this buff give a (percentage scaling) of (atk for +50%)
    // 
    // warnning: but it doesn't specify where the total stat go into.
    // HeroA's buff derived from atk, but it doesn't mean total stat have to be added into ATK, 
    // the skill may specify to add the total stat into AS instead. 
    // e.g. currently the ModifierEnum use to specify where the total stat go into, ModifierEnum.ATK/ModiferEnum.DF/etc...
    public class Scaling : IScaling
    {
        private readonly ScalingEnum _scalingType;      // Is the scaling Flat or Percentage?
        private readonly IReadOnlyList<StatRatio> _ratios;
        private readonly ScalingSourceEnum _source;     // whose stat will be scaling from
        // FIXNOW: don't use flatAmount like this, use StatEnum.None as a flat amount.
        // bc this using flatAmount giving a 2 entry of how the scaling can be computed, 
        // I prefered it was unified using StatRatio instead.
        private readonly float _flatAmount;

        // =================================== getter ===================================
        public ScalingEnum GetScalingEnum() => _scalingType;
        public ScalingSourceEnum GetScalingSource() => _source;

        public Scaling(ScalingEnum scalingEnum, IReadOnlyList<StatRatio> ratios,
                       ScalingSourceEnum source = ScalingSourceEnum.Caster)
        {
            _scalingType = scalingEnum;
            _ratios = ratios;
            _source = source;
        }

        // FIXLATER: I don't like this one
        // A flat amount carries no ratios and no source, because it reads no stat off anyone.
        // Kept a separate constructor rather than a (StatEnum.None, amount) ratio so nothing has
        // to pass a stat it does not mean.
        public Scaling(float flatAmount)
        {
            _scalingType = ScalingEnum.Flat;
            _flatAmount = flatAmount;
            _ratios = null;
            _source = ScalingSourceEnum.Caster;
        }

        // scale the ratio base on the consuming stats.
        // stats could be from the caster itself or other heroes. (most of the time, is "caster")
        public float GetTotalAfterScaling(IHeroStats stats)
        {
            float total = 0f;

            // if scale was flat, the total was added by _flatAmount directly
            if (_scalingType == ScalingEnum.Flat)
            {
                total = _flatAmount;
            }

            // if scale was percentage, the total was added by deriving from ratio and stats
            else if (_scalingType == ScalingEnum.Percentage)
            {
                // guard
                if (_ratios == null || _ratios.Count == 0) return 0f;

                // guard
                if (stats == null)
                {
                    UnityEngine.Debug.LogError($"[Scaling] scales off the {_source}'s stats but was asked without any. " +
                                               "SkillDefinition.Init() has to reach every effect it holds.");
                    return 0f;
                }

                // e.g. skill dmg = 100% ATK + 50% AP
                foreach (StatRatio ratio in _ratios)
                {
                    if (ratio.Stat == StatEnum.None)
                    {
                        UnityEngine.Debug.LogError("[Scaling] a Percentage ratio was given StatEnum.None, so it has " +
                                                   "no stat to take a share of. Did it mean a flat amount?");
                        continue;
                    }

                    // get bonus stat by deriving from specify stat
                    total += stats.GetStat(ratio.Stat) * ratio.Percent / 100f;
                }
            }

            return total;
        }
    }

}
