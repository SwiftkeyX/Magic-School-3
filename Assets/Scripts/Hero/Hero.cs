using UnityEngine;

/// <summary>
/// Hero don't have any logic inside it BUT: 
/// 1) it act like a glue for the entire of hero system.
/// 2) it keep all the variable inside to inject into other hero system.
/// 3) logic allow here is the simple logic for getter & setter.
/// 4) other class that want to use hero logic should reference through Hero only.
/// </summary>
public class Hero : MonoBehaviour
{
    // ==================== Dependency ====================
    [SerializeField] private HeroDataSO _SOData;
    private HeroStateMachine _stateMachine;
    private HeroStateMachineBlackBoard _blackboard;
    private SkillRuntime _skillRuntime;

    // ==================== Etc ====================
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // Hump-shaped (0 -> 1 -> 0): drives the attack dash out toward the enemy and back, not a one-way ease like _walkCurve.
    [SerializeField] private AnimationCurve _attackCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

    // ==================== Runtime data ========================
    private HeroDataRuntime _runtimeData;

    // ==================== getter ====================
    public bool IsInitialized => _runtimeData != null;
    public Team Team => _blackboard.Team;
    public HeroDataSO Stat => _SOData;
    public HeroStateType State => _stateMachine.CurrentType;
    public HeroStateMachine StateMachine => _stateMachine;
    public HeroStateMachineBlackBoard Blackboard => _blackboard;
    public SkillRuntime SkillRuntime => _skillRuntime;

    // ==================== setter ====================
    public void SetBoard(BattleBoard battleBoard) => _blackboard.SetBoard(battleBoard);
    public void SetTeam(Team team) => _blackboard.SetTeam(team);

    #region Life Cycle
    void Awake()
    {
        _runtimeData = new HeroDataRuntime(_SOData);
        _blackboard = new HeroStateMachineBlackBoard(this, _runtimeData, GetComponent<SpriteRenderer>(), _moveSpeed, _walkCurve, _attackCurve);
        _stateMachine = new HeroStateMachine(this);
        _skillRuntime = new SkillRuntime(this, _SOData.Skill);
    }

    void Start()
    {
        _stateMachine.Start(HeroStateType.Idle);
    }

    void Update()
    {
        // if combat not start, return
        if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Combat) return;

        // A hero still sitting on the bench (bought but never dragged onto the board) has no
        // hex yet - running its state machine would crash the moment it tries to path/attack.
        if (!_blackboard.IsInCombat()) return;

        // GameStart fires exactly once, the first Update() this hero is actually in combat on a
        // hex - not in Awake()/Start(), which also run for a hero still sitting on the bench.
        _skillRuntime.EnsureGameStartFired();
        _skillRuntime.Tick(Time.deltaTime);

        _stateMachine.Update();
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
