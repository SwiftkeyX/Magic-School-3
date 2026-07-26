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
    private SpriteRenderer _sprite;
    [SerializeField] private HeroDataSO _SOData;
    private HeroStateMachine _stateMachine;
    private HeroStateMachineBlackBoard _blackboard;

    // ==================== Etc ====================
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // Hump-shaped (0 -> 1 -> 0): drives the attack dash out toward the enemy and back, not a one-way ease like _walkCurve.
    [SerializeField] private AnimationCurve _attackCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

    // ==================== Runtime data ========================
    private HeroDataInCombat _combatData;

    // ==================== getter ====================
    public bool IsInitialized => _combatData != null;
    public Team Team => _blackboard.Team;
    public HeroDataSO Stat => _SOData;
    public HeroStateType State => _stateMachine.CurrentType;
    public HeroStateMachine StateMachine => _stateMachine;
    public HeroStateMachineBlackBoard Blackboard => _blackboard;

    // ==================== setter ====================
    public void SetBoard(BattleBoard battleBoard) => _blackboard.SetBoard(battleBoard);
    public void SetTeam(Team team) => _blackboard.SetTeam(team);

    #region Life Cycle
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _combatData = new HeroDataInCombat(_SOData);
        _blackboard = new HeroStateMachineBlackBoard(this, _combatData, _moveSpeed, _walkCurve, _attackCurve);
        _stateMachine = new HeroStateMachine(this);
    }

    void Start()
    {
        _stateMachine.Start(HeroStateType.Idle);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Combat) return;
        _stateMachine.Update();
    }
    #endregion

    // This is conflict with Hero.cs responsibility BUT I don't know where to put this yet.
    // I'll move it later.
    #region etc
    // When hero die, set his sprite transparent to indicate that he is dead.
    public void SetDeadVisual()
    {
        Color c = _sprite.color;
        c.a = 0.3f;
        _sprite.color = c;
    }
    #endregion

    #region Gizmo
    // draw gize between attacker and receiver to show which hero is attacking.
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !IsInitialized) return;
        if (State != HeroStateType.Attack) return;

        Hero target = _combatData.NearestEnemy;
        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.transform.position);
    }
    #endregion
}
