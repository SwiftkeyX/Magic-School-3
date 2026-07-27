using System;
using UnityEngine;

public class Stat
{
    private int _currentHP;
    private int _maxHP;
    private int _attack;
    private int _defend;
    private int _magic;
    private int _magicResist;
    private float _attackSpeed;
    private int _range;
    private int _startMana;
    private int _maxMana;
    private int _currentMana;

    private readonly ModifierList _modifiers = new ModifierList();

    // ===================== getter =====================
    public int HP => _maxHP + Mathf.RoundToInt(_modifiers.Sum(EffectDetail.BonusHP));
    public int Atk => _attack;
    public int DF => Mathf.Max(0, _defend - Mathf.RoundToInt(_modifiers.Sum(EffectDetail.DEFShred)));
    public int MG => _magic;
    // MR shred is tracked here for authoring parity with the sheet, but TakeDamage doesn't yet
    // distinguish magic damage (see Skill.md) - so it has no gameplay effect until that lands.
    public int MR => Mathf.Max(0, _magicResist - Mathf.RoundToInt(_modifiers.Sum(EffectDetail.MRShred)));
    public float AttackSpeed => _attackSpeed + _modifiers.Sum(EffectDetail.AttackSpeed);
    public int Range => _range;
    public int StartMana => _startMana;
    public int MaxMana => _maxMana;
    public int CurrentHP => _currentHP;
    public int CurrentMana => _currentMana;
    public float DamageReductionPercent => Mathf.Clamp(_modifiers.Sum(EffectDetail.DamageReduction), 0f, 90f);
    public bool IsStunned => _modifiers.Has(EffectDetail.Stun);
    public bool IsWounded => _modifiers.Has(EffectDetail.Wound);

    // =================== setter =====================
    public void SetCurrentHP(int value) => _currentHP = Mathf.Clamp(value, 0, HP);

    // Returns true the instant mana caps out - the caller fires the skill's OnCast trigger then
    // resets mana, rather than this class arming a hardcoded "next attack is empowered" flag.
    public bool AddMana(int amount)
    {
        int newMana = _currentMana + amount;
        bool capped = newMana >= _maxMana;
        _currentMana = capped ? 0 : newMana;
        return capped;
    }

    public void AddModifier(StatModifier modifier) => _modifiers.Add(modifier);
    public void TickModifiers(float deltaTime, Action<StatModifier> onExpired) => _modifiers.Tick(deltaTime, onExpired);

    // ==================== etc ============================
    public int ConsumeAttackDamage(int baseAtk) => baseAtk;

    public Stat(HeroDataSO stat)
    {
        _maxHP = stat.HP;
        _attack = stat.Atk;
        _defend = stat.DF;
        _magic = stat.MG;
        _magicResist = stat.MR;
        _attackSpeed = stat.AttackSpeed;
        _range = stat.Range;
        _startMana = stat.StartMana;
        _maxMana = stat.MaxMana;

        _currentHP = _maxHP;
        _currentMana = _startMana;
    }

}
