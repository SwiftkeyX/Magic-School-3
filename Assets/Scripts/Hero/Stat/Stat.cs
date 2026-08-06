using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stat of a hero.
///
/// This class should only contain data and its getter & setter only.
/// NO LOGIC.
/// </summary>
public class Stat
{
    // ========================================== dependency ==========================================
    private readonly StatModifier _statModifier = new StatModifier();

    // ========================================== base stat ==========================================
    private readonly Dictionary<StatType, float> _base = new Dictionary<StatType, float>();

    private int _currentHP;
    private int _currentMana;

    // ========================================== base stat getter ==========================================
    public float GetBaseStat(StatType type) => _base[type];

    // ========================================== modify stat getter ==========================================
    public float GetFinalStat(StatType type) => _statModifier.Apply(type, _base[type]);

    public int HP => Mathf.RoundToInt(GetFinalStat(StatType.HP));
    public int Atk => Mathf.RoundToInt(GetFinalStat(StatType.Atk));
    public int DF => Mathf.RoundToInt(GetFinalStat(StatType.DF));
    public int MG => Mathf.RoundToInt(GetFinalStat(StatType.MG));
    public int MR => Mathf.RoundToInt(GetFinalStat(StatType.MR));
    public float AttackSpeed => GetFinalStat(StatType.AttackSpeed);
    public int Range => Mathf.RoundToInt(GetFinalStat(StatType.Range));
    public int StartMana => Mathf.RoundToInt(GetFinalStat(StatType.StartMana));
    public int MaxMana => Mathf.RoundToInt(GetFinalStat(StatType.MaxMana));

    public int CurrentHP => _currentHP;
    public int CurrentMana => _currentMana;

    public bool IsStunned => _statModifier.HasModifier(ModifierEnum.Stun);
    public bool IsWounded => _statModifier.HasModifier(ModifierEnum.Wound);
    public float DamageReductionPercent => _statModifier.SumModifier(ModifierEnum.DamageReduction);

    // ========================================== setter ==========================================
    public void SetCurrentHP(int value) => _currentHP = Mathf.Clamp(value, 0, HP);

    // This may conflict with "NO LOGIC", BUT since it was 4 line of code, it was allowed here.
    // Add mana but with additional logic for preventing mana to go above its capacity
    public bool AddMana(int amount)
    {
        int newMana = _currentMana + amount;
        bool capped = newMana >= MaxMana;
        _currentMana = capped ? 0 : newMana;
        return capped;
    }

    public void AddModifier(Modifier modifier) => _statModifier.AddModifier(modifier);
    public void TickModifiers(float deltaTime) => _statModifier.Tick(deltaTime);


    public Stat(HeroDataSO stat)
    {
        _base[StatType.HP] = stat.HP;
        _base[StatType.Atk] = stat.Atk;
        _base[StatType.DF] = stat.DF;
        _base[StatType.MG] = stat.MG;
        _base[StatType.MR] = stat.MR;
        _base[StatType.AttackSpeed] = stat.AttackSpeed;
        _base[StatType.Range] = stat.Range;
        _base[StatType.StartMana] = stat.StartMana;
        _base[StatType.MaxMana] = stat.MaxMana;

        _currentHP = HP;
        _currentMana = StartMana;
    }
}
