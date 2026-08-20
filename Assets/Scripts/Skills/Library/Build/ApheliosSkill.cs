using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    internal class ApheliosSkill
    {
        private const float ExplodeDmg = 240f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Shoot(registry), Explode(registry) });
        }

        private static SkillStep Shoot(TemplateActionRegistrySO registry)
        {
            SkillActionGroup shoot = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.HomingProjectile,
                target: AimTargetEnum.ClusteredCircle
            );

            return Step(trigger: TriggerEnum.OnCast, groups: shoot);
        }

        private static SkillStep Explode(TemplateActionRegistrySO registry)
        {
            SkillActionGroup explode = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.WhereProjectileHit,
                action: TemplateActionEnum.CircleAOE2Hex,
                target: AimTargetEnum.WhereProjectileHit,

                Damage(
                    recipient: EffectRecipientEnum.EnemiesInArea,
                    ratios: (StatEnum.Atk, ExplodeDmg)
                )
            );

            return Step(trigger: TriggerEnum.OnHit, groups: explode);
        }
    }
}