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
        private const float DebuffDuration = 5f;
        private const float ArmorShred = 10f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            var damage = new AttackSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
                        damageRatios: new List<StatRatio> { (StatEnum.Atk, Damage) });

            var debuffArmor = new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.SameToAimTarget,
                        modifier: new CustomModifier(
                            duration: DebuffDuration,
                            modifiers: new List<ModifierSpec>
                            {
                                new ModifierSpec(
                                    modifier:    ModifierEnum.DefendShred,
                                    scalingType: ScalingEnum.Percentage,
                                    amount:      ArmorShred),
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
