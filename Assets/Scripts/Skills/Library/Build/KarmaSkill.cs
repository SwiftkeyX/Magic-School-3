using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Karma: a projectile at the current target that does nothing itself - when it lands, an AOE
    /// goes off where it hit.
    ///
    /// Two steps: the second is triggered by the first hitting something, and it spawns and aims at
    /// the projectile's landing point rather than at the hero or a target.
    ///
    /// Ported from Assets/Data/Heroes/Skills/Karma.asset.
    /// </summary>
    public static class KarmaSkill
    {
        private const float ExplosionDamage = 200f;

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Blinding Dart",
                activeSteps: new List<SkillStep> { Dart(registry), Explosion(registry) });
        }

        // carries no effects of its own - it only exists to land somewhere
        private static SkillStep Dart(TemplateActionRegistrySO registry)
        {
            SkillActionGroup dart = new SkillActionGroup(
                source: ActionSourceEnum.Self,
                templateAction: registry.Get(TemplateActionEnum.FirstHitProjectile),
                target: AimTargetEnum.Current);

            return new SkillStep(
                trigger: TriggerEnum.OnCast,
                actionGroups: new List<SkillActionGroup> { dart });
        }

        private static SkillStep Explosion(TemplateActionRegistrySO registry)
        {
            SkillActionGroup blast = new SkillActionGroup(
                source: ActionSourceEnum.WhereProjectileHit,
                templateAction: registry.Get(TemplateActionEnum.CircleAOE),
                target: AimTargetEnum.WhereProjectileHit,
                effects: new List<SkillEffect>
                {
                    new AttackSkillEffect(
                        recipient: EffectRecipientEnum.EnemiesInArea,
                        damageRatios:    new List<StatRatio> { (StatEnum.MG, ExplosionDamage) }),   // sheet: Karma is AP
                });

            return new SkillStep(
                trigger: TriggerEnum.OnHit,
                actionGroups: new List<SkillActionGroup> { blast });
        }
    }
}
