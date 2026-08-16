using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Samira: a projectile at the current target that only counts its first hit, wounding whoever
    /// it lands on.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Samira.asset.
    /// </summary>
    public static class SamiraSkill
    {
        private const float Damage = 200f;
        private const float WoundDuration = 5f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillActionGroup cast = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.FirstHitProjectile),
                target: AimTargetEnum.Current,
                effects: new List<SkillEffect>
                {
                    new AttackSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
                        damageRatios:    new List<StatRatio> { (StatEnum.Atk, Damage) }),   // sheet: Samira is AD

                    new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
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
                });

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep>
                {
                    new SkillStep(
                        trigger: TriggerEnum.OnCast,
                        actionGroups: new List<SkillActionGroup> { cast }),
                });
        }
    }
}
