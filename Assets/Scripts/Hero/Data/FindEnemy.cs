using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Scans the board for a hero's nearest/furthest/most-clustered living enemy.
public class FindEnemy
{
    private const float TieEpsilon = 0.01f;     // How close two enemies' distances have to be to count as tied.

    private readonly Hero _me;
    private readonly HeroDataRuntime _runtimeData;
    private BattleBoard _board;

    private List<Hero> _enemyBFSCache;
    private int _enemyBFSCacheFrame = -1;

    private List<(Hero target, float dist)> _enemyDistanceCache;
    private int _enemyDistanceCacheFrame = -1;

    public FindEnemy(Hero me, HeroDataRuntime runtimeData)
    {
        _me = me;
        _runtimeData = runtimeData;
    }

    public void SetBoard(BattleBoard board) => _board = board;

    // Picks nearest enemy (if there are several nearest enemies, random it).
    public Hero FindNearestEnemy()
    {
        var enemyDistances = GetEnemyDistance();

        if (enemyDistances.Count == 0) return null;

        float nearestDist = enemyDistances.Min(e => e.dist);
        var tiedNearest = enemyDistances.Where(e => e.dist <= nearestDist + TieEpsilon).Select(e => e.target).ToList();

        // Sticks with the previous pick across calls as long as it's still tied for nearest, so the target
        // doesn't flicker between equally-near enemies frame to frame.
        Hero nearestEnemy = _runtimeData.NearestEnemy;
        if (nearestEnemy != null && tiedNearest.Contains(nearestEnemy)) return nearestEnemy;

        nearestEnemy = tiedNearest[Random.Range(0, tiedNearest.Count)];
        _runtimeData.SetNearestEnemy(nearestEnemy);
        return nearestEnemy;
    }

    // Picks furthest enemy (if there are several furthest enemies, random it).
    public Hero FindFurthestEnemy()
    {
        var enemyDistances = GetEnemyDistance();

        if (enemyDistances.Count == 0) return null;

        float furthestDist = enemyDistances.Max(e => e.dist);
        var tiedFurthest = enemyDistances.Where(e => e.dist >= furthestDist - TieEpsilon).Select(e => e.target).ToList();

        return tiedFurthest[Random.Range(0, tiedFurthest.Count)];
    }

    // Pick the hex that are most cluster (measuring by inpu radius)
    // FLAGGING: This one use IsWithInRange() instead of checking distance like the others. This isn't test yet.
    public Hero FindClusteredEnemy(int radius = 2)
    {
        List<Hero> enemies = GetEnemiesBFS();
        if (enemies.Count == 0) return null;

        Hero best = null;
        int bestCount = -1;

        foreach (Hero candidate in enemies)
        {
            Hex candidateHex = candidate.CurrentHex;
            int count = enemies.Count(other => other != candidate && candidateHex.IsWithinRange(other.CurrentHex, radius));

            if (count > bestCount)
            {
                bestCount = count;
                best = candidate;
            }
        }

        return best;
    }

    // easy boolean logic to filter the enemy 
    private bool IsEnemy(Hero target)
    {
        bool notTargetMyself = target != _me;
        bool notTargetFriend = target.Team != _me.Team;
        bool notTargetDead = target.StateType != HeroStateType.Dead;
        bool notTargetGuyNotInCombat = target.IsInCombat;
        return notTargetMyself && notTargetFriend && notTargetDead && notTargetGuyNotInCombat;
    }

    // Every living enemy on the board. 
    private List<Hero> GetEnemiesBFS()
    {
        // if was cached, return it
        bool isCache = (_enemyBFSCacheFrame == Time.frameCount);
        if (isCache) return _enemyBFSCache;

        _enemyBFSCache = _board.HeroesOnBoard.Where(IsEnemy).ToList();
        _enemyBFSCacheFrame = Time.frameCount;

        return _enemyBFSCache;
    }

    // Scan all enemy distance from myself.
    private List<(Hero target, float dist)> GetEnemyDistance()
    {
        // if was cached, return it
        bool isCache = (_enemyDistanceCacheFrame == Time.frameCount);
        if (isCache) return _enemyDistanceCache;

        Hex myHex = _runtimeData.CurrentPlacement as Hex;

        _enemyDistanceCache = GetEnemiesBFS()
        // calculate distance from myself to each enemy
        .Select(target => (target, dist: Vector3.Distance(myHex.transform.position, target.CurrentHex.transform.position)))
        // get a list of = (Hero : float)
        .ToList();
        _enemyDistanceCacheFrame = Time.frameCount;

        return _enemyDistanceCache;
    }
}
