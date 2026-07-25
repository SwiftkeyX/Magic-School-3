using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bench : MonoBehaviour
{
    [SerializeField] private List<BenchSlot> _benchSlots;

    /// <summary>
    /// Spawn hero on the bench
    /// </summary>
    public void SpawnHeroOnBench(HeroDataSO data)
    {
        // find the first slot that hasn't been used yet
        BenchSlot freeSlot = _benchSlots.FirstOrDefault(slot => !slot.Reserved);
        if (freeSlot == null)
        {
            Debug.LogWarning("Bench: no free slot to place hero.");
            return;
        }

        // spawn hero prefab, move it to that slot, reserve the slot
        GameObject heroPrefab = Instantiate(data.Prefab);
        heroPrefab.transform.position = freeSlot.transform.position;
        freeSlot.SetReserved(true);
    }
}

