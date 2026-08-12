using System.Collections.Generic;

namespace MagicSchool
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
                    new AttackSkillEffect(EffectRecipientEnum.SameToAimTarget, Damage),

                    new ModifierSkillEffect(EffectRecipientEnum.SameToAimTarget,
                        new List<ModifierSpec> { new ModifierSpec(ModifierEnum.Wound, 0f, WoundDuration) },
                        amplifier: 0.3f),
                });

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep>
                {
                    new SkillStep(TriggerEnum.OnCast, new List<SkillActionGroup> { cast }),
                });
        }
    }
}
