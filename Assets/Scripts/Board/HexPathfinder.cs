using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Finds the shortest unoccupied route between hexes on the board, via A* search.
public static class HexPathfinder
{
    // The largest possible Euclidean distance between two adjacent hexes (matches the
    // 1.15x neighbor threshold in Hex.cs). Dividing the heuristic by this keeps it
    // admissible - it never overestimates the true number of hex-hops remaining.
    private const float MaxHexStepDistance = 1.15f;

    // Finds the best next hex for startHex to move to on its way toward any hex adjacent
    // to targetHex, via A* search over the hex grid. Compared to plain BFS, the
    // Euclidean-distance heuristic breaks ties between equally-short routes in favor of
    // the one that actually heads toward the target, instead of whichever route happens
    // to be discovered first - which could otherwise be a same-length detour that doubles
    // back through the mover's own territory when the direct hexes are occupied.
    //
    // Always returns the real next step along the shortest route, even when that step
    // doesn't look like local progress (e.g. a necessary detour around a permanently
    // blocked hex has to start by moving sideways or backward before curving toward the
    // goal). Whether that step is worth taking immediately versus waiting a moment for
    // contention to clear is a timing judgment call, not a pathfinding one - callers that
    // want a grace period before committing to a non-improving step should apply it
    // themselves.
    //
    // reservedHexes is the set of hexes currently claimed by other heroes (Hex itself
    // doesn't track occupancy - the caller supplies it from Hero.ReservedHex).
    //
    // Returns null only when there is no unreserved hex adjacent to the target at all
    // (e.g. the target is fully boxed in by other heroes).
    public static Hex FindValidHexToTarget(Hex startHex, Hex targetHex, HashSet<Hex> reservedHexes)
    {
        var goalHexes = new HashSet<Hex>(targetHex.GetNeighbors().Where(h => !reservedHexes.Contains(h)));
        if (goalHexes.Count == 0) return null;

        float Heuristic(Hex h) => goalHexes.Min(g => Vector3.Distance(h.transform.position, g.transform.position)) / MaxHexStepDistance;

        var cameFrom = new Dictionary<Hex, Hex>();
        var gScore = new Dictionary<Hex, int> { [startHex] = 0 };
        var open = new List<Hex> { startHex };
        var closed = new HashSet<Hex>();

        while (open.Count > 0)
        {
            open.Sort((a, b) => (gScore[a] + Heuristic(a)).CompareTo(gScore[b] + Heuristic(b)));
            Hex current = open[0];
            open.RemoveAt(0);

            if (goalHexes.Contains(current))
            {
                // Walk the backtrack chain to the step right after startHex.
                Hex step = current;
                while (cameFrom[step] != startHex) step = cameFrom[step];
                return step;
            }

            closed.Add(current);

            foreach (var neighbor in current.GetNeighbors())
            {
                if (closed.Contains(neighbor) || reservedHexes.Contains(neighbor)) continue;

                int tentativeG = gScore[current] + 1;
                if (gScore.TryGetValue(neighbor, out int existingG) && tentativeG >= existingG) continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
                if (!open.Contains(neighbor)) open.Add(neighbor);
            }
        }

        return null;
    }
}
