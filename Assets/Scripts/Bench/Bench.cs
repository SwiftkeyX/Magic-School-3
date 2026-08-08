using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool
{
    public class Bench : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private List<BenchSlot> _benchSlots;
        private BattleBoard _board;     // need to know which board, it'll seed on (there maybe several board at once)
        public event Action<HeroDataSO, Team, Placement, BattleBoard> OnHeroSpawned;

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

            // spawn hero prefab, move it to that slot, reserved the freeslot
            OnHeroSpawned.Invoke(data, Team.Blue, freeSlot, _board);
            return true;
        }
    }
}
