using UnityEngine;

namespace MagicSchool.Contracts
{
    // IPlaceable answer: to move a unit (the hero) on the IPlacement (the board) 
    // e.g. hero walk to the next hex
    // e.g. player move hero by dragging
    public interface IPlaceable
    {
        Transform transform { get; }
        IPlacement CurrentPlacement { get; }    // the placement this unit is standing on
        bool IsInCombat { get; }                // FLAGGING: this one shouldn't belong in this interface, no?
        void SetCurrentPlacement(IPlacement placement);
        IPlacement FindFreePlacementNextTo(IPlaceable target);
    }
}
