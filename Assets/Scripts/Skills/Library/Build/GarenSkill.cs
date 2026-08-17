using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Garen: a zone around himself that keeps damaging whoever stands in it, ticking twice a second
    /// for four seconds.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Garen.asset. That asset predates the current schema - it
    /// still stores a _size/shape pair pointing at a Circle class that no longer exists, which Unity
    /// has been dropping on load for a while. Not carried over; git history has it if it is ever
    /// wanted back.
    /// </summary>
    internal static class GarenSkill
    {
        private const float DamagePerTick = 80f;
        private const float TickInterval = 0.5f;
        private const float Duration = 4f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillActionGroup spin = ActionGroup(
                registry: registry,
                source:   ActionSourceEnum.Self,
                action:   TemplateActionEnum.ZoneAOEGarenVariant,
                target:   AimTargetEnum.Self,
                
                DamageOverTime(
                    recipient: EffectRecipientEnum.EnemiesInArea,
                    interval:  TickInterval,
                    duration:  Duration,
                    ratios:    (StatEnum.Atk, DamagePerTick))
            );

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Step(trigger: TriggerEnum.OnCast, groups: spin) });
        }
    }
}
