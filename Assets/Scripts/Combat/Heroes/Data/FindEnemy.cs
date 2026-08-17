using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Combat.Placements;

namespace MagicSchool.Combat.Heroes
{
    // Scans the board for a hero's nearest/furthest/most-clustered living enemy.
    public class FindEnemy
    {
        private const float TieEpsilon = 0.01f;     // How close two enemies' distances have to be to count as tied.

        private readonly Hero _me;
        private readonly BattleBoard _board;

        private List<ICombatant> _enemyBFSCache;
        private int _enemyBFSCacheFrame = -1;

        private List<(ICombatant target, float dist)> _enemyDistanceCache;
        private int _enemyDistanceCacheFrame = -1;

        private ICombatant _lastNearest;
        private ICombatant _currentTarget;
        internal ICombatant CurrentTarget => _currentTarget;

        public FindEnemy(Hero me, BattleBoard board)
        {
            _me = me;
            _board = board;
        }

        // ========================================= public ========================================= 

        // When a hero auto attack a enemy, he'll pick that enemy as current target. 
        // Which mean he'll stick to that target until something break them off.
        // read IsStillEngageWith() for more detail. 
        public ICombatant FindCurrentTarget()
        {
            if (IsStillEngagedWith(_currentTarget)) return _currentTarget;

            // if current target is break off, find new target.
            ICombatant fresh = FindNearestEnemy();
            _currentTarget = fresh;

            return fresh;
        }

        // Picks nearest enemy
        public ICombatant FindNearestEnemy()
        {
            var enemyDistances = GetEnemyDistance();

            if (enemyDistances.Count == 0) return null;

            float nearestDist = enemyDistances.Min(e => e.dist);
            var tiedNearest = enemyDistances.Where(e => e.dist <= nearestDist + TieEpsilon).Select(e => e.target).ToList();

            // Get last answer, or the engaged target if there isn't one yet
            ICombatant preferred = _lastNearest ?? _currentTarget;

            // read function comment
            ICombatant nearestEnemy = BreakTie(tiedNearest, preferred);
            _lastNearest = nearestEnemy;

            return nearestEnemy;
        }


        // Picks furthest enemy
        public ICombatant FindFurthestEnemy()
        {
            var enemyDistances = GetEnemyDistance();

            if (enemyDistances.Count == 0) return null;

            float furthestDist = enemyDistances.Max(e => e.dist);
            var tiedFurthest = enemyDistances.Where(e => e.dist >= furthestDist - TieEpsilon).Select(e => e.target).ToList();

            return BreakTie(tiedFurthest, _currentTarget);
        }

        // Pick the hex that are most cluster (measuring by input radius)
        // FLAGGING: This one use IsWithInRange() instead of checking distance like the others. This isn't test yet.
        // FIXLATER: This logic also doesn't quite right. If we don't care about performance, 
        // we can check every single hex, and enemy in its radius, that one would work right.
        public ICombatant FindClusteredCircle(int radius = 2)
        {
            List<ICombatant> enemies = GetEnemiesBFS();
            if (enemies.Count == 0) return null;

            List<ICombatant> best = new List<ICombatant>();
            int bestCount = -1;

            // look at each candidate enemy, look [x] hex radius from the candidate, how many enemy is with in the radius?
            foreach (ICombatant candidate in enemies)
            {
                Hex candidateHex = candidate.CurrentHex();
                int count = enemies.Count(other => other != candidate && candidateHex.IsWithinRange(other.CurrentHex(), radius));

                // found better candidate
                if (count > bestCount) { bestCount = count; best.Clear(); }

                // keep every candidate that ties for best
                if (count == bestCount) best.Add(candidate);
            }

            // nobody is standing near anybody: there is no cluster to aim at, so hit what we already hit
            if (bestCount <= 0) return UseCurrentTargetOrNewTarget();

            return BreakTie(best, _currentTarget);
        }

        // Context: the laser will be shoot from me to a enemy.
        // Pick the enemy that a laser'll go through the most enemies.
        public ICombatant FindClusteredLaser(float beamHalfWidth)
        {
            List<ICombatant> enemies = GetEnemiesBFS();
            if (enemies.Count == 0) return null;

            Hex myHex = _me.CurrentHex;
            if (myHex == null) return null;

            Vector3 origin = myHex.transform.position;

            List<ICombatant> best = new List<ICombatant>();
            int bestCount = -1;

            // aim at each enemy, and count how many hero caught in the beam
            foreach (ICombatant candidate in enemies)
            {
                Vector3 toCandidate = candidate.CurrentHex().transform.position - origin;
                float candidateDistance = toCandidate.magnitude;

                if (candidateDistance <= Mathf.Epsilon) continue;

                Vector3 direction = toCandidate / candidateDistance;
                int count = enemies.Count(other => other != candidate && IsInLaser(origin, direction, other, beamHalfWidth));

                // found better candidate
                if (count > bestCount) { bestCount = count; best.Clear(); }

                // keep every candidate that ties for best
                if (count == bestCount) best.Add(candidate);
            }

            // the laser can't hit more than 1 target, so just shoot the target we have
            bool noOtherEnemyBesideTarget = (bestCount <= 0);
            if (noOtherEnemyBesideTarget) return UseCurrentTargetOrNewTarget();

            return BreakTie(best, _currentTarget);
        }


        // ========================================= private ========================================= 

        // when choosing a target, it could have several best candidate (a tied)
        // This is the protocol to solve when there's a tied target.
        private ICombatant BreakTie(List<ICombatant> tied, ICombatant preferred)
        {
            if (tied.Count == 1) return tied[0];

            // 1) if the tied list have prefered target, choose prefered target
            // context: prefered target = who we are engaged with
            if (preferred != null && preferred.IsAlive && tied.Contains(preferred)) return preferred;

            // 2) the nearest in the tied
            Hex myHex = _me.CurrentHex;
            if (myHex != null)
            {
                Vector3 origin = myHex.transform.position;

                var withDistance = tied
                    .Where(candidate => candidate.CurrentHex() != null)
                    .Select(candidate => (candidate, dist: Vector3.Distance(origin, candidate.CurrentHex().transform.position)))
                    .ToList();

                if (withDistance.Count > 0)
                {
                    float nearestDist = withDistance.Min(c => c.dist);
                    tied = withDistance.Where(c => c.dist <= nearestDist + TieEpsilon).Select(c => c.candidate).ToList();

                    if (tied.Count == 1) return tied[0];
                }
            }

            // 3) if nothing work, random between the tied
            return tied[Random.Range(0, tied.Count)];
        }

        // A twin to FindCurrentTarget()
        // the difference is this function, don't set new target which is desirable. 
        private ICombatant UseCurrentTargetOrNewTarget()
        {
            ICombatant engaged = _currentTarget;

            if (engaged != null && engaged.IsAlive && engaged.IsInCombat) return engaged;

            return FindNearestEnemy();
        }

        // Hero'll pick new current target if: 
        // 1) If target is out of range, pick new target.
        // 2) If target is dead, pick new target.
        private bool IsStillEngagedWith(ICombatant engaged)
        {
            // Is current target is dead?
            if (engaged == null || !engaged.IsAlive || !engaged.IsInCombat) return false;

            Hex myHex = _me.CurrentHex;
            if (myHex == null || engaged.CurrentHex() == null) return false;

            // Is current target is out of range?
            return myHex.IsWithinRange(engaged.CurrentHex(), _me.Range);
        }

        // Is this enemy close enough to the direction line to be caught by a shot travelling along it?
        private bool IsInLaser(Vector3 origin, Vector3 direction, ICombatant target, float halfWidth)
        {
            Vector3 toTarget = target.CurrentHex().transform.position - origin;

            // behind me - a projectile only travels one way, so these are never hit
            float along = Vector3.Dot(toTarget, direction);
            if (along < 0f) return false;

            // distance from the enemy to the line, measured square to it
            return (toTarget - direction * along).magnitude <= halfWidth;
        }

        // easy boolean logic to filter the enemy
        private bool IsEnemy(ICombatant target)
        {
            bool notTargetMyself = target != _me as ICombatant;
            bool notTargetFriend = target.Team != _me.Team;
            bool notTargetDead = target.IsAlive;
            bool notTargetGuyNotInCombat = target.IsInCombat;
            return notTargetMyself && notTargetFriend && notTargetDead && notTargetGuyNotInCombat;
        }

        // Every living enemy on the board.
        private List<ICombatant> GetEnemiesBFS()
        {
            // if was cached, return it
            bool isCache = (_enemyBFSCacheFrame == Time.frameCount);
            if (isCache) return _enemyBFSCache;

            _enemyBFSCache = _board.HeroesOnBoard.Where(IsEnemy).ToList();
            _enemyBFSCacheFrame = Time.frameCount;

            return _enemyBFSCache;
        }

        // Scan all enemy distance from myself.
        private List<(ICombatant target, float dist)> GetEnemyDistance()
        {
            // if was cached, return it
            bool isCache = (_enemyDistanceCacheFrame == Time.frameCount);
            if (isCache) return _enemyDistanceCache;

            Hex myHex = _me.CurrentHex;

            _enemyDistanceCache = GetEnemiesBFS()
            // calculate distance from myself to each enemy
            .Select(target => (target, dist: Vector3.Distance(myHex.transform.position, target.CurrentHex().transform.position)))
            // get a list of = (Hero : float)
            .ToList();
            _enemyDistanceCacheFrame = Time.frameCount;

            return _enemyDistanceCache;
        }
    }
}
