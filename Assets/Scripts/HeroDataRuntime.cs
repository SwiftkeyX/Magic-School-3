// Not use yet, but at some point in the project
// we'll have to separate SO and runtime data apart.
public class HeroDataRuntime
{
    private string _name;
    private int _hp;
    private int _attack;
    private int _defend;
    private int _magic;
    private int _magicResist;
    private float _attackSpeed;
    private int _startMana;
    private int _maxMana;


    // ===================== setter & getter =====================
    public string Name => _name;
    public int HP => _hp;
    public int Atk => _attack;
    public int DF => _defend;
    public int MG => _magic;
    public int MR => _magicResist;
    public float AttackSpeed => _attackSpeed;
    public int StartMana => _startMana;
    public int MaxMana => _maxMana;
}