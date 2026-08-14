using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    public static class WarwickSkill
    {
        // buff
        private const float AttackSpeedBuff = 1f;  
        private const float BuffDuration = 2.5f;

        // stun
        private const float StunDuration = 2.5f;

        // heal on aa
        private const float HealOnAA = 30f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillDefinition skill = new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Cast(registry), OnCastExpired(registry) },
                passiveSteps: new List<SkillStep> { OnAttack(registry) });

            return skill;
        }

        // ============================== active: the transform ==============================
        private static SkillStep Cast(TemplateActionRegistrySO registry)
        {
            // increase as + 100%
            List<ModifierSpec> modifiers = new List<ModifierSpec>
            {
                new ModifierSpec(ModifierEnum.AttackSpeed, AttackSpeedBuff, BuffDuration),
            };

            // cast buff
            SkillActionGroup cast = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.Cast),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new ModifierSkillEffect(EffectRecipientEnum.Self, modifiers),
                });

            // trigger
            TriggerEnum trigger = TriggerEnum.OnCast;

            return new SkillStep(trigger, new List<SkillActionGroup> { cast });
        }

        private static SkillStep OnCastExpired(TemplateActionRegistrySO registry)
        {
            // stun
            List<ModifierSpec> stun = new List<ModifierSpec>
            {
                new ModifierSpec(ModifierEnum.Stun, 0f, StunDuration),
            };

            // aoe on self 
            SkillActionGroup AOE = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.CircleAOE),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new ModifierSkillEffect(EffectRecipientEnum.EnemiesInArea, stun),
                });

            // trigger
            TriggerEnum trigger = TriggerEnum.OnExpired;

            return new SkillStep(trigger, new List<SkillActionGroup> { AOE });
        }

        // ============================== passive: the combo ==============================
        private static SkillStep OnAttack(TemplateActionRegistrySO registry)
        {
            // cast
            SkillActionGroup cast = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.Cast),
                target: AimTargetEnum.Self,
                effects: new List<SkillEffect>
                {
                    new HealSkillEffect(EffectRecipientEnum.Self, HealOnAA),
                }
            );

            return new SkillStep(TriggerEnum.OnAttack, new List<SkillActionGroup> { cast });
        }
    }
}