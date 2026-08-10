
using UnityEngine;

namespace MagicSchool
{

    public abstract class HeroState
    {
        protected readonly Hero _me;
        protected readonly Transition _transition;

        protected HeroState(Hero hero)
        {
            _me = hero;
            _transition = new Transition(_me);
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
        public Transition(Hero me)
        {
            _me = me;
        }

        public bool TryAttack(ICombatant nearestEnemy)
        {
            // If enemy is within attack range, transition to attack state
            if (IsEnemyInAttackRange(nearestEnemy)) return true;

            return false;
        }

        public bool TryWalk(ICombatant nearestEnemy)
        {
            // If there is ANY enemy that'll walk into my neighbors (adjacent), stop moving, and wait for him instead, transition to idle
            if (IsEnemyArrivingNextToMe()) return false;

            // read this function's comment
            if (IsTargetHexMakeMeGoFurtherFromEnemy(nearestEnemy)) return false;

            // finally, transition to walk
            return true;
        }

        // =======================================

        private bool IsEnemyInAttackRange(ICombatant nearestEnemy)
        {
            return _me.CurrentHex.IsWithinRange(nearestEnemy.CurrentHex, _me.Range);
        }

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
        private bool IsTargetHexMakeMeGoFurtherFromEnemy(ICombatant nearestEnemy)
        {
            // Find next hex that could lead this hero to nearest enemy
            Hex targetHex = HexPathfinder.FindValidHexToTarget(_me.CurrentHex, nearestEnemy.CurrentHex);
            if (targetHex == null) return false;

            // Check is target hex make me go further from enemy
            float distFromMeToEnemy = Vector3.Distance(_me.CurrentHex.transform.position, nearestEnemy.CurrentHex.transform.position);
            float distFromTargetHexToEnemy = Vector3.Distance(targetHex.transform.position, nearestEnemy.CurrentHex.transform.position);
            bool nextHexMakeMeFurtherFromEnemy = distFromTargetHexToEnemy >= distFromMeToEnemy;

            if (nextHexMakeMeFurtherFromEnemy && WorthWaitingForBlocker(distFromMeToEnemy, nearestEnemy))
            {
                if (_holdSince < 0f) _holdSince = Time.time;
                if (Time.time - _holdSince < 1f / movement.MoveSpeed) return true;
            }

            return false;
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
