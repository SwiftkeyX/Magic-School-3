using UnityEngine;

/// <summary>
/// Hex and Benchslot are used to place hero on it.
/// We create interface for both of them.
/// </summary>
public interface Placement
{
    Transform transform { get; }

    // Called once a hero's transform has been moved onto this placement
    void OnHeroPlaced(Hero hero);

    // Called just before a hero already on this placement moves to a different one
    void OnHeroUnplaced(Hero hero);
}