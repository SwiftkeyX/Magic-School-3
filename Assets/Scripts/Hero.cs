using System.Linq;
using UnityEngine;

public class Hero : MonoBehaviour
{
    // ==================== Dependency ====================
    // Hero need to have board ref, in order to move toward the enemy, without it hero don't know enemy whereabout
    private BattleBoard _board;
    private SpriteRenderer _sprite;
    private HeroDataSO _data;

    // ==================== Etc ====================
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private Team _team;

    // ==================== Runtime data ========================
    [SerializeField] private Hex _currentHex;
    private Hex _reservedHex;
    private Hero _nearestEnemy;
    private int _currentHP;
    private int _currentMana;
    private HeroStateMachine _stateMachine;

    // ==================== setter & getter ====================
    public Hex CurrentHex => _currentHex;
    // The hex this hero has claimed - same as CurrentHex while idle, but already pointing
    // at the destination the instant a step is committed, well before the walk animation
    // finishes and CurrentHex catches up. This is the single source of truth for "who's
    // standing where" - Hex itself doesn't track occupancy, heroes do.
    public Hex ReservedHex => _reservedHex;
    public Team Team => _team;
    public HeroDataSO Stat => _data;
    public int CurrentHP => _currentHP;
    public int CurrentMana => _currentMana;
    public HeroStateType State => _stateMachine.CurrentType;

    // ==================== state machine plumbing ====================
    // Exposed so the HeroState family (HeroIdle, HeroWalk, HeroAttack, HeroDead) can read
    // board/movement data and drive transitions - not meant for use outside of it.
    internal BattleBoard Board => _board;
    internal float MoveSpeed => _moveSpeed;
    internal AnimationCurve WalkCurve => _walkCurve;
    internal HeroStateMachine StateMachine => _stateMachine;

    internal void SetCurrentHex(Hex hex) => _currentHex = hex;

    // Commits to a step: reserves the destination the instant it's decided (before the
    // walk animation even starts), then hands off to HeroWalk.
    internal void BeginWalkTo(Hex targetHex)
    {
        _reservedHex = targetHex;
        _stateMachine.Walk.SetTarget(targetHex);
        _stateMachine.ChangeState(_stateMachine.Walk);
    }

    #region Setup
    public void SetBoard(BattleBoard board)
    {
        _board = board;
    }

    // To make hero teleport to their hex and occupy it
    public void Init(Hex startingHex, Team team, HeroDataSO stat)
    {
        // move hero to target hex, occupy that hex
        _currentHex = startingHex;
        _reservedHex = startingHex;
        transform.position = _currentHex.transform.position;

        // set sprite's color for each team
        _team = team;
        if (_team == Team.Blue) _sprite.color = Color.blue;
        else if (_team == Team.Red) _sprite.color = Color.red;

        // initialzie stat
        _data = stat;

        // set up runtime stat from this hero's own data
        _currentHP = _data.HP;
        _currentMana = _data.StartMana;
    }
    #endregion

    #region Life Cycle
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _stateMachine = new HeroStateMachine(this);
    }

    void Start()
    {
        _stateMachine.Start(_stateMachine.Idle);
    }

    void Update()
    {
        _stateMachine.Update();
    }
    #endregion

    // How close two enemies' distances have to be to count as tied. Needed because two
    // geometrically-equal distances can still differ by a hair of floating-point error.
    private const float NearestEnemyTieEpsilon = 0.01f;

    // Picks nearest enemy (if there are several nearest enemies, random it). Sticks with
    // the previous pick across calls as long as it's still tied for nearest, so the target
    // doesn't flicker between equally-near enemies frame to frame.
    internal Hero FindNearestEnemy()
    {
        var enemyDistances = _board.HeroesOnBoard
            .Where(target => target != this && target.Team != _team)
            .Select(target => new { target, dist = Vector3.Distance(_currentHex.transform.position, target.CurrentHex.transform.position) })
            .ToList();

        if (enemyDistances.Count == 0) return null;

        float nearestDist = enemyDistances.Min(e => e.dist);
        var tiedNearest = enemyDistances.Where(e => e.dist <= nearestDist + NearestEnemyTieEpsilon).Select(e => e.target).ToList();

        if (_nearestEnemy != null && tiedNearest.Contains(_nearestEnemy)) return _nearestEnemy;

        _nearestEnemy = tiedNearest[Random.Range(0, tiedNearest.Count)];
        return _nearestEnemy;
    }
}
