using UnityEngine;

/// <summary>
/// Stat of a hero.
/// This class should only contain data and its getter & setter only.
/// NO LOGIC.
/// </summary>
public class Stat
{
    // ========================================== base stat ==========================================
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

    // ========================================== base stat getter ==========================================
    public int BaseHP => _maxHP;
    public int BaseAtk => _attack;
    public int BaseDF => _defend;
    public int BaseMG => _magic;
    public int BaseMR => _magicResist;
    public float BaseAttackSpeed => _attackSpeed;
    public int BaseRange => _range;
    public int BaseStartMana => _startMana;
    public int BaseMaxMana => _maxMana;
    public int CurrentHP => _currentHP;
    public int CurrentMana => _currentMana;

    // ========================================== modify stat getter ==========================================
    public int HP => _statModifier.ModifiedHP();
    public int Atk => _statModifier.ModifiedAtk();
    public int DF => _statModifier.ModifiedDF();
    public bool IsStunned => _statModifier.HasModifier(ModifierEnum.Stun);
    public bool IsWounded => _statModifier.HasModifier(ModifierEnum.Wound);
    public float DamageReductionPercent => _statModifier.SumModifier(ModifierEnum.DamageReduction);

    // ========================================== setter ==========================================
    public void SetCurrentHP(int value) => _currentHP = Mathf.Clamp(value, 0, BaseHP);

    // This may conflict with "NO LOGIC", BUT since it was 4 line of code, it was allowed here.
    // Add mana but with additional logic for preventing mana to go above its capacity
    public bool AddMana(int amount)
    {
        int newMana = _currentMana + amount;
        bool capped = newMana >= _maxMana;
        _currentMana = capped ? 0 : newMana;
        return capped;
    }

    public void AddModifier(Modifier modifier) => _statModifier.AddModifier(modifier);
    public void TickModifiers(float deltaTime) => _statModifier.Tick(deltaTime);

    // ========================================== dependency ==========================================
    private StatModifier _statModifier;

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

        _statModifier = new StatModifier(this);
    }


}
