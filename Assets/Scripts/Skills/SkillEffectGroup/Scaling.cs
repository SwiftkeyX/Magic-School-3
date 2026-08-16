
using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // Specify scaling type the stat is using base on the modifier.
    // the amount is derived from percentage, it need "StatRatio" to tell which stat it derived from.
    // e.g. atk stat = baseATK +200% AP
    // e.g. skill damage = 100% ATK + 50% AP
    public class Scaling
    {
        private readonly ScalingEnum _scalingType; // Is the scaling Flat or Percentage?
        private readonly IReadOnlyList<StatRatio> _ratios;

        // =================================== getter ===================================
        public ScalingEnum GetScalingEnum() => _scalingType;

        public Scaling(ScalingEnum scalingEnum, IReadOnlyList<StatRatio> ratios)
        {
            _scalingType = scalingEnum;
            _ratios = ratios;
        }

        public float GetTotalAfterScaling(IHeroStats stats)
        {
            // guard
            if (_ratios == null || _ratios.Count == 0) return 0f;

            // FLAGGING: another class type check, let leave it for now.
            // Somehow the effect always need stat from hero, maybe we'll have to include IHeroStats into ICombatant?
            // guard
            if (stats == null)
            {
                UnityEngine.Debug.LogError("[Scaling] scales off the caster's stats but was asked without any. " +
                                           "SkillDefinition.Init() has to reach every effect it holds.");
                return 0f;
            }

            float total = 0f;

            // scale amount base on ratio
            if (_scalingType == ScalingEnum.Percentage)
            {
                // e.g. skill dmg = 100% ATK + 50% AP
                foreach (StatRatio ratio in _ratios)
                {
                    total += stats.GetStat(ratio.Stat) * ratio.Percent / 100f;
                }
            }

            return total;
        }
    }

}
