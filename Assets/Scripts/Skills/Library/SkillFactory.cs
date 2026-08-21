using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.StatScaling;
using MagicSchool.Modifiers;

namespace MagicSchool.Skills
{
    /// <summary>
    /// The pieces a hero's skill is assembled from.
    ///
    /// Every hero used to spell out the same construction - a Scaling wrapping a List of StatRatio,
    /// a CustomModifier wrapping a List of IModifier - so any change to those constructors was a
    /// nine file edit. That happened six times while the modifier work was going on, and only four
    /// of the forty-odd builder edits in that stretch were about a hero at all.
    ///
    /// So the construction lives here and the hero files keep what is actually theirs: which stat,
    /// what percent, how long. Add `using static MagicSchool.Skills.SkillFactory;` and they read as
    /// Damage(EnemiesInPath, (StatEnum.Atk, 1000f)).
    ///
    /// This deliberately does NOT wrap SkillActionGroup or SkillStep. Those have not changed once,
    /// so wrapping them would buy nothing and cost a second vocabulary to learn.
    /// </summary>
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

        // tuning a template action
        // e.g. cast time = time hero cast his skill, he can't AA while doing so, default 0.5 sec
        // e.g. range = skill range check, how far can the center of skill go away from the caster
        // e.g. duration = time before the skill expired, usage => 4 sec of AOE doing damage continuously before expired 
        // ...
        public static Tuning Tune(float? castTime = null, int? range = null, float? size = null,
                                  float? duration = null, float? effectRange = null, float? laneHalfWidth = null,
                                  bool? sticky = null)
            => new Tuning
            {
                CastTime = castTime,
                Range = range,
                Size = size,
                Duration = duration,
                EffectRange = effectRange,
                LaneHalfWidth = laneHalfWidth,
                Sticky = sticky,
            };

        // How far a blast of this size actually reaches, in world units.
        private const float AuthoredRadius = 0.5f;
        
        // size of the AOE, etc...
        public static float Reach(float size) => size * AuthoredRadius;

        // a step inside template action
        // step are working in order, each step will be played if it was trigger.
        public static SkillStep Step(TriggerEnum trigger, params SkillActionGroup[] groups)
            => new SkillStep(trigger, new List<SkillActionGroup>(groups));

        // ================================== scaling ==================================
        public static IScaling Scale(params StatRatio[] ratios)
            => new Scaling(ScalingEnum.Percentage, ratios);

        public static IScaling Scale(ScalingSourceEnum source, params StatRatio[] ratios)
            => new Scaling(ScalingEnum.Percentage, ratios, source);
    }
}
