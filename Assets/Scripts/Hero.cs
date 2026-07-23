using System.Collections;
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
    private Coroutine _walkRoutine;
    private Hero _nearestEnemy;
    private int _currentHP;
    private int _currentMana;

    // ==================== setter & getter ====================
    public Hex CurrentHex => _currentHex;
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
        _currentHex.SetOccupant(this);
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
        if (_currentHex.GetNeighbors().Contains(nearestEnemy.CurrentHex)) return STATE.ATTACK;

        Hex targetHex = ClosestNeighborToTarget(nearestEnemy.CurrentHex);
        if (targetHex == null) return STATE.IDLE;

        // If every unoccupied neighbor is farther from the target than where we already are
        // (the direct routes got claimed by allies this same frame), stay put instead of
        // committing to a step backward - retry next frame once occupancy clears.
        float distFromCurrent = Vector3.Distance(_currentHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        float distFromTarget = Vector3.Distance(targetHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        if (distFromTarget >= distFromCurrent) return STATE.IDLE;

        // Clear this hex, Occupy the next hex
        _currentHex.ClearOccupant();
        targetHex.SetOccupant(this);

        // Start walking to target hex (adjacent hex)
        _walkRoutine = StartCoroutine(Walk(targetHex));

        return STATE.MOVE;
    }

    // Find which neighbors is closest to the target
    private Hex ClosestNeighborToTarget(Hex target)
    {
        Hex closest = null;
        float closestDist = float.MaxValue;

        foreach (var neighbor in _currentHex.GetNeighbors())
        {
            if (neighbor.IsOccupied) continue;

            float dist = Vector3.Distance(neighbor.transform.position, target.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = neighbor;
            }
        }

        return closest;
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
