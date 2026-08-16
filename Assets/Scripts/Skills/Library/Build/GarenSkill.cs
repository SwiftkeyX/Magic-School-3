using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;

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
    public static class GarenSkill
    {
        private const float DamagePerTick = 80f;
        private const float TickInterval = 0.5f;
        private const float Duration = 4f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillActionGroup spin = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.ZoneAOEGarenVariant),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new AttackSkillEffect(
                        recipient: EffectRecipientEnum.EnemiesInArea,
                        scaling:    new Scaling(ScalingEnum.Percentage, new List<StatRatio> { (StatEnum.Atk, DamagePerTick) }),   // sheet: Garen is AD
                        cadence:   new Cadence(interval: TickInterval, duration: Duration)),
                });

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep>
                {
                    new SkillStep(
                        trigger: TriggerEnum.OnCast,
                        actionGroups: new List<SkillActionGroup> { spin }),
                });
        }
    }
}
