using System.Collections.Generic;
using Codice.CM.Common;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Cassiopeia: a homing projectile at the furthest enemy, which hits hard and wounds whoever it
    /// lands on.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Cassiopeia.asset.
    /// </summary>
    internal static class CassiopeiaSkill
    {
        private const float DamageRatio = 1000f;
        private const float WoundDuration = 5f;
        private const float WoundedAmplifier = 0.3f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            List<SkillCondition> amplifierCondition = new List<SkillCondition>
            {
                new HasStatusCondition(
                    subject:     ConditionSubjectEnum.Caster,
                    status:      ModifierEnum.Transformed,
                    wantPresent: true),
            };

            // homing projectile to furthest enemy
            SkillActionGroup shootProjectile = ActionGroup(
                registry: registry,
                source:   ActionSourceEnum.Self,
                action:   TemplateActionEnum.HomingProjectile,
                target:   AimTargetEnum.Furthest,

                Damage(
                    recipient: EffectRecipientEnum.SameToAimTarget,
                    ratios:    (StatEnum.MG, DamageRatio)),

                ApplyWhen(
                    recipient:  EffectRecipientEnum.SameToAimTarget,
                    modifier:   Bundle(
                        duration:  WoundDuration,
                        modifiers: Status(ModifierEnum.Wound)),
                    conditions: amplifierCondition,
                    amplifier:  WoundedAmplifier)
            );

            return new SkillDefinition(
                skillName: "Twin Fang",
                activeSteps: new List<SkillStep> { Step(trigger: TriggerEnum.OnCast, groups: shootProjectile) });
        }
    }
}
