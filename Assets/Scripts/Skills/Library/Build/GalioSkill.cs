using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;
using MagicSchool.Modifiers;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Galio: braces himself - bonus health and healing over a couple of seconds, plus damage
    /// reduction - and when that wears off, slams the ground for damage around him.
    ///
    /// Two steps: the second one is triggered by the first expiring, not by anything the hero does.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Galio.asset.
    /// </summary>
    public static class GalioSkill
    {
        private const float BraceDuration = 2f;
        private const float TickInterval = 0.5f;

        // percentages of the caster's stat, the way the sheet writes them
        private const float HealAmount = 200f;
        private const float SlamDamage = 120f;
        private const float DamageReductionPercent = 25f;   // sheet: 25/25/35%

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Idol of Durand",
                activeSteps: new List<SkillStep> { Brace(registry), Slam(registry) });
        }

        // the cast itself - everything here lands on Galio
        private static SkillStep Brace(TemplateActionRegistrySO registry)
        {
            SkillActionGroup brace = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.CastGalioVariant),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new HealSkillEffect(
                        recipient: EffectRecipientEnum.Self,
                        scaling: new Scaling(ScalingEnum.Percentage, new List<StatRatio> { (StatEnum.MG, HealAmount) }),   // sheet: Galio heals off AP
                        duration:  BraceDuration,
                        cadence:   new Cadence(interval: TickInterval, duration: BraceDuration)),

                    new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.Self,
                        modifier: new CustomModifier(
                            duration:  BraceDuration,
                            modifiers: new List<IModifier>
                            {
                                // sheet: a flat 25/25/35% - derived from nothing, so StatEnum.None
                                new StatModifier(
                                    modifier:    ModifierEnum.DamageReduction,
                                    scaling:     new Scaling(ScalingEnum.Percentage, new List<StatRatio> { (StatEnum.None, DamageReductionPercent) })),
                            }),
                        amplifier: 0.3f),
                });

            return new SkillStep(
                trigger: TriggerEnum.OnCast,
                actionGroups: new List<SkillActionGroup> { brace });
        }

        // fired when the brace above expires
        private static SkillStep Slam(TemplateActionRegistrySO registry)
        {
            SkillActionGroup slam = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.CircleAOE),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new AttackSkillEffect(
                        recipient: EffectRecipientEnum.EnemiesInArea,
                        scaling:    new Scaling(ScalingEnum.Percentage, new List<StatRatio> { (StatEnum.MG, SlamDamage) })),   // sheet: Galio is AP
                });

            return new SkillStep(
                trigger: TriggerEnum.OnExpired,
                actionGroups: new List<SkillActionGroup> { slam });
        }
    }
}
