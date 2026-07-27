using UnityEngine;

// For simplicity, we will use default stat for every hero
[CreateAssetMenu(fileName = "HeroStat", menuName = "Magic School 3/Hero Stat")]
public class HeroDataSO : ScriptableObject
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private string _name = "Hero A";
    [SerializeField] private int _hp = 500;
    [SerializeField] private int _attack = 40;
    [SerializeField] private int _defend = 20;
    [SerializeField] private int _magic = 100;
    [SerializeField] private int _magicResist = 20;
    [SerializeField] private float _attackSpeed = 0.7f;
    [SerializeField] private int _range = 1;
    [SerializeField] private int _startMana = 0;
    [SerializeField] private int _maxMana = 50;
    [SerializeField] private SkillDefinitionSO _skill;


    // ===================== setter & getter =====================
    public GameObject Prefab => _prefab;
    public string Name => _name;
    public int HP => _hp;
    public int Atk => _attack;
    public int DF => _defend;
    public int MG => _magic;
    public int MR => _magicResist;
    public float AttackSpeed => _attackSpeed;
    public int Range => _range;
    public int StartMana => _startMana;
    public int MaxMana => _maxMana;
    public SkillDefinitionSO Skill => _skill;
}