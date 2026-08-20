using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicSchool.Combat.Placements
{
    // Picks a hex for a specify condition
    // Other class need to find a hex for some reason e.g. a jump, a dash, a summon. 
    internal static class HexFinder
    {
        // find all possible "free" hex with in the specify radius
        // free hex = no hero occupy it
        public static List<Hex> FindFreeHexesWithin(Hex from, int range, Func<Hex, bool> isHexBlocked)
            => FindHexesWithin(from, range).Where(hex => !isHexBlocked(hex)).ToList();

        // find all possible hex with in the specify radius
        public static List<Hex> FindHexesWithin(Hex centre, int radius)
        {
            var inReach = new List<Hex>();
            if (centre == null) return inReach;

            inReach.Add(centre);

            var visited = new HashSet<Hex> { centre };
            var frontier = new List<Hex> { centre };

            for (int step = 0; step < radius; step++)
            {
                var nextFrontier = new List<Hex>();
                foreach (Hex hex in frontier)
                {
                    foreach (Hex neighbor in hex.GetNeighbors())
                    {
                        if (!visited.Add(neighbor)) continue;

                        nextFrontier.Add(neighbor);
                        inReach.Add(neighbor);
                    }
                }
                frontier = nextFrontier;
            }

            return inReach;
        }
    }
}
