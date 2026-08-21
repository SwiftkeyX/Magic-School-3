using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    internal class AkshanSkill
    {
        private const 
        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Shoot(registry) });
        }

        private static SkillStep Shoot(TemplateActionRegistrySO registry)
        {
            ProjectileTuning tune = TuneProjectile(castTime: 0f);

            SkillActionGroup shoot = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.FireTimingRunnerHomingProjectile,
                target: AimTargetEnum.Furthest,
                tuning: TuneFireTimingRunner(6, FireTimingModeEnum.Sequence, 0.1f, tune),

                Damage(
                    recipient: EffectRecipientEnum.SameToAimTarget,
                    ratios: (StatEnum.Atk, 125f)
                )
            );

            return Step(trigger: TriggerEnum.OnCast, groups: shoot);
        }

    }
}