using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillEffect
{
    [SerializeField] private EffectRecipientEnum _recipient;
    [SerializeField] private int _aoeRadius;

    // ================================== getter ==================================
    public EffectRecipientEnum Recipient => _recipient;
    public int AoeRadius => _aoeRadius;

    protected SkillEffect(EffectRecipientEnum recipient, int aoeRadius = 1)
    {
        _recipient = recipient;
        _aoeRadius = aoeRadius;
    }

    public abstract void ApplyEffect(List<Hero> recipients);
}

[Serializable]
public class AttackSkillEffect : SkillEffect
{
    [SerializeField] private float _damageAmount;
    // FLAG: I mark this here as to tell that I have think throughly about AttackSkillEffect shouldn't have duration
    // [SerializeField] private float _skillDuration;

    public AttackSkillEffect(EffectRecipientEnum recipient, int aoeRadius, float damageAmount) : base(recipient, aoeRadius)
    {
        _damageAmount = damageAmount;
    }

    public override void ApplyEffect(List<Hero> recipients)
    {
        foreach (Hero recipient in recipients)
        {
            if (recipient == null || recipient.State == HeroStateType.Dead) continue;

            // apply damage
            recipient.Blackboard.TakeDamage(Mathf.RoundToInt(_damageAmount));

            // On kill, trigger related skill
            SkillTrigger.OnKill();
        }
    }
}

[Serializable]
public class BuffSkillEffect : SkillEffect, Modifier
{
    [SerializeField] private ModifierEnum _modifier;
    [SerializeField] private float _buffAmount;
    [SerializeField] private float _buffDuration;

    public BuffSkillEffect(EffectRecipientEnum recipient, int aoeRadius, ModifierEnum detail, float buffAmount, float buffDuration) : base(recipient, aoeRadius)
    {
        _modifier = detail;
        _buffAmount = buffAmount;
        _buffDuration = buffDuration;
    }

    public override void ApplyEffect(List<Hero> recipients)
    {
        foreach (Hero recipient in recipients)
        {
            if (recipient == null || recipient.State == HeroStateType.Dead) continue;

            recipient.Blackboard.AddModifier(this);
            if (_modifier == ModifierEnum.BonusHP) recipient.Blackboard.Heal(_buffAmount);
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

    public DebuffSkillEffect(EffectRecipientEnum recipient, int aoeRadius, ModifierEnum detail, float deBuffAmount, float deBuffDuration) : base(recipient, aoeRadius)
    {
        _modifier = detail;
        _debuffAmount = deBuffAmount;
        _deBuffDuration = deBuffDuration;
    }

    public override void ApplyEffect(List<Hero> recipients)
    {
        throw new NotImplementedException();
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

    public StatusSkillEffect(EffectRecipientEnum recipient, int aoeRadius, ModifierEnum detail, float statusDuration) : base(recipient, aoeRadius)
    {
        _modifier = detail;
        _statusDuration = statusDuration;
        // initiate _statusAmount using dictionary e.g. wound => fix 50%, stun => fix 0%, etc...
    }

    public override void ApplyEffect(List<Hero> recipients)
    {
        foreach (Hero recipient in recipients)
        {
            if (recipient == null || recipient.State == HeroStateType.Dead) continue;

            recipient.Blackboard.AddModifier(this);
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