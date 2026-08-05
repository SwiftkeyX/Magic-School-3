using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keep list of placement for each hero on the board
/// It is here to quickly seed the scene with hero for fast testing
/// </summary>
[CreateAssetMenu(fileName = "BattleSetup", menuName = "Magic School 3/Battle Placement")]
public class BattlePlacementSO : ScriptableObject
{
    [SerializeField] private List<HeroPlacement> _heroesPlacement = new List<HeroPlacement>();

    public IReadOnlyList<HeroPlacement> HeroesPlacement => _heroesPlacement;
}
