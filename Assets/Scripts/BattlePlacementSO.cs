using System.Collections.Generic;
using UnityEngine;

// A reusable test-case config: which heroes start on which hexes. Swap the asset assigned
// to BattleBoard's _placement field to try a different scenario without touching the scene.
[CreateAssetMenu(fileName = "BattleSetup", menuName = "Magic School 3/Battle Placement")]
public class BattlePlacementSO : ScriptableObject
{
    [SerializeField] private List<HeroPlacement> _heroesPlacement = new List<HeroPlacement>();

    public IReadOnlyList<HeroPlacement> HeroesPlacement => _heroesPlacement;
}
