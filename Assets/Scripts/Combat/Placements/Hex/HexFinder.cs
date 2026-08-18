using System;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Combat.Placements
{
    // Picks a hex to stand on for anything that ARRIVES somewhere without walking there - a jump,
    // a dash, a summon. Walking has HexPathfinder; this is the "where do I land?" half, which is
    // one hop and no route.
    internal static class HexFinder
    {
        // The free hex next to targetHex that sits closest to `from`, or null when every neighbor
        // of the target is taken - the caller decides whether that means "wait" or "don't cast".
        //
        // isHexBlocked: is this hex claimed by someone other than the mover? Hex doesn't track
        // occupancy itself, so the caller owns that - same contract as HexPathfinder.
        public static Hex FindFreeHexNextTo(Hex targetHex, Hex from, Func<Hex, bool> isHexBlocked)
        {
            if (targetHex == null) return null;

            // no origin to measure from (mover is off-board) - measure from the target itself,
            // which just means "any free neighbor"
            Vector3 origin = from != null ? from.transform.position : targetHex.transform.position;

            return targetHex.GetNeighbors()
                .Where(hex => !isHexBlocked(hex))
                .OrderBy(hex => Vector3.Distance(origin, hex.transform.position))
                .FirstOrDefault();
        }
    }
}
