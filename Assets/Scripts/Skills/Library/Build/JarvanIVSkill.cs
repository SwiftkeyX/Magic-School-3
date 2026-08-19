using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    internal static class JarvanIVSkill
    {
        private const float StunDuration = 2f;
        private const float LandingDamage = 200f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Jump(registry), Landing(registry) });
        }

        private static SkillStep Jump(TemplateActionRegistrySO registry)
        {
            SkillActionGroup jump = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.Move,
                target: AimTargetEnum.ClusteredCircle
            );

            return Step(trigger: TriggerEnum.OnCast, groups: jump);
        }

        private static SkillStep Landing(TemplateActionRegistrySO registry)
        {
            ICustomModifier stun = Bundle(
                duration: StunDuration,
                modifiers: (
                    Status(ModifierEnum.Stun)
                )
            );

            SkillActionGroup landing = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.CircleAOEJarvanIvVariant,
                target: AimTargetEnum.Self,

                Damage(
                    recipient: EffectRecipientEnum.EnemiesInArea,
                    ratios: (StatEnum.MG, LandingDamage)
                ),
                Apply(
                    recipient: EffectRecipientEnum.EnemiesInArea,
                    modifier: stun
                )
            );

            return Step(TriggerEnum.OnExpired, landing);
        }
    }
}