using System;
using System.Collections.Generic;
using UnityEngine;

// 1) SkillSO = SO for the skill system. Skill are data-driven which is suitable for using scritable object.
// 2) SkillSO contains several "SkillStep".
// 2.1) SkillStep are small skill that can work independently on its own e.g. do AOE damage, shoot projectile, etc...
// 3) So SkillSO are the bigger skill that construct by several SkillStep and actually was used by the actual hero.
[CreateAssetMenu(fileName = "Skill", menuName = "Magic School 3/Skill Definition")]
public class SkillSO : ScriptableObject
{
    [SerializeField] private string _skillName = "Skill";
    [SerializeField] private List<SkillStep> _steps = new List<SkillStep>();

    // ================================== getter ==================================
    public string SkillName => _skillName;
    public IReadOnlyList<SkillStep> Steps => _steps;

    // ================================== setter ==================================
    public void SetSkillName(string skillName) => _skillName = skillName;
    public void SetSteps(List<SkillStep> steps) => _steps = steps;
}


[Serializable]
public class SkillStep
{
    [SerializeField] private TriggerEnum _trigger;
    [SerializeField] private List<SkillEffect> _effects;

    // ================================== getter ==================================
    public TriggerEnum Trigger => _trigger;
    public IReadOnlyList<SkillEffect> Effects => _effects;

    // ================================== setter ==================================
    public SkillStep(TriggerEnum trigger, List<SkillEffect> effects)
    {
        _trigger = trigger;
        _effects = effects;
    }
}

//
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

            recipient.Blackboard.AddModifier(new StatModifier(_modifier, 0f, _statusDuration));
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