using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    internal static class VesperSkill
    {
        private const float DamageRatio = 200f;
        private const float ShredDuration = -1f;
        private const float ShredFromAP = 20f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            SkillActionGroup shootProjectile = ActionGroup(
                registry: registry,
                source:   ActionSourceEnum.Self,
                action:   TemplateActionEnum.FirstHitProjectile,
                target:   AimTargetEnum.Current,

                Damage(
                    recipient: EffectRecipientEnum.SameToAimTarget,
                    ratios:    (StatEnum.Atk, DamageRatio)),

                Apply(
                    recipient: EffectRecipientEnum.SameToAimTarget,
                    modifier:  Bundle(
                        duration:  ShredDuration,
                        modifiers: Buff(
                            modifier: ModifierEnum.DefendShred,
                            ratios:   (StatEnum.MG, -ShredFromAP))))
            );

            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Step(trigger: TriggerEnum.OnCast, groups: shootProjectile) },
                description: "Fires a shot at the current target. The first enemy it finds takes the hit and loses armour "
                             + "for the rest of the fight.");
        }
    }
}
