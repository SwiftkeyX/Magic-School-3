using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Jhin: a piercing shot at the current target that damages everyone standing in its path.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Jhin.asset. Like Garen's, that asset predates the
    /// current schema and still carries a _size/shape pair - Jhin's shape reference was already
    /// broken (rid -2, a managed reference Unity cannot resolve at all). Not carried over.
    /// </summary>
    public static class JhinSkill
    {
        private const float Damage = 1000f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillActionGroup shot = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.PiercingProjectile),
                target: AimTargetEnum.Current,
                effects: new List<SkillEffect>
                {
                    new AttackSkillEffect(
                        recipient: EffectRecipientEnum.EnemiesInPath,
                        // sheet says "744% AD & AP" - AD only until it is settled whether that is
                        // 744% of each or split between them. The list is what it goes in either way.
                        damageRatios:    new List<StatRatio> { (StatEnum.Atk, Damage) }),
                });

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep>
                {
                    new SkillStep(
                        trigger: TriggerEnum.OnCast,
                        actionGroups: new List<SkillActionGroup> { shot }),
                });
        }
    }
}
