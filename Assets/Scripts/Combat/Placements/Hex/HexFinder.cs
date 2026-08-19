using System;
using System.Collections.Generic;

namespace MagicSchool.Combat.Placements
{
    // Picks a hex for a specify condition
    // Other class need to find a hex for some reason e.g. a jump, a dash, a summon. 
    internal static class HexFinder
    {
        public static List<Hex> FindFreeHexesWithin(Hex from, int range, Func<Hex, bool> isHexBlocked)
        {
            var landings = new List<Hex>();
            if (from == null) return landings;

            if (!isHexBlocked(from)) landings.Add(from);

            var visited = new HashSet<Hex> { from };
            var frontier = new List<Hex> { from };

            for (int step = 0; step < range; step++)
            {
                var nextFrontier = new List<Hex>();
                foreach (Hex hex in frontier)
                {
                    foreach (Hex neighbor in hex.GetNeighbors())
                    {
                        if (!visited.Add(neighbor)) continue;

                        nextFrontier.Add(neighbor);
                        if (!isHexBlocked(neighbor)) landings.Add(neighbor);
                    }
                }
                frontier = nextFrontier;
            }

            return landings;
        }
    }
}
