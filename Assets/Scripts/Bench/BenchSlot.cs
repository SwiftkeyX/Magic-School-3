using UnityEngine;

public class BenchSlot : MonoBehaviour, Placement
{
    private bool _reserved = false;

    // ==================== getter ===================
    public bool Reserved => _reserved;

    // ==================== setter ===================
    public void SetReserved(bool value) => _reserved = value;

    // A hero placed on a bench slot has no hex - nothing to do.
    public void OnHeroPlaced(Hero hero) { }
}