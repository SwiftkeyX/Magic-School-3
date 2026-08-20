

using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    internal static class SionSkill
    {
        private const int ChargeRange = 4;              // hexes he can cross
        // the hitbox that rides him is HalfCircleAOESticky, 1.25 across - the charge is aimed by
        // what that will sweep, so the two numbers are one number
        private const float HitboxHalfWidth = 1.25f;
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
                tuning: Tune(range: ChargeRange, laneHalfWidth: HitboxHalfWidth),
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
                action: TemplateActionEnum.HalfCircleAOESticky,  
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