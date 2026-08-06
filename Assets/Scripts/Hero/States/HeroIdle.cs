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

    private readonly MovementConfig _movement;

    public HeroIdle(Hero hero, HeroStateMachineBlackBoard blackboard, SkillSO skill, MovementConfig movement) : base(hero, blackboard, skill)
    {
        _movement = movement;
    }

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
        bool isMeDead = (_blackboard.GetCurrentHP() <= 0);
        if (isMeDead)
        {
            _me.StateMachine.ChangeState(HeroStateType.Dead);
            return;
        }

        if (_blackboard.IsStunned())
        {
            _me.StateMachine.ChangeState(HeroStateType.Stunned);
            return;
        }

        // TODO: This is temporarily. I want the dummy to be its own unit later (Not a hero).
        // Dummy never walks or attacks - it just stands there to be a target.
        if (_blackboard.IsDummy) return;

        Hero nearestEnemy = _blackboard.FindNearestEnemy();
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
        Hex targetHex = HexPathfinder.FindValidHexToTarget(_blackboard.GetCurrentHex(), nearestEnemy.CurrentHex, ReservedHexes());
        if (targetHex == null) return;

        // Do I wait for the blocker to move? (Read function's comment)
        if (IsTargetHexMakeMeGoFurtherFromEnemy(nearestEnemy, targetHex))
        {
            return;
        }

        // Finally, I'll decide to walk
        _blackboard.SetReservedHex(targetHex);
        _me.StateMachine.ChangeState(HeroStateType.Walk);
    }


    private bool IsEnemyInAttackRange(Hero nearestEnemy)
    {
        return _blackboard.GetCurrentHex().IsWithinRange(nearestEnemy.CurrentHex, _blackboard.GetRange());
    }

    private bool IsEnemyArrivingNextToMe()
    {
        return _blackboard.Board.HeroesOnBoard.Any(h => h.Team != _me.Team && h.State != HeroStateType.Dead && _blackboard.GetCurrentHex().IsAdjacentTo(h.ReservedHex));
    }

    private HashSet<Hex> ReservedHexes()
    {
        // Dead heroes don't hold their hex - otherwise a corpse would block that hex forever.
        return new HashSet<Hex>(_blackboard.Board.HeroesOnBoard.Where(h => h != _me && h.State != HeroStateType.Dead).Select(h => h.ReservedHex));
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
        float distFromMeToEnemy = Vector3.Distance(_blackboard.GetCurrentHex().transform.position, nearestEnemy.CurrentHex.transform.position);
        float distFromTargetHexToEnemy = Vector3.Distance(targetHex.transform.position, nearestEnemy.CurrentHex.transform.position);
        bool nextHexMakeMeFurtherFromEnemy = distFromTargetHexToEnemy >= distFromMeToEnemy;

        if (nextHexMakeMeFurtherFromEnemy && WorthWaitingForBlocker(distFromMeToEnemy, nearestEnemy))
        {
            if (_holdSince < 0f) _holdSince = Time.time;
            if (Time.time - _holdSince < 1f / _movement.MoveSpeed) return true;
        }

        return false;
    }

    // If my blocker is not in Attack state, it's worth waiting a moment, since it's likely
    // that ally will step aside soon.
    private bool WorthWaitingForBlocker(float distFromMeToEnemy, Hero nearestEnemy)
    {
        foreach (var neighbor in _blackboard.GetCurrentHex().GetNeighbors())
        {
            float neighborDist = Vector3.Distance(neighbor.transform.position, nearestEnemy.CurrentHex.transform.position);
            if (neighborDist >= distFromMeToEnemy) continue;

            var occupant = _blackboard.Board.HeroesOnBoard.FirstOrDefault(h => h != _me && h.State != HeroStateType.Dead && h.ReservedHex == neighbor);
            if (occupant != null && occupant.State != HeroStateType.Attack) return true;
        }

        return false;
    }
}
