using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool
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
                    new AttackSkillEffect(EffectRecipientEnum.EnemiesInPath, Damage),
                });

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep>
                {
                    new SkillStep(TriggerEnum.OnCast, new List<SkillActionGroup> { shot }),
                });
        }
    }
}
