using System;
using UnityEngine;

namespace MagicSchool
{

    public abstract class HeroState
    {
        protected readonly Hero _me;
        protected readonly Transition _transition;

        protected HeroState(Hero hero, Transition transition)
        {
            _me = hero;
            _transition = transition;
        }

        public abstract HeroStateType StateType { get; }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public abstract void OnUpdate();
        protected abstract void CheckSwitchState();

    }

    // share transition condition for statemachine
    public class Transition
    {
        private readonly Hero _me;

        public Transition(Hero me, MovementConfig movement)
        {
            _me = me;
            _movement = movement;

            _isHexBlocked = hex => _me.IsHexReservedByOther(hex);
        }

        // ======================================== transition condition ========================================
        // if enemy is in attack range, not dead, transition to attack 
        public bool CanAttack(ICombatant nearestEnemy)
        {
            return nearestEnemy != null && nearestEnemy.IsAlive & _me.CurrentHex.IsWithinRange(nearestEnemy.CurrentHex, _me.Range);
        }

        // walk condition, read function's comment
        // true = transition to walk, false = transition to idle
        private readonly MovementConfig _movement;
        private readonly Func<Hex, bool> _isHexBlocked;
        private float _holdSince = -1f;
        private Hex _targetHex;
        public Hex GetReservedHex() => _targetHex;
        public bool CanWalk(ICombatant nearestEnemy)
        {
            // reset target hex
            _targetHex = null;

            // If there is ANY enemy that'll walk into my neighbors (adjacent), stop moving, and wait for him instead
            if (IsEnemyArrivingNextToMe()) return false;

            // Find next hex that could lead this hero toward nearest enemy
            _targetHex = HexPathfinder.FindValidHexToTarget(_me.CurrentHex, nearestEnemy.CurrentHex, _isHexBlocked);
            if (_targetHex == null) return false;

            // Do I wait for the blocker to move? (Read function's comment)
            if (ShouldWaitForBlocker(nearestEnemy, _targetHex))
            {
                _targetHex = null;
                return false;
            }

            // finally, walk, reset the hold
            _holdSince = -1f;
            return true;
        }

        // ======================================== private ========================================
        // check if my neighbor was reserved by enemy
        private bool IsEnemyArrivingNextToMe()
        {
            foreach (var neighbor in _me.CurrentHex.GetNeighbors())
            {
                ICombatant reserver = _me.WhoReservedThisHex(neighbor);
                if (reserver != null && reserver.Team != _me.Team) return true;
            }

            return false;
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
        private bool ShouldWaitForBlocker(ICombatant nearestEnemy, Hex targetHex)
        {
            float distFromMeToEnemy = Vector3.Distance(_me.CurrentHex.transform.position, nearestEnemy.CurrentHex.transform.position);
            float distFromTargetHexToEnemy = Vector3.Distance(targetHex.transform.position, nearestEnemy.CurrentHex.transform.position);
            bool nextHexMakeMeFurtherFromEnemy = distFromTargetHexToEnemy >= distFromMeToEnemy;

            // if the next Hex make me closer to enemy, don't wait
            // OR if the blocker isn't worth waiting for, don't wait 
            if (!nextHexMakeMeFurtherFromEnemy || !WorthWaitingForBlocker(distFromMeToEnemy, nearestEnemy))
            {
                _holdSince = -1f;
                return false;
            }

            // hold, but only for a amount of time (amount of time that a step would take)
            if (_holdSince < 0f) _holdSince = Time.time;
            return Time.time - _holdSince < 1f / _movement.MoveSpeed;
        }

        // If my blocker is not in these state, it's worth waiting a moment, since it's likely that ally will step aside soon.
        // these state = [attack, cast]
        private bool WorthWaitingForBlocker(float distFromMeToEnemy, ICombatant nearestEnemy)
        {
            foreach (var neighbor in _me.CurrentHex.GetNeighbors())
            {
                float neighborDist = Vector3.Distance(neighbor.transform.position, nearestEnemy.CurrentHex.transform.position);
                if (neighborDist >= distFromMeToEnemy) continue;

                ICombatant occupant = _me.WhoReservedThisHex(neighbor);
                if (occupant == null || occupant == _me as ICombatant) continue;

                // if blocker is in attack or cast state, wait for him.
                bool isCommitted = occupant.StateType == HeroStateType.Attack || occupant.StateType == HeroStateType.Cast;
                if (!isCommitted) return true;
            }

            return false;
        }

    }
}
