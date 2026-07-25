using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bench : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private List<BenchSlot> _benchSlots;

    private bool _isHeroHolded = false;
    private Hero _heroHolded;

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
        Hero hero = heroPrefab.GetComponent<Hero>();
        hero.SetTeam(Team.Blue);
        heroPrefab.transform.position = freeSlot.transform.position;
        freeSlot.SetReserved(true);
    }
}

