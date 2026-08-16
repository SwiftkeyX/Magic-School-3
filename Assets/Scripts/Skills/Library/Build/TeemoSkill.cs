using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Teemo: a homing shot at the furthest enemy which does nothing on its own - where it lands, a
    /// patch goes down that wounds and then keeps damaging whoever stands in it.
    ///
    /// Same two step shape as Karma: the second step is triggered by the first hitting something,
    /// and spawns at the landing point.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Teemo.asset. Its skill name reads "Blinding Dart", the
    /// same as Karma's - left as it was rather than guessed at.
    /// </summary>
    public static class TeemoSkill
    {
        private const float WoundDuration = 3f;
        private const float PoisonDamage = 2000f;
        private const float PoisonInterval = 0.1f;
        private const float PoisonDuration = 3f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Blinding Dart",
                activeSteps: new List<SkillStep> { Dart(registry), Patch(registry) });
        }

        private static SkillStep Dart(TemplateActionRegistrySO registry)
        {
            SkillActionGroup dart = ActionGroup(
                registry: registry,
                source:   ActionSourceEnum.Self,
                action:   TemplateActionEnum.HomingProjectile,
                target:   AimTargetEnum.Furthest
            );

            return Step(trigger: TriggerEnum.OnCast, groups: dart);
        }

        private static SkillStep Patch(TemplateActionRegistrySO registry)
        {
            SkillActionGroup patch = ActionGroup(
                registry: registry,
                source:   ActionSourceEnum.WhereProjectileHit,
                action:   TemplateActionEnum.CircleAOE,
                target:   AimTargetEnum.WhereProjectileHit,

                Apply(
                    recipient: EffectRecipientEnum.EnemiesInArea,
                    modifier:  Bundle(
                        duration:  WoundDuration,
                        modifiers: Status(ModifierEnum.Wound)),
                    amplifier: 0.3f),

                // sheet: Teemo is AP
                DamageOverTime(
                    recipient: EffectRecipientEnum.EnemiesInArea,
                    interval:  PoisonInterval,
                    duration:  PoisonDuration,
                    ratios:    (StatEnum.MG, PoisonDamage))
            );

            return Step(trigger: TriggerEnum.OnHit, groups: patch);
        }
    }
}
