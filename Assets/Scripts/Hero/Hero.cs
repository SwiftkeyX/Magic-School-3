using UnityEngine;

/// <summary>
/// Hero don't have any logic inside it BUT: 
/// 1) It's the ONLY Monobehavior for the Hero, so it's here so we could make hero interact with Unity.
/// 2) it act like a glue, which mean itself don't contain any real logic.
/// 
/// ASKING: I think Hero and HeroStateMachineBlackBoard are doing the same thing. They are both the glue that have no logic inside. 
/// And initial reason for HeroStateMachineBlackBoard are to separate thing for readability.
/// which is good BUT it didn't separate well enough that they both still doing the same thing.
/// I propose to combine them for now. And break it down later when appropriate. Right now it only add to the confusion that how those two different? when they aren't.
/// </summary>
public class Hero : MonoBehaviour, IDamageable, IStatReadout, IPlaceable, ITargeter
{
    // ======================================== Dependency ========================================
    private HeroDataSO _SOData;
    private HeroStateMachine _stateMachine;
    private HeroStateMachineBlackBoard _blackboard;
    private SkillSO _skill;

    // ======================================== Etc ========================================
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // Hump-shaped (0 -> 1 -> 0): drives the attack dash out toward the enemy and back, not a one-way ease like _walkCurve.
    [SerializeField] private AnimationCurve _attackCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

    // ======================================== Runtime data ========================================
    private HeroDataRuntime _runtimeData;

    // ======================================== getter ========================================
    public bool IsInitialized => _runtimeData != null;
    public Team Team => _blackboard.Team;
    public HeroStateType State => _stateMachine.CurrentType;
    public HeroStateMachine StateMachine => _stateMachine;

    // ======================================== interface method ========================================
    // === IDamageable ===
    public void TakeDamage(int damage) => _blackboard.TakeDamage(damage);
    public void Heal(float amount) => _blackboard.Heal(amount);
    public void AddModifier(Modifier modifier) => _blackboard.AddModifier(modifier);
    public bool IsAlive => this != null && IsInitialized && State != HeroStateType.Dead;

    // === IStatReadout ===
    public int CurrentHP => _blackboard.GetCurrentHP();
    public int MaxHP => _blackboard.GetMaxHP();
    public int CurrentMana => _blackboard.GetCurrentMana();
    public int MaxMana => _blackboard.GetMaxMana();

    // === IPlaceable ===
    public Hex CurrentHex => _blackboard.GetCurrentHex();
    public Hex ReservedHex => _blackboard.GetReservedHex();
    public Placement CurrentPlacement => _blackboard.CurrentPlacement();
    public bool IsInCombat => _blackboard.IsInCombat();
    public void SetReservedHex(Hex hex) => _blackboard.SetReservedHex(hex);
    public void SetCurrentPlacement(Placement placement) => _blackboard.SetCurrentPlacement(placement);

    // === ITargeter ===
    public Hero FindNearestEnemy() => _blackboard.FindNearestEnemy();
    public Hero FindFurthestEnemy() => _blackboard.FindFurthestEnemy();

    // ======================================== setup wiring ========================================
    // Called by Preparation once the hero exists but before it can act - not part of any
    // interface above, since this is lifecycle wiring rather than a role the hero plays.
    public void SetBoard(BattleBoard board) => _blackboard.SetBoard(board);
    public void SetTeam(Team team) => _blackboard.SetTeam(team);

    #region Life Cycle
    public void Init(HeroDataSO data)
    {
        _SOData = data;
        _runtimeData = new HeroDataRuntime(_SOData);
        _blackboard = new HeroStateMachineBlackBoard(this, _runtimeData, GetComponent<SpriteRenderer>());
        _skill = _SOData.Skill;
        _stateMachine = new HeroStateMachine(this, _blackboard, _skill, new MovementConfig(_moveSpeed, _walkCurve, _attackCurve));
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
        if (!_blackboard.IsInCombat()) return;

        _blackboard.TickModifiers(Time.deltaTime);

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
