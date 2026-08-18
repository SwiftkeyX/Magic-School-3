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

        // "Where could I stand next to that unit?" - the free placement closest to it, or null if
        // it is boxed in. Asked by anything that arrives somewhere without walking there (a jump,
        // a dash), which needs a landing spot that isn't already taken. It lives on IPlaceable
        // rather than ITargeter because the answer is a placement to move onto, not an enemy.
        IPlacement FindFreePlacementNextTo(IPlaceable target);
    }
}
