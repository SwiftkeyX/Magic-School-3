using UnityEngine;

/// <summary>
/// Hex and Benchslot are used to place hero on it.
/// We create interface for both of them.
/// </summary>
public interface Placement
{
    Transform transform { get; }
}