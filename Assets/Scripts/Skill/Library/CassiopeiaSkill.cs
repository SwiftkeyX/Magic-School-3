using System.Collections.Generic;

namespace MagicSchool
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

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillActionGroup cast = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.HomingProjectile),
                target: AimTargetEnum.Furthest,
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
