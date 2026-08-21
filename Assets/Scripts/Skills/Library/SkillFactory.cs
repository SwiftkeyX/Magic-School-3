using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;
using MagicSchool.Modifiers;

namespace MagicSchool.Skills
{
    internal static class SkillFactory
    {
        // ================================== damage ==================================
        // e.g. Damage(EnemiesInPath, (StatEnum.Atk, 1000f))  ->  1000% AD to everyone in the path
        public static AttackSkillEffect Damage(EffectRecipientEnum recipient, params StatRatio[] ratios)
            => new AttackSkillEffect(recipient, Scale(ratios));

        // the same, re-applied on a timer - Garen's spin, Teemo's patch
        public static AttackSkillEffect DamageOverTime(EffectRecipientEnum recipient, float interval, float duration,
                                                       params StatRatio[] ratios)
            => new AttackSkillEffect(recipient, Scale(ratios), new Cadence(interval, duration));

        // ================================== heal ==================================
        public static HealSkillEffect Heal(EffectRecipientEnum recipient, params StatRatio[] ratios)
            => new HealSkillEffect(recipient, Scale(ratios));

        public static HealSkillEffect HealOverTime(EffectRecipientEnum recipient, float duration, float interval,
                                                   params StatRatio[] ratios)
            => new HealSkillEffect(recipient, Scale(ratios), duration, new Cadence(interval, duration));

        // ================================== modifiers ==================================
        // apply a group of modifiers on the recipient
        public static ModifierSkillEffect Apply(EffectRecipientEnum recipient, ICustomModifier modifier, float amplifier = 0f)
            => new ModifierSkillEffect(recipient, modifier, amplifier: amplifier);

        // the same, but amplified only when every condition holds - the effect still lands either
        // way, the conditions just scale it. Cassiopeia's wound is applied harder while transformed.
        public static ModifierSkillEffect ApplyWhen(EffectRecipientEnum recipient, ICustomModifier modifier,
                                                    List<SkillCondition> conditions, float amplifier)
            => new ModifierSkillEffect(recipient, modifier, conditions: conditions, amplifier: amplifier);

        // the group of modifiers - everything in it shares one duration. -1f is permanent.
        public static ICustomModifier Bundle(float duration, params IModifier[] modifiers)
            => new CustomModifier(duration, modifiers);

        // a modifier that give stat bonus, e.g. Buff(Attack, (StatEnum.MG, 50f)) is "+Atk 50% of the caster's AP".
        // 1) If have several ratios, it add up.
        // 2) StatEnum.None is a number written straight in rather than derived from any stat.
        // 2.1) Buff(DefendShred, (StatEnum.None, 20f), (StatEnum.MG, 20f)) is "20 flat, plus 20% AP on top".
        public static IModifier Buff(ModifierEnum modifier, params StatRatio[] ratios)
            => new StatModifier(modifier, Scale(ratios));

        // 3) the buff will derived from the caster itself unless it consume "source" parameter.
        // Sona's skill buff ally base on their attack speed by +25% => This mean the skill is derived from ally, not the caster itself.
        // BUT it have another pattern, you should know:
        // Sona's skill (alternative) buff ally base on Sona's AP by +50%AP => This will get different result.
        public static IModifier Buff(ModifierEnum modifier, ScalingSourceEnum source, params StatRatio[] ratios)
            => new StatModifier(modifier, Scale(source, ratios));

        // a modifier that give status 
        // e.g. Wound, Stun, Transformed
        public static IModifier Status(ModifierEnum status)
            => new StatusModifier(status);

        // ================================== ActionGroup ==================================
        // FIXLATER: there's too much duplicate of ActionGroup function here, could we group them into 1?
        // a template action
        public static SkillActionGroup ActionGroup(TemplateActionRegistrySO registry, ActionSourceEnum source,
                                                   TemplateActionEnum action, AimTargetEnum target,
                                                   params SkillEffect[] effects)
            => new SkillActionGroup(source, registry.Get(action), target,
                                    effects: new List<SkillEffect>(effects));

        // the same, with this hero's numbers for the action
        public static SkillActionGroup ActionGroup(TemplateActionRegistrySO registry, ActionSourceEnum source,
                                                   TemplateActionEnum action, AimTargetEnum target,
                                                   Tuning tuning,
                                                   params SkillEffect[] effects)
            => new SkillActionGroup(source, registry.Get(action), target,
                                    effects: new List<SkillEffect>(effects), tuning: tuning);

        // a template action with condition
        // conditin need to be true, in order this template action to play.
        public static SkillActionGroup ActionGroupWhen(TemplateActionRegistrySO registry, ActionSourceEnum source,
                                                       TemplateActionEnum action, AimTargetEnum target,
                                                       List<SkillCondition> conditions,
                                                       params SkillEffect[] effects)
            => new SkillActionGroup(source, registry.Get(action), target,
                                    conditions: conditions, effects: new List<SkillEffect>(effects));

        // the same, with this hero's numbers for the action
        public static SkillActionGroup ActionGroupWhen(TemplateActionRegistrySO registry, ActionSourceEnum source,
                                                       TemplateActionEnum action, AimTargetEnum target,
                                                       List<SkillCondition> conditions, Tuning tuning,
                                                       params SkillEffect[] effects)
            => new SkillActionGroup(source, registry.Get(action), target,
                                    conditions: conditions, effects: new List<SkillEffect>(effects), tuning: tuning);

        // a step inside template action
        // step are working in order, each step will be played if it was trigger.
        public static SkillStep Step(TriggerEnum trigger, params SkillActionGroup[] groups)
            => new SkillStep(trigger, new List<SkillActionGroup>(groups));
        
        // ================================== Tune ==================================
        // tuning a template action
        public static Tuning Tune(float? castTime = null)
            => new Tuning { CastTime = castTime };

        public static AOETuning TuneAOE(float? castTime = null, float? duration = null, float? size = null,
                                        bool? sticky = null, AOEOffsetEnum? offset = null, int? range = null)
            => new AOETuning { CastTime = castTime, Duration = duration, Size = size, Sticky = sticky,
                               Offset = offset, Range = range };

        public static MoveTuning TuneMove(float? castTime = null, int? range = null,
                                          float? duration = null, float? spread = null)
            => new MoveTuning { CastTime = castTime, Range = range, Duration = duration, Spread = spread };

        public static ProjectileTuning TuneProjectile(float? castTime = null, int? range = null,
                                                      float? spread = null, float? size = null)
            => new ProjectileTuning { CastTime = castTime, Range = range, Spread = spread, Size = size };

        public static FireTimingRunnerTuning TuneFireTimingRunner(int count, FireTimingModeEnum mode,
                                                                   float interval = 0f, Tuning innerTuning = null,
                                                                   int? randomPoolRadius = null)
            => new FireTimingRunnerTuning { Count = count, Mode = mode, Interval = interval,
                                            InnerTuning = innerTuning, RandomPoolRadius = randomPoolRadius };

        // How far a blast of this size actually reaches, in world units.
        private const float AuthoredRadius = 0.5f;
        
        // size of the AOE, etc...
        public static float Reach(float size) => size * AuthoredRadius;


        // ================================== scaling ==================================
        public static IScaling Scale(params StatRatio[] ratios)
            => new Scaling(ScalingEnum.Percentage, ratios);

        public static IScaling Scale(ScalingSourceEnum source, params StatRatio[] ratios)
            => new Scaling(ScalingEnum.Percentage, ratios, source);
    }
}
