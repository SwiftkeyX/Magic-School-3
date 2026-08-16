using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Samira: a projectile at the current target that only counts its first hit, permanently
    /// shredding the armour of whoever it lands on.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Samira.asset.
    /// </summary>
    public static class SamiraSkill
    {
        private const float Damage = 200f;
        private const float ShredDuration = -1f;
        private const float ShredFromAP = 20f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            var damage = new AttackSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
                        damageRatios: new List<StatRatio> { (StatEnum.Atk, Damage) });

            var debuffArmor = new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
                        modifier: new CustomModifier(
                            duration: ShredDuration,
                            modifiers: new List<ModifierSpec>
                            {
                                new ModifierSpec(
                                    modifier:    ModifierEnum.DefendShred,
                                    scalingType: ScalingEnum.Percentage,
                                    ratios:      new List<StatRatio> { (StatEnum.MG, -ShredFromAP) }),
                            })
                        );

            SkillActionGroup shootProjectile = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.FirstHitProjectile),
                target: AimTargetEnum.Current,
                effects: new List<SkillEffect>
                {
                    damage,
                    debuffArmor,
                });

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep>
                {
                    new SkillStep(
                        trigger: TriggerEnum.OnCast,
                        actionGroups: new List<SkillActionGroup> { shootProjectile }),
                });
        }
    }
}
