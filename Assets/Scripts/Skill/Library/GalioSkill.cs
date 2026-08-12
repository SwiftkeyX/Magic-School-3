using System.Collections.Generic;

namespace MagicSchool
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
        private const float TickWindow = 3f;

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
                templateAction: registry.Get(TemplateActionEnum.Cast),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new ModifierSkillEffect(EffectRecipientEnum.Self,
                        new List<ModifierSpec> { new ModifierSpec(ModifierEnum.BonusHP, 150f, BraceDuration) },
                        cadence: new Cadence(TickInterval, TickWindow),
                        amplifier: 0.3f),

                    new HealSkillEffect(EffectRecipientEnum.Self, totalHealAmount: 200f, duration: BraceDuration,
                        cadence: new Cadence(TickInterval, TickWindow)),

                    new ModifierSkillEffect(EffectRecipientEnum.Self,
                        new List<ModifierSpec> { new ModifierSpec(ModifierEnum.DamageReduction, 25f, BraceDuration) },
                        amplifier: 0.3f),
                });

            return new SkillStep(TriggerEnum.OnCast, new List<SkillActionGroup> { brace });
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
                    new AttackSkillEffect(EffectRecipientEnum.EnemiesInArea, 120f),
                });

            return new SkillStep(TriggerEnum.OnExpired, new List<SkillActionGroup> { slam });
        }
    }
}
