using UnityEngine;

namespace MagicSchool.Contracts
{
    /// <summary>
    /// Hex and Benchslot are used to place hero on it.
    /// We create interface for both of them.
    /// </summary>
    public interface Placement
    {
        Transform transform { get; }

        // Called once a hero's transform has been moved onto this placement
        void OnHeroPlaced(IPlaceable hero);

        // Called just before a hero already on this placement moves to a different one
        void OnHeroUnplaced(IPlaceable hero);
    }

    // need somewhere to put the redundant code of each Placement logic, so we move it into dedicated Extension class
    public static class PlacementExtensions
    {
        // basically, set placement for the hero
        public static void EnterPlacementExtension(this Placement placement, IPlaceable hero)
        {
            hero.SetCurrentPlacement(placement);
            hero.transform.position = placement.transform.position;
        }
    }
}
