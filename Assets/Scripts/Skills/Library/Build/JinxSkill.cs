using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;
using static MagicSchool.Skills.SkillFactory;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Jinx: fires 5 rockets in sequence, each at a random enemy within 2 hexes of her current
    /// target - no repeat until every enemy in that pool has been hit once.
    ///
    /// Sheet (Hero set 9): "Fire 5 rockets at random enemies within 2 hexes of the current target.
    /// Each rocket deals 150/155/160% Attack Damage + 15/20/35% Ability Power physical damage."
    /// Star-level scaling (the /155/160 and /20/35 tiers) isn't implemented yet - same as Sona's
    /// DamageRatio, this takes the 1-star baseline only.
    /// </summary>
    internal class JinxSkill
    {
        private const float ADDamagePerShot = 150f;   // sheet: 150/155/160% AD
        private const float APDamagePerShot = 15f;    // sheet: 15/20/35% AP
        private const int ShotCount = 5;
        private const float IntervalBetweenShot = 0.1f;
        private const int RandomPoolRadius = 2;        // sheet: "random enemies within 2 hexes of the current target"

        public static SkillDefinition Build(TemplateActionRegistrySO registry)
        {
            return new SkillDefinition(
                skillName: "Skill",
                activeSteps: new List<SkillStep> { Shoot(registry) });
        }

        private static SkillStep Shoot(TemplateActionRegistrySO registry)
        {
            ProjectileTuning tune = TuneProjectile(castTime: 0f);

            SkillActionGroup shoot = ActionGroup(
                registry: registry,
                source: ActionSourceEnum.Self,
                action: TemplateActionEnum.FireTimingRunnerHomingProjectile,
                target: AimTargetEnum.Random,
                tuning: TuneFireTimingRunner(ShotCount, FireTimingModeEnum.Sequence, IntervalBetweenShot, tune,
                                             randomPoolRadius: RandomPoolRadius),

                Damage(
                    recipient: EffectRecipientEnum.SameToAimTarget,
                    (StatEnum.Atk, ADDamagePerShot), (StatEnum.MG, APDamagePerShot)
                )
            );

            return Step(trigger: TriggerEnum.OnCast, groups: shoot);
        }
    }
}
