
using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // Specify scaling type the stat is using base on the modifier.
    // if the amount is increase by flat amount.
    // e.g. atk stat = baseATK +50 flat amount
    // if the amount is derived from percentage, it need "StatRatio" to tell which stat it derived from.
    // e.g. atk stat = baseATK +200% AP
    // e.g. skill damage = 100% ATK + 50% AP


    /// <summary>
    /// FIXNOW: It's so confusing to do Scaling for both AttackSkillEffect & Modifier at the same time.
    /// Let focus on making functionality first, by making Scaling work for only AttackSkillEffect.
    /// It orignially work by only consuming StatRatio & IHeroStats, let see.
    /// Scaling by AttackSkillEffect only use the Percentage one.
    /// </summary>
    public class Scaling
    {
        private readonly ScalingEnum _scalingType; // Is the scaling Flat or Percentage?
        private readonly IReadOnlyList<StatRatio> _ratios;

        // =================================== getter ===================================
        public ScalingEnum GetScalingEnum() => _scalingType;

        public Scaling(IReadOnlyList<StatRatio> ratios)
        {
            _scalingType = ScalingEnum.Percentage;
            _ratios = ratios;
        }

        // The caster is a parameter, NOT a field held from the constructor.
        // A skill is built once by a static Build() that has no caster to hand over - the caster
        // only arrives later, through SkillDefinition.Init(). Reading it here also means the
        // damage follows the caster's stats as they move during the fight, instead of freezing
        // whatever they were at build time.
        public float GetFinalAmount(IHeroStats stats)
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

            float damageTotal = 0f;

            // scale amount base on ratio
            if (_scalingType == ScalingEnum.Percentage)
            {
                // e.g. skill dmg = 100% ATK + 50% AP
                foreach (StatRatio ratio in _ratios)
                {
                    damageTotal += stats.GetStat(ratio.Stat) * ratio.Percent / 100f;
                }
            }

            return damageTotal;
        }
    }

}
