using System.Collections;
using UnityEngine;

public class Hero : MonoBehaviour
{
    // ==================== Dependency ====================
    // Hero need to have board ref, in order to move toward the enemy, without it hero don't know enemy whereabout
    private BattleBoard _board;

    // ==================== SerializeField ====================
    [SerializeField] private float _moveSpeed = 1f; 
    [SerializeField] private Hex _currentHex;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ==================== Etc ====================
    private Team _team;
    private Coroutine _walkRoutine;
    // ==================== setter & getter ====================
    public Hex CurrentHex => _currentHex;
    public Team Team => _team;

    #region Setup
    public void SetBoard(BattleBoard board)
    {
        _board = board;
    }

    // Init when hero spawn, because he need to teleport to his hex
    public void Init(Hex startingHex, Team team)
    {
        _currentHex = startingHex;
        _team = team;
        startingHex.SetOccupant(this);
        transform.position = startingHex.transform.position;
    }
    #endregion

    #region Life Cycle
    void Start()
    {
        if (_currentHex != null)
            transform.position = _currentHex.transform.position;
    }

    void Update()
    {
        Debug.Log("currentHex: " + _currentHex);
        MoveTowardEnemy();
    }
    #endregion

    /// <summary>
    /// Movement in this game, hero can only move to adjacent hex at a time.
    /// Hero want to move toward nearest enemy, stop moving once enemy is in attack range.
    /// </summary>
    #region Movement
    // Walks toward the nearest enemy, one hex at a time. Stops once already adjacent to a enemy.
    private void MoveTowardEnemy()
    {
        if (_walkRoutine != null) return;

        // Find nearest enemy whereabout
        Hero nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        // If there is enemy in the neighbors (adjacent), stop moving
        if (_currentHex.GetNeighbors().Contains(nearestEnemy.CurrentHex))
            return;

        Hex targetHex = ClosestNeighborToTarget(nearestEnemy.CurrentHex);
        if (targetHex == null) return;

        // Reserve the destination and release the current hex now, since we're committing to leave -
        // otherwise another hero deciding this same frame could also target the same free hex.
        _currentHex.ClearOccupant();
        targetHex.SetOccupant(this);

        _walkRoutine = StartCoroutine(Walk(targetHex));
    }

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

}
