using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cadence
{
    public bool isCadence;
    public bool cadenceInterval;    // the interval of time effect is re-apply
}

[Serializable]
public abstract class SkillEffect
{
    [SerializeField] private EffectRecipientEnum _recipient;        // the list of recipients who get effect
    [SerializeField] private Cadence _cadence;                      // Is effect reapply over time?       

    // ================================== getter ==================================
    public EffectRecipientEnum Recipient => _recipient;

    public abstract void ApplyEffect(List<Hero> recipients);
}

[Serializable]
public class AttackSkillEffect : SkillEffect
{
    [SerializeField] private float _damageAmount;
    // FLAG: I mark this here as to tell that I have think throughly about AttackSkillEffect shouldn't have duration
    // [SerializeField] private float _skillDuration;

    public override void ApplyEffect(List<Hero> recipients)
    {
        foreach (Hero recipient in recipients)
        {
            if (recipient == null || recipient.State == HeroStateType.Dead) continue;

            // apply damage
            recipient.Blackboard.TakeDamage(Mathf.RoundToInt(_damageAmount));
        }
    }

}

[Serializable]
public class BuffSkillEffect : SkillEffect, Modifier
{
    [SerializeField] private ModifierEnum _modifier;
    [SerializeField] private float _buffAmount;
    [SerializeField] private float _buffDuration;

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