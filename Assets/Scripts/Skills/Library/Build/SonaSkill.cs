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
    public static class SonaSkill
    {
        private const float DamageRatio = 170f;   // sheet: 170/255/420% AP

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Wave(registry) });
        }

        // FIXLATER: two parts of the sheet row are not expressible yet.
        // 1) Scaling Type "Falloff per hit", -33% per enemy passed through. Nothing carries a
        //    per-hit decay - Jhin's row wants the same thing.
        // 2) "Allies in path are buffed instead" - +20/25/35% Attack Speed, Permanent.
        //    EffectRecipientEnum has no allies member, only Self/EnemiesInArea/EnemiesInPath/
        //    SameToAimTarget. And the buff is a share of the ALLY's attack speed, while a ratio
        //    resolves against the caster, so AlliesInPath alone would not be enough.
        private static SkillStep Wave(TemplateActionRegistrySO registry)
        {
            SkillActionGroup wave = ActionGroup(
                registry: registry,
                source:   ActionSourceEnum.Self,
                action:   TemplateActionEnum.PiercingProjectile,
                target:   AimTargetEnum.ClusteredInLine,   // A/B: swap to AimTargetEnum.Clustered for the old, radial pick
                
                // sheet: Sona is AP
                Damage(
                    recipient: EffectRecipientEnum.EnemiesInPath,
                    ratios:    (StatEnum.MG, DamageRatio))
            );

            return Step(trigger: TriggerEnum.OnCast, groups: wave);
        }
    }
}
