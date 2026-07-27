using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Per-hero executor for a SkillDefinitionSO - the piece that actually turns the sheet's
// Trigger/Condition/Action/Aim Target/Effect data into gameplay. Owned by Hero exactly like
// HeroStateMachineBlackBoard, constructed once in Hero.Awake(). Every hero gets one even
// without an authored skill (_skill == null), because Tick() also decays StatModifiers an
// ENEMY's skill applied to this hero (a stun doesn't care whether its target has a skill).
public class SkillRuntime
{
    private readonly Hero _me;
    private readonly SkillDefinitionSO _skill;
    private readonly List<ActiveEffect> _activeEffects = new List<ActiveEffect>();

    // Swain-style "cast once to transform, cast again for the upgraded effect" state. Set as a
    // side effect of a step whose Condition is IfNotTransformed succeeding - see ExecuteStep.
    private bool _transformed;
    private bool _gameStartFired;

    public SkillRuntime(Hero hero, SkillDefinitionSO skill)
    {
        _me = hero;
        _skill = skill;
    }

    // Called every frame from Hero.Update() regardless of whether this hero has a skill of its
    // own - decays this hero's StatModifiers (buffs/debuffs/stun, however they got there) and
    // re-applies this hero's own periodic effects (Zone AOE ticks, channel heals, laser DoTs).
    public void Tick(float deltaTime)
    {
        _me.Blackboard.TickModifiers(deltaTime, OnModifierExpired);

        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect ae = _activeEffects[i];
            ae.Elapsed += deltaTime;
            ae.SinceLastTick += deltaTime;

            if (ae.Effect.Cadence == EffectCadence.Periodic && ae.SinceLastTick >= ae.Effect.CadenceSeconds)
            {
                ae.SinceLastTick -= ae.Effect.CadenceSeconds;
                ApplyEffectPayload(ae.Effect, ae.ResolveRecipients());
            }

            if (ae.Effect.Duration > 0f && ae.Elapsed >= ae.Effect.Duration)
            {
                _activeEffects.RemoveAt(i);
            }
        }
    }

    public void EnsureGameStartFired()
    {
        if (_gameStartFired) return;
        _gameStartFired = true;
        FireTrigger(TriggerType.GameStart);
    }

    // Runs every Step whose Trigger matches and whose Condition currently holds. Public because
    // it's fired from outside too: HeroAttack on mana-cap (OnCast) and on a kill (OnKill), Hero
    // on entering combat (GameStart via EnsureGameStartFired) - as well as recursively, from
    // ExecuteStep itself, for AfterStepN sequencing and WhenTransformed.
    public void FireTrigger(TriggerType trigger)
    {
        if (_skill == null) return;

        foreach (SkillStep step in _skill.Steps)
        {
            if (step.Trigger != trigger) continue;
            if (!ConditionMet(step.Condition)) continue;
            ExecuteStep(step);
        }
    }

    private bool ConditionMet(ConditionType condition)
    {
        switch (condition)
        {
            case ConditionType.IfTransformed: return _transformed;
            case ConditionType.IfNotTransformed: return !_transformed;
            default: return true;
        }
    }

    private void ExecuteStep(SkillStep step)
    {
        // Casting while not-yet-transformed IS the transformation (Swain) - a side effect of
        // this condition succeeding, not a separate authored effect, since "become transformed"
        // isn't a buff/debuff/damage payload the sheet's Effect columns model.
        if (step.Condition == ConditionType.IfNotTransformed)
        {
            _transformed = true;
            FireTrigger(TriggerType.WhenTransformed);
        }

        Hero aimTarget = ResolveAimTarget(step.AimTarget);

        // Move/Move Behind only really repositions the caster when the step's own payload says
        // so (Effect Detail "(reposition)") - Katarina's first Move is "(setup)" and leaves her
        // standing still, her second Move is the real blink. Same action key, different payload.
        bool isReposition = step.Effects.Any(e => e.Category == EffectCategory.Movement && e.Detail == EffectDetail.Reposition);
        if (isReposition && (step.Action == ActionKey.Move || step.Action == ActionKey.MoveBehind))
        {
            Reposition(aimTarget);
        }

        foreach (SkillEffect effect in step.Effects)
        {
            if (effect.Category == EffectCategory.Movement) continue; // reposition/setup carry no further payload
            if (effect.Category == EffectCategory.Summon) continue;   // out of Phase 1 scope

            if (effect.Cadence == EffectCadence.Once)
            {
                ApplyEffectPayload(effect, ResolveRecipients(effect.Recipient, aimTarget, step));
            }
            else
            {
                _activeEffects.Add(new ActiveEffect(effect, BuildRecipientResolver(effect, aimTarget, step)));
            }
        }

        FireTrigger(AfterStepTriggerFor(step.Step));
    }

    private static TriggerType AfterStepTriggerFor(int stepNumber)
    {
        switch (stepNumber)
        {
            case 1: return TriggerType.AfterStep1;
            case 2: return TriggerType.AfterStep2;
            default: return TriggerType.AfterStep3;
        }
    }

    #region Aim target / recipients

    private Hero ResolveAimTarget(AimTargetType aimType)
    {
        switch (aimType)
        {
            case AimTargetType.Self: return _me;
            case AimTargetType.Current:
            case AimTargetType.CurrentNew:
                return _me.Blackboard.FindNearestEnemy();
            case AimTargetType.Clustered:
                return _me.Blackboard.Board.FindClusteredEnemy(_me.Team);
            default: return null;
        }
    }

    private List<Hero> ResolveRecipients(EffectRecipientType recipientType, Hero aimTarget, SkillStep step)
    {
        switch (recipientType)
        {
            case EffectRecipientType.Self:
                return new List<Hero> { _me };
            case EffectRecipientType.SameAsAimTarget:
                return aimTarget != null ? new List<Hero> { aimTarget } : new List<Hero>();
            case EffectRecipientType.EnemiesInArea:
                return aimTarget != null ? EnemiesWithinRadius(aimTarget, step.AoeRadius) : new List<Hero>();
            default:
                return new List<Hero>();
        }
    }

    // AOE recipients are rebuilt every tick (Zone AOE's whole point); single-target recipients
    // are captured once at cast time and simply filtered for death each tick.
    private Func<List<Hero>> BuildRecipientResolver(SkillEffect effect, Hero aimTarget, SkillStep step)
    {
        if (effect.Recipient == EffectRecipientType.EnemiesInArea)
        {
            int radius = step.AoeRadius;
            return () => aimTarget != null && aimTarget.State != HeroStateType.Dead
                ? EnemiesWithinRadius(aimTarget, radius)
                : new List<Hero>();
        }

        List<Hero> fixedRecipients = ResolveRecipients(effect.Recipient, aimTarget, step);
        return () => fixedRecipients.Where(h => h != null && h.State != HeroStateType.Dead).ToList();
    }

    private List<Hero> EnemiesWithinRadius(Hero centerHero, int radius)
    {
        Hex centerHex = centerHero.Blackboard.GetCurrentHex();
        if (centerHex == null) return new List<Hero>();

        return _me.Blackboard.Board.HeroesOnBoard
            .Where(h => h.Team != _me.Team && h.State != HeroStateType.Dead && centerHex.IsWithinRange(h.Blackboard.GetCurrentHex(), radius))
            .ToList();
    }

    // Instantly relocates the caster to a free hex adjacent to the target - this game has no
    // cast-time travel animation for reposition actions yet, so the "charge in" beat is a snap
    // rather than a lerp (HeroWalk's lerp is reserved for ordinary movement-phase pathing).
    private void Reposition(Hero target)
    {
        if (target == null) return;
        Hex targetHex = target.Blackboard.GetCurrentHex();
        if (targetHex == null) return;

        var occupied = new HashSet<Hex>(_me.Blackboard.Board.HeroesOnBoard
            .Where(h => h != _me && h.State != HeroStateType.Dead)
            .Select(h => h.Blackboard.GetCurrentHex()));

        Hex destination = targetHex.GetNeighbors().FirstOrDefault(n => !occupied.Contains(n));
        if (destination == null) return; // no room to land - stay put rather than stack on another hero

        Hex previousHex = _me.Blackboard.GetCurrentHex();
        previousHex?.OnHeroUnplaced(_me);
        destination.OnHeroPlaced(_me);
        _me.transform.position = destination.transform.position;
    }

    #endregion

    #region Effect application

    private void ApplyEffectPayload(SkillEffect effect, List<Hero> recipients)
    {
        foreach (Hero recipient in recipients)
        {
            if (recipient == null || recipient.State == HeroStateType.Dead) continue;

            switch (effect.Category)
            {
                case EffectCategory.Attack:
                    // Must check BEFORE dealing damage: OnKill has to fire exactly once, on the
                    // alive->dead transition. Checking only "is HP now <= 0" would refire every
                    // time a already-dead body takes another hit - and since Darius's own OnKill
                    // recast deals damage too, that reads its own output and recurses forever.
                    bool wasAlive = recipient.Blackboard.GetCurrentHP() > 0;
                    recipient.Blackboard.TakeDamage(Mathf.RoundToInt(effect.Amount));
                    // Darius: a skill hit that kills recasts on a fresh target. Checked here
                    // (not in HeroAttack) because it's THIS damage - the skill's own Cast action
                    // landing a kill - that TFT's "On Kill" trigger actually means for his kit.
                    if (wasAlive && recipient.Blackboard.GetCurrentHP() <= 0) FireTrigger(TriggerType.OnKill);
                    break;
                case EffectCategory.Buff:
                    ApplyBuff(recipient, effect);
                    break;
                case EffectCategory.Debuff:
                    ApplyDebuff(recipient, effect);
                    break;
                case EffectCategory.Status:
                    ApplyStatus(recipient, effect);
                    break;
            }
        }
    }

    private void ApplyBuff(Hero recipient, SkillEffect effect)
    {
        switch (effect.Detail)
        {
            case EffectDetail.Heal:
                recipient.Blackboard.Heal(effect.Amount);
                break;
            case EffectDetail.BonusHP:
                // Bonus HP both raises the cap and fills it - it reads as a heal-plus-shield, not
                // just extra unused headroom.
                recipient.Blackboard.AddModifier(new StatModifier(EffectDetail.BonusHP, effect.Amount, effect.Duration));
                recipient.Blackboard.Heal(effect.Amount);
                break;
            case EffectDetail.DamageReduction:
            case EffectDetail.AttackSpeed:
                recipient.Blackboard.AddModifier(new StatModifier(effect.Detail, effect.Amount, effect.Duration));
                break;
        }
    }

    private void ApplyDebuff(Hero recipient, SkillEffect effect)
    {
        switch (effect.Detail)
        {
            case EffectDetail.MRShred:
            case EffectDetail.DEFShred:
                recipient.Blackboard.AddModifier(new StatModifier(effect.Detail, effect.Amount, effect.Duration));
                break;
        }
    }

    private void ApplyStatus(Hero recipient, SkillEffect effect)
    {
        switch (effect.Detail)
        {
            case EffectDetail.Stun:
                recipient.Blackboard.AddModifier(new StatModifier(EffectDetail.Stun, 0f, effect.Duration));
                break;
            case EffectDetail.Wound:
                recipient.Blackboard.AddModifier(new StatModifier(EffectDetail.Wound, 0f, effect.Duration));
                break;
        }
    }

    private void OnModifierExpired(StatModifier modifier)
    {
        if (modifier.Detail == EffectDetail.BonusHP)
        {
            FireTrigger(TriggerType.OnBonusHPExpire);
        }
    }

    #endregion
}
