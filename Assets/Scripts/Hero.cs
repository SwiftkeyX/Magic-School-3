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
    private STATE _state = STATE.IDLE;
    private float _holdSince = -1f;

    // ==================== setter & getter ====================
    public Hex CurrentHex => _currentHex;
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

        // Read the function comment
        if (IsTargetHexMakeMeGoFurtherFromEnemy(nearestEnemy, targetHex)) return _state = STATE.IDLE;

        _holdSince = -1f;

        // Reserve the next hex
        _reservedHex = targetHex;

        // Start walking to target hex (adjacent hex)
        _walkRoutine = StartCoroutine(Walk(targetHex));

        return _state = STATE.MOVE;
    }

    // If my blocker is not in attack state, it's worth waiting a moment, since it's likely that ally will step aside soon.
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

    /// <summary>
    // If the next hex I CAN walk right now, ACTUALLY make me go further from that nearest enemy. It mean:
    // 1) There is a shortest path to that enemy BUT I can't take that path right now because something is blocking me (usually a ally).
    // So the PathFinding algo told me the longer path that I can also used.  
    // 2) Instead of immediately take a longer path, I'll wait a moment in case my ally step out of the way
    // So I can take a shortest path.
    // 2.1) BUT it's not worth waiting for me, if ally that's blocking me is in Attack state, Because he surely won't move soon. 
    /// </summary>
    /// <returns value=TRUE> I'll wait a moment </returns>
    /// <returns value=TRUE> No wait, I'll take a long path </returns>
    private bool IsTargetHexMakeMeGoFurtherFromEnemy(Hero nearestEnemy, Hex targetHex)
    {
        float distFromMeToEnemy = Vector3.Distance(_currentHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        float distFromTargetHexToEnemy = Vector3.Distance(targetHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        bool nextHexMakeMeFurtherFromEnemy = distFromTargetHexToEnemy >= distFromMeToEnemy;
        if (nextHexMakeMeFurtherFromEnemy && WorthWaitingForBlocker(distFromMeToEnemy, nearestEnemy))
        {
            if (_holdSince < 0f) _holdSince = Time.time;
            if (Time.time - _holdSince < 1f / _moveSpeed) return true;
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

    // How close two enemies' distances have to be to count as tied. Needed because two
    // geometrically-equal distances can still differ by a hair of floating-point error.
    private const float NearestEnemyTieEpsilon = 0.01f;

    // Picks nearest enemy (If there is several nearest enemy, random it).
    private Hero FindNearestEnemy()
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
 
    void Attack()
    {
        Hero target = FindNearestEnemy();

    }
}
