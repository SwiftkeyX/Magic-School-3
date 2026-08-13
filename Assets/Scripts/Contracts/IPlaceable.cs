using UnityEngine;
using MagicSchool.Placements;

namespace MagicSchool.Contracts
{
    // contract for moving hero on the board e.g. when we move hero on our own
    public interface IPlaceable
    {
        Transform transform { get; }

        Hex CurrentHex { get; }
        Hex ReservedHex { get; }
        Placement CurrentPlacement { get; }
        bool IsInCombat { get; }        // false when standing somewhere that isn't a Hex, e.g. the bench

        void SetReservedHex(Hex hex);
        void SetCurrentPlacement(Placement placement);
    }
}
