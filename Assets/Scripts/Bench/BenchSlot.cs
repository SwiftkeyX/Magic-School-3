using UnityEngine;

public class BenchSlot : MonoBehaviour, Placement
{
    private bool _reserved = false;
    private IPlaceable _occupant;

    // ==================== getter ===================
    public bool Reserved => _reserved;

    // ==================== setter ===================
    public void SetReserved(bool value) => _reserved = value;

    // A hero placed on a bench slot has no hex - just remember who's here so
    // OnHeroUnplaced can free the slot again once they leave.
    public void OnHeroPlaced(IPlaceable hero)
    {
        this.EnterPlacementExtension(hero);

        _occupant = hero;
        _reserved = true;
    }

    // The hero sitting here is moving to a different placement - free the slot.
    public void OnHeroUnplaced(IPlaceable hero)
    {
        _occupant = null;
        _reserved = false;
    }
}