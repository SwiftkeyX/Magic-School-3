/// <summary>
/// Hold every runtime data for a hero e.g. stat, placement.
/// Runtime data = data that was can be changed in game dynamically. There MUST NOT have static variable here.
/// 
/// The data flow for hero look like this: 
/// Manager class (Hero, HeroStateMachineBlackBoard) => Logic class (CombatMath, FindEnemy, SkillExecutor) => Data class (HeroDataRuntime, Stat)
/// So HeroDataRuntime is a Data class one.  
/// 
/// </summary>
public class HeroDataRuntime
{
    // ==================================== Description (name, skill desc, etc...) ====================================
    // TODO: this is temporarily.
    private bool _isDummy;

    // ==================================== Stat ====================================
    private Stat _stat;
    private StatModifier _statModifier;     // FIXME: stat modifier is actually a logic class, and HeroDataRuntime shouldn't reference it. Move this to HeroStateMachineBlackBoard.

    // ==================================== Position ====================================
    private Placement _currentPlacement;    // placement hero stand on e.g. hex, benchslot
    private Hex _reservedHex;               // hex that hero reserved. use while battle
    private Hero _nearestEnemy;


    // ==================================== getter ====================================
    // === stat === 
    public int HP => _stat.HP;
    public int Atk => _stat.Atk;
    public int DF => _stat.DF;
    public int MG => _stat.MG;
    public int MR => _stat.MR;
    public float AttackSpeed => _stat.AttackSpeed;
    public int Range => _stat.Range;
    public int StartMana => _stat.StartMana;
    public int MaxMana => _stat.MaxMana;
    public int CurrentHP => _stat.CurrentHP;
    public int CurrentMana => _stat.CurrentMana;
    public bool IsStunned => _stat.IsStunned;
    public bool IsWounded => _stat.IsWounded;

    // === placement === 
    public Placement CurrentPlacement => _currentPlacement;
    public Hex ReservedHex => _reservedHex;
    public Hero NearestEnemy => _nearestEnemy;

    // === etc ===
    public bool IsDummy => _isDummy;


    // ==================================== setter ====================================
    // === Placement ===
    public void SetCurrentPlacement(Placement placement) => _currentPlacement = placement;
    public void SetReservedHex(Hex hex) => _reservedHex = hex;
    public void SetNearestEnemy(Hero hero) => _nearestEnemy = hero;

    // === Stat ===
    public void SetCurrentHP(int value) => _stat.SetCurrentHP(value);
    public bool GainMana(int amount) => _stat.AddMana(amount);
    public float DamageReductionPercent => _stat.DamageReductionPercent;

    // === stat modifier ===
    public void AddModifier(Modifier modifier) => _statModifier.AddModifier(modifier);
    public void TickModifiers(float deltaTime) => _statModifier.Tick(deltaTime);



    public HeroDataRuntime(HeroDataSO dataSO)
    {
        _isDummy = dataSO.IsDummy;
        _stat = new Stat(dataSO);
        _statModifier = new StatModifier(_stat);
    }
}
