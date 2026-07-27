using System;
using System.Collections.Generic;
using UnityEngine;

// Holds every piece of runtime data for a hero
// separate from HeroDataSO
public class HeroDataRuntime
{
    // ================== Description (name, skill desc, etc...) =================
    private string _name;

    // ================== Stat ====================
    private Stat _stat;

    // ===================== Position ========================
    private Placement _currentPlacement;    // placement hero stand on e.g. hex, benchslot
    private Hex _reservedHex;               // hex that hero reserved. use while battle
    private Hero _nearestEnemy;

    // ===================== getter =====================
    public string Name => _name;
    public int HP => _stat.HP;
    public int Atk => _stat.Atk;
    public int DF => _stat.DF;
    public int MG => _stat.MG;
    public int MR => _stat.MR;
    public float AttackSpeed => _stat.AttackSpeed;
    public int Range => _stat.Range;
    public int StartMana => _stat.StartMana;
    public int MaxMana => _stat.MaxMana;
    public Placement CurrentPlacement => _currentPlacement;
    // reserved hex = hex that this hero want to walk into, so he reserved it, to prevent other hero to walk into the same hex
    public Hex ReservedHex => _reservedHex;
    public int CurrentHP => _stat.CurrentHP;
    public int CurrentMana => _stat.CurrentMana;
    public Hero NearestEnemy => _nearestEnemy;
    public bool IsStunned => _stat.IsStunned;
    public bool IsWounded => _stat.IsWounded;

    // =================== setter =====================
    public void SetCurrentPlacement(Placement placement) => _currentPlacement = placement;
    public void SetReservedHex(Hex hex) => _reservedHex = hex;
    public void SetNearestEnemy(Hero hero) => _nearestEnemy = hero;
    public void SetCurrentHP(int value) => _stat.SetCurrentHP(value);
    public bool GainMana(int amount) => _stat.AddMana(amount);
    public void Heal(float amount) => _stat.Heal(amount);
    public void AddModifier(StatModifier modifier) => _stat.AddModifier(modifier);
    public float DamageReductionPercent => _stat.DamageReductionPercent;

    // ==================== etc ========================
    public int ConsumeAttackDamage(int baseAtk) => _stat.ConsumeAttackDamage(baseAtk);
    // Ticks every timed StatModifier down; onExpired fires once per modifier that just expired
    public void TickModifiers(float deltaTime, Action<StatModifier> onExpired) => _stat.TickModifiers(deltaTime, onExpired);

    public HeroDataRuntime(HeroDataSO dataSO)
    {
        _name = dataSO.Name;
        _stat = new Stat(dataSO);
    }
}

// 1) One timed buff/debuff instance layered on top of a Stat's base values. 
// 2) Amount's meaning depends on Detail (e.g. BonusHP/AttackSpeed/DamageReduction add, Stun/Wound just need to be
// present - Amount is unused for those). 
// 3) Remaining counts down in TickModifiers;
// 4) float.PositiveInfinity means "lasts until something else removes it" (the sheet's "Permanent").
public class StatModifier
{
    public readonly EffectDetail Detail;
    public readonly float Amount;
    public float Remaining;

    public StatModifier(EffectDetail detail, float amount, float durationSeconds)
    {
        Detail = detail;
        Amount = amount;
        Remaining = durationSeconds <= 0f ? float.PositiveInfinity : durationSeconds;
    }
}

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

    private readonly List<StatModifier> _modifiers = new List<StatModifier>();

    // ===================== getter =====================
    public int HP => _maxHP + Mathf.RoundToInt(ModifierSum(EffectDetail.BonusHP));
    public int Atk => _attack;
    public int DF => Mathf.Max(0, _defend - Mathf.RoundToInt(ModifierSum(EffectDetail.DEFShred)));
    public int MG => _magic;
    // MR shred is tracked here for authoring parity with the sheet, but TakeDamage doesn't yet
    // distinguish magic damage (see Skill.md) - so it has no gameplay effect until that lands.
    public int MR => Mathf.Max(0, _magicResist - Mathf.RoundToInt(ModifierSum(EffectDetail.MRShred)));
    public float AttackSpeed => _attackSpeed + ModifierSum(EffectDetail.AttackSpeed);
    public int Range => _range;
    public int StartMana => _startMana;
    public int MaxMana => _maxMana;
    public int CurrentHP => _currentHP;
    public int CurrentMana => _currentMana;
    public float DamageReductionPercent => Mathf.Clamp(ModifierSum(EffectDetail.DamageReduction), 0f, 90f);
    public bool IsStunned => HasModifier(EffectDetail.Stun);
    public bool IsWounded => HasModifier(EffectDetail.Wound);

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

    // Wound halves incoming healing (simplified from "reduce healing" per effect-types.csv,
    // since this game has no finer-grained heal-reduction percentage authored yet).
    public void Heal(float amount)
    {
        float healed = IsWounded ? amount * 0.5f : amount;
        _currentHP = Mathf.Clamp(_currentHP + Mathf.RoundToInt(healed), 0, HP);
    }

    public void TickModifiers(float deltaTime, Action<StatModifier> onExpired)
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            StatModifier modifier = _modifiers[i];
            if (float.IsPositiveInfinity(modifier.Remaining)) continue;

            modifier.Remaining -= deltaTime;
            if (modifier.Remaining <= 0f)
            {
                _modifiers.RemoveAt(i);
                onExpired?.Invoke(modifier);
            }
        }
    }

    private float ModifierSum(EffectDetail detail)
    {
        float sum = 0f;
        foreach (StatModifier modifier in _modifiers)
            if (modifier.Detail == detail) sum += modifier.Amount;
        return sum;
    }

    private bool HasModifier(EffectDetail detail)
    {
        foreach (StatModifier modifier in _modifiers)
            if (modifier.Detail == detail) return true;
        return false;
    }

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
