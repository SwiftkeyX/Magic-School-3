using System;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Combat.Placements
{
    // Picks a hex for a unit to stand on BUT without them walking 
    // e.g. a jump, a dash, a summon. 
    internal static class HexFinder
    {
        // consume target hex, spit out the neighbors hex to target hex.
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
