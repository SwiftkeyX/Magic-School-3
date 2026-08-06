using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour, Placement
{
    private BattleBoard _board;
    // neighbors hex of this hex - to give the hex available for pathfinding logic
    private List<Hex> _neighbors;
    private HexPlacement _hexPlacement;

    // ========================== getter & setter ==========================
    public string Name => gameObject.name;

    #region Life Cycle
    void Start()
    {
        InitializeNeighbors();
    }

    #endregion

    #region Setup
    public void Init(BattleBoard board, HexPlacement hexPlacement)
    {
        _board = board;
        _hexPlacement = hexPlacement;
        _neighbors = null;
    }

    // place hero on Hex
    public void OnHeroPlaced(IPlaceable hero)
    {
        this.EnterPlacementExtension(hero);

        hero.SetReservedHex(this);
    }

    // unplace hero from Hex
    public void OnHeroUnplaced(IPlaceable hero)
    {
        // what about setcurrenthex = null?
        hero.SetReservedHex(null);
    }

    public Team GetTeam()
    {
        return _hexPlacement.team;
    }
    #endregion

    /// <summary>
    /// Neighbors are the hex adjacent to current hex.
    /// </summary>
    #region Neighbors
    // called by Hero - so hero know which hex is valid to move
    public List<Hex> GetNeighbors()
    {
        if (_neighbors == null) InitializeNeighbors();

        return _neighbors;
    }

    public bool IsAdjacentTo(Hex other) => GetNeighbors().Contains(other);

    // Generalizes IsAdjacentTo to N hex-hops via BFS, for heroes whose attack range is > 1.
    // BFS over the neighbor graph rather than raw world-space distance, since hex spacing
    // isn't uniform (same-column neighbors are ~1.0 apart, diagonal ~1.118 apart) - a
    // distance-based cutoff would drift as range grows, hop-counting can't.
    // FIXLATER: Now we have 2 standard of measuring the hex distance, we do Vector3.Distance() & count the hex using BFS. 
    // => We should use the same standard NOT scatter like this, no?
    public bool IsWithinRange(Hex other, int range)
    {
        if (other == this) return true;

        var visited = new HashSet<Hex> { this };
        var frontier = new List<Hex> { this };

        for (int step = 0; step < range; step++)
        {
            var nextFrontier = new List<Hex>();
            foreach (var hex in frontier)
            {
                foreach (var neighbor in hex.GetNeighbors())
                {
                    if (!visited.Add(neighbor)) continue;
                    if (neighbor == other) return true;
                    nextFrontier.Add(neighbor);
                }
            }
            frontier = nextFrontier;
        }

        return false;
    }

    // Neighbors don't create itself, we need to calculate it ourself
    private void InitializeNeighbors()
    {
        var hexs = _board.Hexs.Values;

        // find distance between current hex and every hex
        float nearestDist = float.MaxValue;
        foreach (var hex in hexs)
        {
            if (hex == this) continue;
            float dist = Vector3.Distance(transform.position, hex.transform.position);
            if (dist < nearestDist)
                nearestDist = dist;
        }

        // create neighbors of hex as a list
        float threshold = nearestDist * 1.15f;
        _neighbors = new List<Hex>();
        foreach (var hex in hexs)
        {
            if (hex == this) continue;
            if (Vector3.Distance(transform.position, hex.transform.position) <= threshold)
                _neighbors.Add(hex);
        }
    }
    #endregion
}
