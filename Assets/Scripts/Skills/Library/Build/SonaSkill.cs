using System.Collections.Generic;
using MagicSchool.Contracts;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Sona: a wave through the clustered enemy, damaging everyone it passes through.
    ///
    /// Sheet (Hero set 9): "Send a wave at the clustered enemy; damage falls off per enemy hit.
    /// Allies hit are buffed instead." Two of the three parts cannot be said yet - see FIXLATER.
    /// </summary>
    internal static class SonaSkill
    {
        private const float DamageRatio = 170f;   // sheet: 170/255/420% AP
        private const float ASbuff = 25f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Wave(registry) });
        }

        private static SkillStep Wave(TemplateActionRegistrySO registry)
        {
            ICustomModifier ASBuff = Bundle(
                duration: -1f,
                Buff(
                    modifier: ModifierEnum.AttackSpeed,
                    source:   ScalingSourceEnum.Recipient,
                    ratios:   (StatEnum.AttackSpeed, ASbuff))
            );

            SkillActionGroup wave = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.PiercingProjectile,
                target: AimTargetEnum.ClusteredInLine,   // A/B: swap to AimTargetEnum.Clustered for the old, radial pick

                // deal dmg to enemy & give buff to ally 
                Damage(
                    recipient: EffectRecipientEnum.EnemiesInPath,
                    ratios: (StatEnum.MG, DamageRatio)),
                Apply(
                    recipient: EffectRecipientEnum.AlliesInPath,
                    modifier: ASBuff
                )
            );

            return Step(trigger: TriggerEnum.OnCast, groups: wave);
        }
    }
}
