using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Scans the board for a hero's nearest/furthest living enemy. Split out of
// HeroStateMachineBlackBoard since it's a self-contained algorithm, not glue/data.
public class FindEnemy
{
    private const float TieEpsilon = 0.01f;     // How close two enemies' distances have to be to count as tied.

    private readonly Hero _me;
    private readonly HeroDataRuntime _runtimeData;
    private BattleBoard _board;

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

    // Scan all enemy distance from myself. Cached per frame so repeat calls in the same tick
    // (e.g. attack-target logic, then a skill cast aiming) don't redo the same board scan.
    private List<(Hero target, float dist)> GetEnemyDistance()
    {
        bool isCache = (_enemyDistanceCacheFrame == Time.frameCount);
        if (isCache) return _enemyDistanceCache;

        Hex myHex = _runtimeData.CurrentPlacement as Hex;

        _enemyDistanceCache = _board.HeroesOnBoard
        // select enemy hero only
        .Where(target =>
        {
            bool notTargetMyself = target != _me;
            bool notTargetFriend = target.Team != _me.Team;
            bool notTargetDead = target.State != HeroStateType.Dead;
            bool notTargetGuyNotInCombat = target.Blackboard.IsInCombat();
            return notTargetMyself && notTargetFriend && notTargetDead && notTargetGuyNotInCombat;
        })
        // calculate distance from myself to each enemy
        .Select(target => (target, dist: Vector3.Distance(myHex.transform.position, target.Blackboard.GetCurrentHex().transform.position)))
        // get a list of = (Hero : float)
        .ToList();
        _enemyDistanceCacheFrame = Time.frameCount;

        return _enemyDistanceCache;
    }
}
