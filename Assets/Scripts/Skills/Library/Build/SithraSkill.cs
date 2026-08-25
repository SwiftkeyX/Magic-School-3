using System.Collections.Generic;
using Codice.CM.Common;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    internal static class SithraSkill
    {
        private const float DamageRatio = 1000f;
        private const float WoundDuration = 5f;
        private const float WoundedAmplifier = 0.3f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            List<SkillCondition> amplifierCondition = new List<SkillCondition>
            {
                new HasStatusCondition(
                    subject:     ConditionSubjectEnum.Recipient,
                    status:      ModifierEnum.Wound,
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
                skillName: "Split Venom",
                activeSteps: new List<SkillStep> { Step(trigger: TriggerEnum.OnCast, groups: shootProjectile) },
                description: "Spits a homing bolt at the furthest enemy. It hits hard and leaves the target wounded, so "
                             + "everything that tries to heal them for the next 5 seconds does less." 
                             + "If target is already wounded, amplified damage by +30%");
        }
    }
}
