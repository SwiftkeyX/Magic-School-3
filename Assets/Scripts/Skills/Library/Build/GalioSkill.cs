using System.Collections.Generic;
using MagicSchool.Contracts;

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
                        recipient:       EffectRecipientEnum.Self,
                        totalHealAmount: 200f,
                        duration:        BraceDuration,
                        cadence:         new Cadence(interval: TickInterval, duration: BraceDuration)),

                    new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.Self,
                        modifier: new CustomModifier(
                            duration:  BraceDuration,
                            modifiers: new ModifierSpec(
                                modifier:    ModifierEnum.DamageReduction,
                                scalingType: ScalingEnum.Flat,
                                amount:      25f)),
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
                        recipient:    EffectRecipientEnum.EnemiesInArea,
                        damageAmount: 120f),
                });

            return new SkillStep(
                trigger: TriggerEnum.OnExpired,
                actionGroups: new List<SkillActionGroup> { slam });
        }
    }
}
