/// <summary>
/// Hold every runtime data for a hero e.g. stat, placement.
/// Runtime data = data that was can be changed in game dynamically. There MUST NOT have static variable here.
///
/// This class should only contain data and its getter & setter only.
/// NO LOGIC.
///
/// It used to also mirror all ~15 of Stat's getters one-for-one, which meant every new stat
/// had to be added here too. It exposes Stat directly now - callers that want a stat ask Stat.
/// </summary>
public class HeroDataRuntime
{
    // ==================================== Description (name, skill desc, etc...) ====================================
    // Temporary, tagged once at its source - see the FIXLATER on HeroDataSO._isDummy.
    private bool _isDummy;

    // ==================================== Stat ====================================
    private Stat _stat;

    // ==================================== Position ====================================
    private Placement _currentPlacement;    // placement hero stand on e.g. hex, benchslot
    private Hex _reservedHex;               // hex that hero reserved. use while battle
    private Hero _nearestEnemy;


    // ==================================== getter ====================================
    // === stat ===
    public Stat Stat => _stat;  // allow blackboard to read its stat directly

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


    public HeroDataRuntime(HeroDataSO dataSO)
    {
        _isDummy = dataSO.IsDummy;
        _stat = new Stat(dataSO);
    }
}
