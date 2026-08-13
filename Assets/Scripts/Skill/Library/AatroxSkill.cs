using System.Collections.Generic;
using MagicSchool.Contracts;

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
            List<ModifierSpec> modifiers = new List<ModifierSpec>
            {
                new ModifierSpec(ModifierEnum.Omnivamp, 10f, TransformDuration),
                new ModifierSpec(ModifierEnum.Attack, 80f, TransformDuration),
                new ModifierSpec(ModifierEnum.Transformed, 0f, TransformDuration),

                // no mana while transformed, and the combo below stands in for the auto attack
                new ModifierSpec(ModifierEnum.ManaBlocked, 0f, TransformDuration),
                new ModifierSpec(ModifierEnum.AutoAttackWasReplaced, 0f, TransformDuration),
            };

            SkillActionGroup cast = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.Cast),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new ModifierSkillEffect(EffectRecipientEnum.Self, modifiers),
                });

            return new SkillStep(TriggerEnum.OnCast, new List<SkillActionGroup> { cast });
        }

        // ============================== passive: the combo ==============================
        private static SkillStep Combo(TemplateActionRegistrySO registry, ComboTracker combo)
        {
            List<SkillActionGroup> beats = new List<SkillActionGroup>
            {
                Beat(registry, combo, TemplateActionEnum.BoxAOETip,      beat: 1, damage: 200f),
                Beat(registry, combo, TemplateActionEnum.TriangleAOETip, beat: 2, damage: 300f),
                Beat(registry, combo, TemplateActionEnum.CircleAOETip,   beat: 3, damage: 400f),
            };

            return new SkillStep(TriggerEnum.OnAttack, beats);
        }

        // One beat of the combo: play this shape when transformed and the combo is on this count.
        private static SkillActionGroup Beat(TemplateActionRegistrySO registry, ComboTracker combo,
                                             TemplateActionEnum action, int beat, float damage)
        {
            List<SkillCondition> conditions = new List<SkillCondition>
            {
                new HasStatusCondition(ConditionSubjectEnum.Caster, ModifierEnum.Transformed, wantPresent: true),
                new NumberCondition(ConditionSubjectEnum.Caster, combo, matchBeat: beat),
            };

            return new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(action),
                target: AimTargetEnum.Current,
                conditions: conditions,
                effects: new List<SkillEffect>
                {
                    new AttackSkillEffect(EffectRecipientEnum.EnemiesInArea, damage),
                });
        }
    }
}
