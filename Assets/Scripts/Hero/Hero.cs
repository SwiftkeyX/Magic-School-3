using UnityEngine;

/// <summary>
/// Hero don't have any logic inside it BUT:
/// 1) It's the ONLY Monobehavior for the Hero, so it's here so we could make hero interact with Unity.
/// 2) it act like a glue, which mean itself don't contain any real logic.
/// </summary>
public class Hero : MonoBehaviour, IDamageable, IStatReadout, IPlaceable, ITargeter
{
    // ======================================== Dependency ========================================
    private HeroDataSO _SOData;
    private HeroStateMachine _stateMachine;
    private SkillSO _skill;
    private BattleBoard _board;
    private FindEnemy _findEnemy;
    private BlackboardTemp _temp;

    // ======================================== Etc ========================================
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // Hump-shaped (0 -> 1 -> 0): drives the attack dash out toward the enemy and back, not a one-way ease like _walkCurve.
    [SerializeField] private AnimationCurve _attackCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

    // ======================================== Runtime data ========================================
    private HeroDataRuntime _runtimeData;
    private Team _team;
    private Stat Stat => _runtimeData.Stat;

    // ======================================== getter ========================================
    public bool IsInitialized => _runtimeData != null;
    public Team Team => _team;
    public HeroStateType State => _stateMachine.CurrentType;
    public HeroStateMachine StateMachine => _stateMachine;
    public BattleBoard Board => _board;
    public BlackboardTemp Temp => _temp;    // Grab-bag for logic that doesn't have an obvious home yet - see BlackboardTemp for why.
    public bool IsDummy => _runtimeData.IsDummy;

    // ======================================== interface method ========================================
    // === IDamageable ===
    public bool IsAlive => this != null && IsInitialized && State != HeroStateType.Dead;
    public void AddModifier(Modifier modifier) => Stat.AddModifier(modifier);

    public void Heal(float amount)
    {
        int healed = CombatMath.Heal(amount, Stat.CurrentHP, Stat.IsWounded);
        Stat.SetCurrentHP(healed);
    }

    public void TakeDamage(int damage)
    {
        int newHP = CombatMath.TakeDamage(damage, Stat.DF, Stat.DamageReductionPercent, Stat.CurrentHP);
        Stat.SetCurrentHP(newHP);
    }

    // === IStatReadout ===
    public int CurrentHP => Stat.CurrentHP;
    public int MaxHP => Stat.HP;
    public int CurrentMana => Stat.CurrentMana;
    public int MaxMana => Stat.MaxMana;

    // === IPlaceable ===
    public Hex CurrentHex => _runtimeData.CurrentPlacement as Hex;
    public Hex ReservedHex => _runtimeData.ReservedHex;
    public Placement CurrentPlacement => _runtimeData.CurrentPlacement;
    public bool IsInCombat => _runtimeData.CurrentPlacement is Hex;
    public void SetReservedHex(Hex hex) => _runtimeData.SetReservedHex(hex);
    public void SetCurrentPlacement(Placement placement) => _runtimeData.SetCurrentPlacement(placement);

    // === ITargeter ===
    public Hero FindNearestEnemy() => _findEnemy.FindNearestEnemy();
    public Hero FindFurthestEnemy() => _findEnemy.FindFurthestEnemy();

    // ======================================== stat ========================================
    // ASKING: Hey, this should be IStatReadout too, no?
    public int AttackDamage => Stat.Atk;
    public float AttackSpeed => Stat.AttackSpeed;
    public int Range => Stat.Range;
    public bool IsStunned => Stat.IsStunned;
    public bool IsWounded => Stat.IsWounded;

    public bool GainMana(int amount) => Stat.AddMana(amount);      // return true if mana if capped
    public void TickModifiers(float deltaTime) => Stat.TickModifiers(deltaTime);

    // ======================================== setup wiring ========================================
    public void SetBoard(BattleBoard board)
    {
        _board = board;
        _findEnemy.SetBoard(board);     // UNSURE: This is kinda awkward. I want findEnemy to use the exact same ref to battle board. (SingleSourceTruth)
    }

    public void SetTeam(Team team) => _team = team;

    #region Life Cycle
    public void Init(HeroDataSO data)
    {
        _SOData = data;
        _runtimeData = new HeroDataRuntime(_SOData);
        _temp = new BlackboardTemp(this, GetComponent<SpriteRenderer>());
        _findEnemy = new FindEnemy(this, _runtimeData);
        _skill = _SOData.Skill;
        _stateMachine = new HeroStateMachine(this, _skill, new MovementConfig(_moveSpeed, _walkCurve, _attackCurve));
    }

    void Start()
    {
        _stateMachine.Start(HeroStateType.Idle);
    }

    void Update()
    {
        // if combat not start, return
        if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Combat) return;

        // Some hero are not on BattleBoard but was in the bench. They don't consider in combat.
        if (!IsInCombat) return;

        TickModifiers(Time.deltaTime);

        _stateMachine.Tick();
    }
    #endregion

    #region Gizmo
    // draw gize between attacker and receiver to show which hero is attacking.
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !IsInitialized) return;
        if (State != HeroStateType.Attack) return;

        Hero target = _runtimeData.NearestEnemy;
        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.transform.position);
    }
    #endregion
}
