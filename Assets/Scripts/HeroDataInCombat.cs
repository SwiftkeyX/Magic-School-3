// Not use yet, but at some point in the project
// we'll have to separate SO and runtime data apart.
public class HeroDataInCombat
{
    // ================== Description (name, skill desc, etc...) =================
    private string _name;

    // ================== Stat ====================
    private Stat _stat;

    // ===================== Position ========================
    private Hex _currentHex;
    private Hex _reservedHex;
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
    // current hex = hex that this hero is currently standing on
    public Hex CurrentHex => _currentHex;
    // reserved hex = hex that this hero want to walk into, so he reserved it, to prevent other hero to walk into the same hex
    public Hex ReservedHex => _reservedHex;
    public int CurrentHP => _stat.CurrentHP;
    public int CurrentMana => _stat.CurrentMana;
    public Hero NearestEnemy => _nearestEnemy;

    // =================== setter =====================
    public void SetCurrentHex(Hex hex) => _currentHex = hex;
    public void SetReservedHex(Hex hex) => _reservedHex = hex;
    public void SetNearestEnemy(Hero hero) => _nearestEnemy = hero;
    public void SetCurrentHP(int value) => _stat.SetCurrentHP(value);
    public void GainMana(int amount) => _stat.GainMana(amount);

    // ==================== etc ========================
    public int ConsumeAttackDamage(int baseAtk) => _stat.ConsumeAttackDamage(baseAtk);

    public HeroDataInCombat(HeroDataSO dataSO)
    {
        _name = dataSO.Name;
        _stat = new Stat(dataSO);
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
    // Active skill: set once mana caps out, consumed by the next landed attack.
    private bool _skillReady;

    // ===================== getter =====================
    public int HP => _maxHP;
    public int Atk => _attack;
    public int DF => _defend;
    public int MG => _magic;
    public int MR => _magicResist;
    public float AttackSpeed => _attackSpeed;
    public int Range => _range;
    public int StartMana => _startMana;
    public int MaxMana => _maxMana;
    public int CurrentHP => _currentHP;
    public int CurrentMana => _currentMana;

    // =================== setter =====================
    public void SetCurrentHP(int value) => _currentHP = value;
    public void GainMana(int amount)
    {
        int newMana = _currentMana + amount;
        _currentMana = newMana > _maxMana ? _maxMana : newMana;

        // Full mana casts the active skill immediately - it empowers whichever attack lands next.
        if (_currentMana >= _maxMana)
        {
            _skillReady = true;
            _currentMana = 0;
        }
    }

    // ==================== etc ============================
    // This logic kinda bullshit and ruin readability 
    // But it was fine now I guess
    public int ConsumeAttackDamage(int baseAtk)
    {
        if (!_skillReady) return baseAtk;

        _skillReady = false;
        return baseAtk * 5;
    }

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
