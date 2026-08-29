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
            => GetTotals(stats, null).Additive;

        // Split the terms against the stat the total is landing on.
        // A term reading that same stat cannot be resolved here - it is a share of a number this
        // scaling never sees, the finished stat - so it is handed up as a percentage and applied
        // once everything additive has settled.
        public ScalingTotals GetTotals(IHeroStats stats, StatEnum? landingOn)
        {
            // guard
            if (_ratios == null || _ratios.Count == 0) return new ScalingTotals(0f, 0f);

            float additive = 0f;
            float multiplierPercent = 0f;

            foreach (StatRatio ratio in _ratios)
            {
                // flat scaling - the number stands on its own
                if (!ratio.Stat.HasValue)
                {
                    additive += ratio.Amount;
                    continue;
                }

                // derived from the same stat it lands on - a multiplier, e.g. Fang's "+100% of his
                // own attack speed". Reads nothing now; the resolver applies it last.
                if (landingOn.HasValue && ratio.Stat.Value == landingOn.Value)
                {
                    multiplierPercent += ratio.Amount;
                    continue;
                }

                // derived from another stat - an amount to add, e.g. "+ATK worth 80% of AS"
                // FLAGGING: another class type check, let leave it for now.
                // Somehow the effect always need stat from hero, maybe we'll have to include IHeroStats into ICombatant?
                // guard - only this branch needs a hero to read off, so it is checked here rather
                // than up front: the other two are worth paying out without one.
                if (stats == null)
                {
                    UnityEngine.Debug.LogError($"[Scaling] scales off the {_source}'s stats but was asked without any. " +
                                               "SkillDefinition.Init() has to reach every effect it holds.");
                    continue;
                }

                additive += stats.GetStat(ratio.Stat.Value) * ratio.Amount / 100f;
            }

            return new ScalingTotals(additive, multiplierPercent);
        }
    }

}
