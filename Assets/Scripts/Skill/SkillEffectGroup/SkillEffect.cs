using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Not cadence => apply effect when hitbox collide (OnTriggerEnter())
/// cadence but start only initial collision => e.g. teemo's mushroom (after OnTriggerEnter(), start coroutine for the poison damage)
/// cadence but not need initial collision => e.g. garen's E (OnTriggerEnter() trigger every interval)
/// </summary>
[Serializable]
public class Cadence
{
    public bool isCadence = false;
    public float cadenceInterval = 0.5f;    // the interval of time effect is re-apply
    public float cadenceDuration = 3f;      // total duration of the effect
}

[Serializable]
public abstract class SkillEffect
{
    [SerializeField] protected EffectRecipientEnum _recipient;        // the list of recipients who get effect
    [SerializeField] protected Cadence _cadence = new Cadence();      // Is effect reapply over time?

    // ================================== getter ==================================
    public EffectRecipientEnum Recipient => _recipient;
    public Cadence Cadence => _cadence;

    public abstract void ApplyEffect(IReadOnlyList<IDamageable> recipients);
}

[Serializable]
public class AttackSkillEffect : SkillEffect
{
    [SerializeField] private float _damageAmount;

    public override void ApplyEffect(IReadOnlyList<IDamageable> recipients)
    {
        foreach (IDamageable recipient in recipients)
        {
            if (recipient == null || !recipient.IsAlive) continue;

            // if the damage was cadence, calculate tick damage instead
            float dmg;
            if (_cadence.isCadence) dmg = _damageAmount * _cadence.cadenceInterval / _cadence.cadenceDuration;
            
            // if not cadence, apply normal flat damage
            else dmg = _damageAmount;

            // apply damage
            recipient.TakeDamage(Mathf.RoundToInt(dmg));
        }
    }

}

[Serializable]
public class HealSkillEffect : SkillEffect
{
    [SerializeField] private float _totalHealAmount;   // spread evenly across every cadence tick over _duration
    [SerializeField] private float _duration;

    public float Duration => _duration;

    public override void ApplyEffect(IReadOnlyList<IDamageable> recipients)
    {
        int totalTicks = Mathf.Max(1, Mathf.RoundToInt(_duration / Cadence.cadenceInterval));
        float healPerTick = _totalHealAmount / totalTicks;

        foreach (IDamageable recipient in recipients)
        {
            if (recipient == null || !recipient.IsAlive) continue;

            recipient.Heal(healPerTick);
        }
    }
}

// FIXLATER: I see BuffSkillEffect/DebuffSkillEffect/StatusSkillEffect are all the same. I starting to consider combining them.
[Serializable]
public class BuffSkillEffect : SkillEffect, Modifier
{
    [SerializeField] private ModifierEnum _modifier;
    [SerializeField] private float _buffAmount;
    [SerializeField] private float _buffDuration;

    public override void ApplyEffect(IReadOnlyList<IDamageable> recipients)
    {
        foreach (IDamageable recipient in recipients)
        {
            if (recipient == null || !recipient.IsAlive) continue;

            recipient.AddModifier(this);

            // FIXLATER: this shouldn't be here, so I comment it out, and it doesn't even use now, so let leave it for now.
            // if (_modifier == ModifierEnum.BonusHP) recipient.Heal(_buffAmount);
        }
    }

    public float GetAmount()
    {
        return _buffAmount;
    }

    public ModifierEnum GetModifier()
    {
        return _modifier;
    }

    public float GetDuration()
    {
        return _buffDuration;
    }
}

[Serializable]
public class DebuffSkillEffect : SkillEffect, Modifier
{
    [SerializeField] private ModifierEnum _modifier;
    [SerializeField] private float _debuffAmount;
    [SerializeField] private float _deBuffDuration;

    public override void ApplyEffect(IReadOnlyList<IDamageable> recipients)
    {
        foreach (IDamageable recipient in recipients)
        {
            if (recipient == null || !recipient.IsAlive) continue;

            recipient.AddModifier(this);
        }
    }

    public float GetAmount()
    {
        return _debuffAmount;
    }

    public ModifierEnum GetModifier()
    {
        return _modifier;
    }

    public float GetDuration()
    {
        return _deBuffDuration;
    }
}

[Serializable]
public class StatusSkillEffect : SkillEffect, Modifier
{
    [SerializeField] private ModifierEnum _modifier;
    [SerializeField] private float _statusAmount;
    [SerializeField] private float _statusDuration;

    public override void ApplyEffect(IReadOnlyList<IDamageable> recipients)
    {
        foreach (IDamageable recipient in recipients)
        {
            if (recipient == null || !recipient.IsAlive) continue;

            recipient.AddModifier(this);
        }
    }

    public float GetAmount()
    {
        return _statusAmount;
    }

    public ModifierEnum GetModifier()
    {
        return _modifier;
    }

    public float GetDuration()
    {
        return _statusDuration;
    }
}