using System.Collections.Generic;
using MagicSchool.Contracts;

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
            SkillActionGroup dart = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.HomingProjectile),
                target: AimTargetEnum.Furthest);

            return new SkillStep(
                trigger: TriggerEnum.OnCast,
                actionGroups: new List<SkillActionGroup> { dart });
        }

        private static SkillStep Patch(TemplateActionRegistrySO registry)
        {
            SkillActionGroup patch = new SkillActionGroup(
                source: ActionSourceEnum.WhereProjectileHit,
                templateAction: registry.Get(TemplateActionEnum.CircleAOE),
                target: AimTargetEnum.WhereProjectileHit,
                effects: new List<SkillEffect>
                {
                    new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.EnemiesInArea,
                        modifier: new CustomModifier(
                            duration:  WoundDuration,
                            modifiers: new List<ModifierSpec>
                            {
                                new ModifierSpec(
                                    modifier:    ModifierEnum.Wound,
                                    scalingType: ScalingEnum.Flat,
                                    amount:      0f),
                            }),
                        amplifier: 0.3f),

                    new AttackSkillEffect(
                        recipient: EffectRecipientEnum.EnemiesInArea,
                        damageRatios:    new List<StatRatio> { (StatEnum.MG, PoisonDamage) },   // sheet: Teemo is AP
                        cadence:   new Cadence(interval: PoisonInterval, duration: PoisonDuration)),
                });

            return new SkillStep(
                trigger: TriggerEnum.OnHit,
                actionGroups: new List<SkillActionGroup> { patch });
        }
    }
}
