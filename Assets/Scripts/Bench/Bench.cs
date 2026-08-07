using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool
{
    public class Bench : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private List<BenchSlot> _benchSlots;

        private bool _isHeroHolded = false;
        private Hero _heroHolded;

        /// <summary>
        /// Spawn hero on the bench. Returns false (and spawns nothing) if the bench is full.
        /// </summary>
        public bool SpawnHeroOnBench(HeroDataSO data)
        {
            // find the first slot that hasn't been used yet
            BenchSlot freeSlot = _benchSlots.FirstOrDefault(slot => !slot.Reserved);
            if (freeSlot == null)
            {
                Debug.LogWarning("Bench: no free slot to place hero.");
                return false;
            }

            // spawn hero prefab, move it to that slot - freeSlot.OnHeroPlaced() reserves it
            Preparation.Instance.SpawnHero(data, Team.Blue, freeSlot);
            return true;
        }
    }
}
