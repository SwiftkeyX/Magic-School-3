// Holds every runtime data for a hero e.g. stat, buff/debuff, status, placement.
// Runtime data = data that was can be changed in game dynamically. There MUST NOT have static variable here.
public class HeroDataRuntime
{
    // ==================================== Description (name, skill desc, etc...) ====================================
    private string _name;

    // ==================================== Stat ====================================
    private Stat _stat;
    private StatModifier _statModifier;

    // ==================================== Position ====================================
    private Placement _currentPlacement;    // placement hero stand on e.g. hex, benchslot
    private Hex _reservedHex;               // hex that hero reserved. use while battle
    private Hero _nearestEnemy;

    // ==================================== getter ====================================
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

    // ==================================== setter ====================================
    public void SetCurrentPlacement(Placement placement) => _currentPlacement = placement;
    public void SetReservedHex(Hex hex) => _reservedHex = hex;
    public void SetNearestEnemy(Hero hero) => _nearestEnemy = hero;
    public void SetCurrentHP(int value) => _stat.SetCurrentHP(value);
    public bool GainMana(int amount) => _stat.AddMana(amount);
    public void AddModifier(Modifier modifier) => _statModifier.AddModifier(modifier);
    public void TickModifiers(float deltaTime) => _statModifier.Tick(deltaTime);
    public float DamageReductionPercent => _stat.DamageReductionPercent;


    public HeroDataRuntime(HeroDataSO dataSO)
    {
        _name = dataSO.Name;
        _stat = new Stat(dataSO);
        _statModifier = new StatModifier(_stat);
    }
}
