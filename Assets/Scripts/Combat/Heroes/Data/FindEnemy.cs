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
        private readonly HeroDataRuntime _runtimeData;
        private readonly BattleBoard _board;

        private List<ICombatant> _enemyBFSCache;
        private int _enemyBFSCacheFrame = -1;

        private List<(ICombatant target, float dist)> _enemyDistanceCache;
        private int _enemyDistanceCacheFrame = -1;

        public FindEnemy(Hero me, HeroDataRuntime runtimeData, BattleBoard board)
        {
            _me = me;
            _runtimeData = runtimeData;
            _board = board;
        }

        // When a hero auto attack a enemy, he'll pick that enemy as current target. 
        // Which mean he'll stick to that target until something break them off.
        // read IsStillEngageWith() for more detail. 
        public ICombatant CurrentTarget
        {
            get
            {
                ICombatant engaged = _runtimeData.CurrentTarget;

                if (IsStillEngagedWith(engaged)) return engaged;

                // if current target is break off, find new target.
                ICombatant fresh = FindNearestEnemy();
                _runtimeData.SetCurrentTarget(fresh);

                return fresh;
            }
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

        // Picks nearest enemy (if there are several nearest enemies, random it).
        public ICombatant FindNearestEnemy()
        {
            var enemyDistances = GetEnemyDistance();

            if (enemyDistances.Count == 0) return null;

            float nearestDist = enemyDistances.Min(e => e.dist);
            var tiedNearest = enemyDistances.Where(e => e.dist <= nearestDist + TieEpsilon).Select(e => e.target).ToList();

            // Sticks with the previous pick across calls as long as it's still tied for nearest, so the target
            // doesn't flicker between equally-near enemies frame to frame.
            ICombatant nearestEnemy = _runtimeData.NearestEnemy;
            if (nearestEnemy != null && nearestEnemy.IsAlive && tiedNearest.Contains(nearestEnemy)) return nearestEnemy;

            nearestEnemy = tiedNearest[Random.Range(0, tiedNearest.Count)];
            _runtimeData.SetNearestEnemy(nearestEnemy);
            return nearestEnemy;
        }


        // Picks furthest enemy (if there are several furthest enemies, random it).
        public ICombatant FindFurthestEnemy()
        {
            var enemyDistances = GetEnemyDistance();

            if (enemyDistances.Count == 0) return null;

            float furthestDist = enemyDistances.Max(e => e.dist);
            var tiedFurthest = enemyDistances.Where(e => e.dist >= furthestDist - TieEpsilon).Select(e => e.target).ToList();

            return tiedFurthest[Random.Range(0, tiedFurthest.Count)];
        }

        // Pick the hex that are most cluster (measuring by input radius)
        // FiXLATER: This one use IsWithInRange() instead of checking distance like the others. This isn't test yet.
        public ICombatant FindClusteredEnemy(int radius = 2)
        {
            List<ICombatant> enemies = GetEnemiesBFS();
            if (enemies.Count == 0) return null;

            ICombatant best = null;
            int bestCount = -1;

            foreach (ICombatant candidate in enemies)
            {
                Hex candidateHex = candidate.CurrentHex();
                int count = enemies.Count(other => other != candidate && candidateHex.IsWithinRange(other.CurrentHex(), radius));

                if (count > bestCount)
                {
                    bestCount = count;
                    best = candidate;
                }
            }

            return best;
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

            Hex myHex = _runtimeData.CurrentPlacement as Hex;

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
