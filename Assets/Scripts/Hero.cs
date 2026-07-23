using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hero : MonoBehaviour
{
    // ==================== Dependency ====================
    // Hero need to have board ref, in order to move toward the enemy, without it hero don't know enemy whereabout
    private BattleBoard _board;
    private SpriteRenderer _sprite;
    private HeroDataSO _data;
    // private HeroDataRuntime _runtimeData;


    // ==================== Etc ====================
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private Team _team;
    private enum STATE { IDLE, MOVE, ATTACK, DEAD }

    // ==================== Runtime data ========================
    [SerializeField] private Hex _currentHex;
    private Hex _reservedHex;
    private Coroutine _walkRoutine;
    private Hero _nearestEnemy;
    private int _currentHP;
    private int _currentMana;
    // Timestamp a "this step doesn't look like progress" hold started, or -1f when not
    // holding. Gives momentary contention (allies about to move out of the way) a short
    // grace period before committing to a step that looks like backing off - but only a
    // grace period, so a permanently blocked route (e.g. an ally stuck in melee forever)
    // still eventually gets routed around instead of freezing the hero in place forever.
    private float _holdSince = -1f;

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
    }

    void Start()
    {
    }

    void Update()
    {
        // Find nearest enemy whereabout
        Hero nearestEnemy = FindNearestEnemy();

        STATE state = MoveTowardEnemy();
        if (state == STATE.ATTACK) Attack();
    }
    #endregion

    /// <summary>
    /// Movement in this game, hero can only move to adjacent hex at a time.
    /// Hero want to move toward nearest enemy, stop moving once enemy is in attack range.
    /// </summary>
    #region Movement
    // Walks toward the nearest enemy, one hex at a time. Stops once already adjacent to a enemy.
    private STATE MoveTowardEnemy()
    {
        if (_walkRoutine != null) return STATE.IDLE;

        // Find nearest enemy whereabout
        Hero nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return STATE.IDLE;

        // If there is enemy in the neighbors (adjacent), stop moving, and attacking instead
        if (_currentHex.GetNeighbors().Contains(nearestEnemy.CurrentHex))
        {
            _holdSince = -1f;
            return STATE.ATTACK;
        }

        // If there is enemy that'll walk into my neighbors (adjacent), stop moving, and waiting for him instead
        if (_currentHex.GetNeighbors().Contains(nearestEnemy.ReservedHex)) return STATE.IDLE;

        // Every other hero's reserved hex is off-limits to path through.
        var reservedHexes = new HashSet<Hex>(_board.HeroesOnBoard.Where(h => h != this).Select(h => h.ReservedHex));

        // Find next hex that could lead this hero to nearest enemy
        Hex targetHex = HexPathfinder.FindValidHexToTarget(_currentHex, nearestEnemy.CurrentHex, reservedHexes);
        if (targetHex == null)
        {
            _holdSince = -1f;
            return STATE.IDLE;
        }

        // If this step doesn't actually get us closer, it's likely because a direct
        // neighbor is only momentarily blocked by an ally who's about to move out of the
        // way - give that a short grace period rather than immediately taking a step that
        // looks like backing off. But don't hold forever: if the block hasn't cleared by
        // the time we'd normally finish a step, commit anyway - it may be a permanently
        // blocked route (e.g. an ally stuck in melee) that genuinely needs the detour.
        float distFromCurrent = Vector3.Distance(_currentHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        float distFromTarget = Vector3.Distance(targetHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        if (distFromTarget >= distFromCurrent)
        {
            if (_holdSince < 0f) _holdSince = Time.time;
            if (Time.time - _holdSince < 1f / _moveSpeed) return STATE.IDLE;
        }

        _holdSince = -1f;

        // Reserve the next hex
        _reservedHex = targetHex;

        // Start walking to target hex (adjacent hex)
        _walkRoutine = StartCoroutine(Walk(targetHex));

        return STATE.MOVE;
    }

    // hero walk to target hex
    private IEnumerator Walk(Hex targetHex)
    {
        Vector3 start = transform.position;
        Vector3 end = targetHex.transform.position;
        float duration = 1f / _moveSpeed;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float easedT = _walkCurve.Evaluate(t / duration);
            transform.position = Vector3.Lerp(start, end, easedT);
            yield return null;
        }

        transform.position = end;
        _currentHex = targetHex;
        _walkRoutine = null;
    }
    #endregion

    private Hero FindNearestEnemy()
    {
        Hero nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var target in _board.HeroesOnBoard)
        {
            if (target == this || target.Team == _team) continue;

            float dist = Vector3.Distance(_currentHex.transform.position, target.CurrentHex.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = target;
            }
        }

        return nearest;
    }

    void Attack()
    {
        Hero target = FindNearestEnemy();

    }
}
