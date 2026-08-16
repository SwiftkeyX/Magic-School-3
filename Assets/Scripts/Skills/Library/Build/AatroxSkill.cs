using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;
using MagicSchool.Modifiers;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Aatrox: casting transforms him, and while transformed his auto attack is replaced by a three
    /// beat combo - box, then triangle, then circle - each hitting harder than the last.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Aatrox.asset, which stays as the reference until this is
    /// verified against it.
    /// </summary>
    public static class AatroxSkill
    {
        private const int ComboLength = 3;

        // how long the transform and everything that comes with it lasts
        private const float OmnivampFromAP = 10f;   // sheet: 10% AP omnivamp
        private const float AttackFromAS = 80f;   // sheet: 80% of bonus AS, converted to AD
        private const float TransformDuration = 10f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            // one count for the whole combo, handed to each beat below. Shared deliberately and
            // visibly, rather than a count inside each beat that nothing keeps in agreement.
            ComboTracker combo = new ComboTracker(ComboLength);

            SkillDefinition skill = new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Transform(registry) },
                passiveSteps: new List<SkillStep> { Combo(registry, combo) });

            // moves the combo on when he attacks - not when a condition is asked
            skill.Triggered += combo.Count;

            return skill;
        }

        // ============================== active: the transform ==============================
        private static SkillStep Transform(TemplateActionRegistrySO registry)
        {
            // one group, one timer - the whole transform ends on the same tick
            CustomModifier WorldEnderBuff = new CustomModifier(TransformDuration, new List<IModifier>
            {
                // sheet: 10% Ability Power Omnivamp
                new StatModifier(
                    modifier:    ModifierEnum.Omnivamp,
                    scalingType: ScalingEnum.Percentage,
                    ratios:      new List<StatRatio> { (StatEnum.MG, OmnivampFromAP) }),

                // FIXLATER: the sheet says 80% of *bonus* AS, and that the bonus is consumed.
                // IHeroStats only exposes the final stat, so this reads total AS for now.
                new StatModifier(
                    modifier:    ModifierEnum.Attack,
                    scalingType: ScalingEnum.Percentage,
                    ratios:      new List<StatRatio> { (StatEnum.AttackSpeed, AttackFromAS) }),

                new StatusModifier(ModifierEnum.Transformed),

                // no mana while transformed, and the combo below stands in for the auto attack
                new StatusModifier(ModifierEnum.ManaBlocked),

                new StatusModifier(ModifierEnum.AutoAttackWasReplaced),
            });

            SkillActionGroup cast = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.Cast),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new ModifierSkillEffect(
                        recipient: EffectRecipientEnum.Self,
                        modifier:  WorldEnderBuff),
                });

            return new SkillStep(
                trigger: TriggerEnum.OnCast,
                actionGroups: new List<SkillActionGroup> { cast });
        }

        // ============================== passive: the combo ==============================
        private static SkillStep Combo(TemplateActionRegistrySO registry, ComboTracker combo)
        {
            List<SkillActionGroup> beats = new List<SkillActionGroup>
            {
                Beat(registry: registry, combo: combo, action: TemplateActionEnum.BoxAOETip,      beat: 1, damage: 200f),
                Beat(registry: registry, combo: combo, action: TemplateActionEnum.TriangleAOETip, beat: 2, damage: 300f),
                Beat(registry: registry, combo: combo, action: TemplateActionEnum.CircleAOETip,   beat: 3, damage: 400f),
            };

            return new SkillStep(
                trigger: TriggerEnum.OnAttack,
                actionGroups: beats);
        }

        // One beat of the combo: play this shape when transformed and the combo is on this count.
        private static SkillActionGroup Beat(TemplateActionRegistrySO registry, ComboTracker combo,
                                             TemplateActionEnum action, int beat, float damage)
        {
            List<SkillCondition> conditions = new List<SkillCondition>
            {
                new HasStatusCondition(
                    subject:     ConditionSubjectEnum.Caster,
                    status:      ModifierEnum.Transformed,
                    wantPresent: true),

                new NumberCondition(
                    subject:   ConditionSubjectEnum.Caster,
                    combo:     combo,
                    matchBeat: beat),
            };

            return new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(action),
                target: AimTargetEnum.Current,
                conditions: conditions,
                effects: new List<SkillEffect>
                {
                    new AttackSkillEffect(
                        recipient: EffectRecipientEnum.EnemiesInArea,
                        scaling:    new Scaling(ScalingEnum.Percentage, new List<StatRatio> { (StatEnum.Atk, damage) })),   // sheet: Aatrox is AD
                });
        }
    }
}
