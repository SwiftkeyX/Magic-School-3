using System;

// Holds every runtime data for a hero e.g. stat, buff/debuff, status, placement.
// Runtime data = data that was can be changed in game dynamically. There MUST NOT have static variable here.
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
    public int HP => _stat.BaseHP;
    public int Atk => _stat.BaseAtk;
    public int DF => _stat.BaseDF;
    public int MG => _stat.BaseMG;
    public int MR => _stat.BaseMR;
    public float AttackSpeed => _stat.BaseAttackSpeed;
    public int Range => _stat.BaseRange;
    public int StartMana => _stat.BaseStartMana;
    public int MaxMana => _stat.BaseMaxMana;
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
