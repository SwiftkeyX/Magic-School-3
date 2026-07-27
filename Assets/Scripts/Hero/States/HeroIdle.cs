using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Looks for a reason to stop being idle: an adjacent enemy to attack, or a valid step
// toward the nearest enemy to start walking. Also owns the grace-period timer that gives
// an undecided (non-Attack) blocking ally a moment to step aside before committing to a
// longer detour.
public class HeroIdle : HeroState
{
    public override HeroStateType StateType => HeroStateType.Idle;

    // Timestamp a "this step doesn't look like progress" hold started, or -1f when not holding.
    private float _holdSince = -1f;

    public HeroIdle(Hero hero) : base(hero) { }

    public override void OnEnter()
    {
        _holdSince = -1f;
    }

    public override void OnUpdate()
    {
        CheckSwitchState();
    }

    protected override void CheckSwitchState()
    {
        // If hp is below 0, transition to dead, WOW
        bool isMeDead = (_me.Blackboard.GetCurrentHP() <= 0);
        if (isMeDead)
        {
            _me.StateMachine.ChangeState(HeroStateType.Dead);
            return;
        }

        if (_me.Blackboard.IsStunned())
        {
            _me.StateMachine.ChangeState(HeroStateType.Stunned);
            return;
        }

        Hero nearestEnemy = _me.Blackboard.FindNearestEnemy();
        if (nearestEnemy == null) return;

        // If enemy is within attack range, stop moving, and transition to attack state
        if (IsEnemyInAttackRange(nearestEnemy))
        {
            _me.StateMachine.ChangeState(HeroStateType.Attack);
            return;
        }

        // If there is ANY enemy that'll walk into my neighbors (adjacent), stop moving, and wait for him instead
        if (IsEnemyArrivingNextToMe())
        {
            return;
        }

        // Find next hex that could lead this hero to nearest enemy
        Hex targetHex = HexPathfinder.FindValidHexToTarget(_me.Blackboard.GetCurrentHex(), nearestEnemy.Blackboard.GetCurrentHex(), ReservedHexes());
        if (targetHex == null) return;

        // Do I wait for the blocker to move? (Read function's comment)
        if (IsTargetHexMakeMeGoFurtherFromEnemy(nearestEnemy, targetHex))
        {
            return;
        }

        // Finally, I'll decide to walk
        _me.Blackboard.SetReservedHex(targetHex);
        _me.StateMachine.ChangeState(HeroStateType.Walk);
    }


    private bool IsEnemyInAttackRange(Hero nearestEnemy)
    {
        return _me.Blackboard.GetCurrentHex().IsWithinRange(nearestEnemy.Blackboard.GetCurrentHex(), _me.Blackboard.GetRange());
    }

    private bool IsEnemyArrivingNextToMe()
    {
        return _me.Blackboard.Board.HeroesOnBoard.Any(h => h.Team != _me.Team && h.State != HeroStateType.Dead && _me.Blackboard.GetCurrentHex().IsAdjacentTo(h.Blackboard.GetReservedHex()));
    }

    private HashSet<Hex> ReservedHexes()
    {
        // Dead heroes don't hold their hex - otherwise a corpse would block that hex forever.
        return new HashSet<Hex>(_me.Blackboard.Board.HeroesOnBoard.Where(h => h != _me && h.State != HeroStateType.Dead).Select(h => h.Blackboard.GetReservedHex()));
    }

    /// <summary>
    // If the next hex I CAN walk right now actually makes me go further from the nearest enemy, it means: 
    // 1) There's a shorter path but something's blocking it (usually an ally), 
    // so pathfinding gave me the longer route I can take instead.
    // 2) Instead of immediately taking that longer path, wait a moment in case the ally steps aside - but
    // only if it's worth waiting for.
    /// </summary>
    /// <returns = TRUE> I'll wait because I think ally will stop blocking me </returns>
    /// <returns = FALSE> I'll take a longer path </returns>
    private bool IsTargetHexMakeMeGoFurtherFromEnemy(Hero nearestEnemy, Hex targetHex)
    {
        float distFromMeToEnemy = Vector3.Distance(_me.Blackboard.GetCurrentHex().transform.position, nearestEnemy.Blackboard.GetCurrentHex().transform.position);
        float distFromTargetHexToEnemy = Vector3.Distance(targetHex.transform.position, nearestEnemy.Blackboard.GetCurrentHex().transform.position);
        bool nextHexMakeMeFurtherFromEnemy = distFromTargetHexToEnemy >= distFromMeToEnemy;

        if (nextHexMakeMeFurtherFromEnemy && WorthWaitingForBlocker(distFromMeToEnemy, nearestEnemy))
        {
            if (_holdSince < 0f) _holdSince = Time.time;
            if (Time.time - _holdSince < 1f / _me.Blackboard.MoveSpeed) return true;
        }

        return false;
    }

    // If my blocker is not in Attack state, it's worth waiting a moment, since it's likely
    // that ally will step aside soon.
    private bool WorthWaitingForBlocker(float distFromMeToEnemy, Hero nearestEnemy)
    {
        foreach (var neighbor in _me.Blackboard.GetCurrentHex().GetNeighbors())
        {
            float neighborDist = Vector3.Distance(neighbor.transform.position, nearestEnemy.Blackboard.GetCurrentHex().transform.position);
            if (neighborDist >= distFromMeToEnemy) continue;

            var occupant = _me.Blackboard.Board.HeroesOnBoard.FirstOrDefault(h => h != _me && h.State != HeroStateType.Dead && h.Blackboard.GetReservedHex() == neighbor);
            if (occupant != null && occupant.State != HeroStateType.Attack) return true;
        }

        return false;
    }
}
