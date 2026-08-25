using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Quatre: a piercing shot at the current target that damages everyone standing in its path.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Quatre.asset. Like Roland's, that asset predates the
    /// current schema and still carries a _size/shape pair - Quatre's shape reference was already
    /// broken (rid -2, a managed reference Unity cannot resolve at all). Not carried over.
    /// </summary>
    internal static class QuatreSkill
    {
        private const float DamageRatio = 744f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillActionGroup shot = ActionGroup(registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.PiercingProjectile,
                target: AimTargetEnum.Current,
                Damage(EffectRecipientEnum.EnemiesInPath, (StatEnum.Atk, DamageRatio))
            );

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Step(trigger: TriggerEnum.OnCast, groups: shot) });
        }
    }
}
