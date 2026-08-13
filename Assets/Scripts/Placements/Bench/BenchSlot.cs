using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Placements
{
    public class BenchSlot : MonoBehaviour, Placement
    {
        private bool _reserved = false;

        // ==================== getter ===================
        public bool Reserved => _reserved;

        // A hero placed on a bench slot has no hex - just remember who's here so
        // OnHeroUnplaced can free the slot again once they leave.
        public void OnHeroPlaced(IPlaceable hero)
        {
            this.EnterPlacementExtension(hero);

            _reserved = true;
        }

        // The hero sitting here is moving to a different placement - free the slot.
        public void OnHeroUnplaced(IPlaceable hero)
        {
            _reserved = false;
        }
    }
}
