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
    public enum STATE { IDLE, MOVE, ATTACK, DEAD }

    // ==================== Runtime data ========================
    [SerializeField] private Hex _currentHex;
    private Hex _reservedHex;
    private Coroutine _walkRoutine;
    private Hero _nearestEnemy;
    private int _currentHP;
    private int _currentMana;
    // Last state MoveTowardEnemy() settled on, so other heroes can check "is this ally
    // actually going to move" instead of guessing off a timer. See WorthWaitingForBlocker.
    private STATE _state = STATE.IDLE;
    // Timestamp a "this step doesn't look like progress" hold started, or -1f when not
    // holding. Gives an undecided ally (not yet locked in melee) a short grace period to
    // move out of the way before committing to a step that looks like backing off.
    // Skipped entirely when the blocker is already ATTACK-ing (see WorthWaitingForBlocker)
    // - it won't vacate within this window, so there's nothing to gain by waiting.
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
    public STATE State => _state;

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
        if (_walkRoutine != null)
        {
            _state = STATE.MOVE;
            return STATE.IDLE;
        }

        // Find nearest enemy whereabout
        Hero nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return _state = STATE.IDLE;

        // If there is enemy in the neighbors (adjacent), stop moving, and attacking instead
        if (_currentHex.GetNeighbors().Contains(nearestEnemy.CurrentHex))
        {
            _holdSince = -1f;
            return _state = STATE.ATTACK;
        }

        // If there is ANY enemy that'll walk into my neighbors (adjacent), stop moving, and waiting for him instead
        bool enemyArrivingNextToMe = _board.HeroesOnBoard.Any(h => h.Team != _team && _currentHex.GetNeighbors().Contains(h.ReservedHex));
        if (enemyArrivingNextToMe) return _state = STATE.IDLE;

        // Every other hero's reserved hex is off-limits to path through.
        var reservedHexes = new HashSet<Hex>(_board.HeroesOnBoard.Where(h => h != this).Select(h => h.ReservedHex));

        // Find next hex that could lead this hero to nearest enemy
        Hex targetHex = HexPathfinder.FindValidHexToTarget(_currentHex, nearestEnemy.CurrentHex, reservedHexes);
        if (targetHex == null)
        {
            _holdSince = -1f;
            return _state = STATE.IDLE;
        }

        // If this step doesn't actually get us closer, it's likely because a direct
        // neighbor is currently occupied by an ally who hasn't decided to move yet - give
        // that a short grace period rather than immediately taking a step that looks like
        // backing off. But only if that ally could plausibly still move: one already
        // locked in melee (ATTACK) won't vacate on its own within this window - today
        // because Attack() never resolves, later because a real fight outlasts a single
        // grace period anyway - so waiting on it would be guaranteed wasted time.
        float distFromMeToEnemy = Vector3.Distance(_currentHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        float distFromTargetHexToEnemy = Vector3.Distance(targetHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        if (distFromTargetHexToEnemy >= distFromMeToEnemy && WorthWaitingForBlocker(distFromMeToEnemy, nearestEnemy))
        {
            if (_holdSince < 0f) _holdSince = Time.time;
            if (Time.time - _holdSince < 1f / _moveSpeed) return _state = STATE.IDLE;
        }

        _holdSince = -1f;

        // Reserve the next hex
        _reservedHex = targetHex;

        // Start walking to target hex (adjacent hex)
        _walkRoutine = StartCoroutine(Walk(targetHex));

        return _state = STATE.MOVE;
    }

    // True if at least one hex closer to nearestEnemy than my current spot is occupied by
    // a hero that hasn't given up on moving (anything but ATTACK) - worth a short wait on
    // the chance it steps aside. False if every closer neighbor is either unoccupied
    // (nothing to wait for) or held only by heroes already locked in melee, who won't
    // vacate within a single grace window regardless.
    private bool WorthWaitingForBlocker(float distFromMeToEnemy, Hero nearestEnemy)
    {
        foreach (var neighbor in _currentHex.GetNeighbors())
        {
            float neighborDist = Vector3.Distance(neighbor.transform.position, nearestEnemy.CurrentHex.transform.position);
            if (neighborDist >= distFromMeToEnemy) continue;

            var occupant = _board.HeroesOnBoard.FirstOrDefault(h => h != this && h.ReservedHex == neighbor);
            if (occupant != null && occupant.State != STATE.ATTACK) return true;
        }

        return false;
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
