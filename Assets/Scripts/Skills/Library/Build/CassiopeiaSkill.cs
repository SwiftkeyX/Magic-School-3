using System.Collections.Generic;
using Codice.CM.Common;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;
using MagicSchool.Modifiers;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Cassiopeia: a homing projectile at the furthest enemy, which hits hard and wounds whoever it
    /// lands on.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Cassiopeia.asset.
    /// </summary>
    public static class CassiopeiaSkill
    {
        private const float Damage = 1000f;
        private const float WoundDuration = 5f;
        private const float WoundedAmplifier = 0.3f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            var damage = new AttackSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
                        damageRatios: new List<StatRatio> { (StatEnum.MG, Damage) });

            var amplifierCondition = new HasStatusCondition(
                    subject: ConditionSubjectEnum.Caster,
                    status: ModifierEnum.Transformed,
                    wantPresent: true);

            var wounded = new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
                        modifier: new CustomModifier(
                            duration: WoundDuration,
                            modifiers: new List<IModifier>
                            {
                                new StatusModifier(ModifierEnum.Wound),
                            }),
                        conditions: new List<SkillCondition> { amplifierCondition },
                        amplifier: WoundedAmplifier);

            SkillActionGroup shootProjectile = new SkillActionGroup(
                // homing projectile to furthest enemy
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.HomingProjectile),
                target: AimTargetEnum.Furthest,
                effects: new List<SkillEffect>
                {
                    damage,
                    wounded,
                });

            return new SkillDefinition(
                skillName: "Twin Fang",
                activeSteps: new List<SkillStep>
                {
                    new SkillStep(
                        trigger: TriggerEnum.OnCast,
                        actionGroups: new List<SkillActionGroup> { shootProjectile }),
                });
        }
    }
}
