

using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    internal static class SionSkill
    {
        private const float KnockedUpDuration = 2f;
        private const float CollideDamage = 200f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Move(registry), Charge(registry) });
        }

        private static SkillStep Move(TemplateActionRegistrySO registry)
        {
            SkillActionGroup move = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.Move,
                // FIXLATER: He actually target more like clusterd laser.
                target: AimTargetEnum.ClusteredLaser
            );

            return Step(trigger: TriggerEnum.OnCast, groups: move);
        }

        private static SkillStep Charge(TemplateActionRegistrySO registry)
        {
            ICustomModifier knockup = Bundle(
                duration: KnockedUpDuration,
                modifiers: (
                    Status(ModifierEnum.Stun)
                )
            );

            // Make sure the AOE's lifetime is long enough until Move dies
            SkillActionGroup charge = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.CircleAOESticky,  
                target: AimTargetEnum.Self,

                Damage(
                    recipient: EffectRecipientEnum.EnemiesInPath,
                    ratios: (StatEnum.MG, CollideDamage)
                ),
                Apply(
                    recipient: EffectRecipientEnum.EnemiesInPath,
                    modifier: knockup
                )
            );

            // FLAGGING: new trigger, start immediately at OnCast
            return Step(TriggerEnum.OnCastStart, charge);
        }
    }
}