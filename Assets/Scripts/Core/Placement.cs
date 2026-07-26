using UnityEngine;

/// <summary>
/// Hex and Benchslot are used to place hero on it.
/// We create interface for both of them.
/// </summary>
public interface Placement
{
    Transform transform { get; }

    // Called once a hero's transform has been moved onto this placement -
    // lets each concrete placement decide what that means for the hero (or nothing).
    void OnHeroPlaced(Hero hero);
}