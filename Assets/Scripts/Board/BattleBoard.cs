using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleBoard : MonoBehaviour
{
    // ======================== Runtime data ============================
    // track every hex
    private Dictionary<HexPlacement, Hex> _hexs = new Dictionary<HexPlacement, Hex>();

    // track every hero on the battle board
    private List<Hero> _heroesOnBoard = new List<Hero>();

    // ======================== Setter & Getter ========================
    public IReadOnlyDictionary<HexPlacement, Hex> Hexs => _hexs;
    public IReadOnlyList<Hero> HeroesOnBoard => _heroesOnBoard;

    void Awake()
    {
        InitializeHex();
    }

    // BattleBoard find reference to each hex
    void InitializeHex()
    {
        var allHexes = new List<Hex>(GetComponentsInChildren<Hex>(true));

        foreach (var sideGroup in allHexes.GroupBy(h => h.transform.parent.name))
        {
            Team side = sideGroup.Key == "BlueSideHex" ? Team.Blue : Team.Red;

            var columns = sideGroup
                .GroupBy(h => Mathf.RoundToInt(h.transform.localPosition.x * 10f))
                .OrderBy(g => g.Key)
                .ToList();

            int columnIndex = 0;
            foreach (var column in columns)
            {
                var sortedRows = column.OrderByDescending(h => h.transform.localPosition.y).ToList();
                int rowIndex = 0;
                foreach (var hex in sortedRows)
                {
                    HexPlacement hexKey = new HexPlacement(side, columnIndex, rowIndex);
                    _hexs[hexKey] = hex;
                    hex.Init(this, hexKey);
                    rowIndex++;
                }
                columnIndex++;
            }
        }
    }

    // Every hero need to be tracked on the board
    // If they didn't get tracked, those heroes will be invisible to other hero.
    public void TrackThisHero(Hero hero)
    {
        if (!_heroesOnBoard.Contains(hero)) _heroesOnBoard.Add(hero);
    }

    // Counterpart to TrackThisHero - a hero leaving its hex for a non-hex placement
    // (e.g. back to the bench) is no longer on the battlefield.
    public void UntrackThisHero(Hero hero) => _heroesOnBoard.Remove(hero);

    // FIXNOW: This should belong to FindEnemy.cs
    public Hero FindClusteredEnemy(Team myTeam, int radius = 2)
    {
        List<Hero> enemies = _heroesOnBoard.Where(h => h.Team != myTeam && h.State != HeroStateType.Dead).ToList();
        if (enemies.Count == 0) return null;

        Hero best = null;
        int bestCount = -1;
        foreach (var candidate in enemies)
        {
            Hex candidateHex = candidate.CurrentHex;
            int count = enemies.Count(other => other != candidate && candidateHex.IsWithinRange(other.CurrentHex, radius));
            if (count > bestCount)
            {
                bestCount = count;
                best = candidate;
            }
        }

        return best;
    }
}
