using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleBoard : MonoBehaviour
{
    // ============================ Dependency ============================
    [SerializeField] private GameObject _heroPrefab;
    [SerializeField] private BattlePlacementSO _placementSO;
    
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
                    hex.SetBoard(this);
                    rowIndex++;
                }
                columnIndex++;
            }
        }
    }

    void Start()
    {
        // Spawn every hero to the board at the start of the combat
        if (_placementSO == null) return;
        foreach (var heroPlacement in _placementSO.HeroesPlacement)
        {
            HeroDataSO data = heroPlacement.dataSO;
            HexPlacement placement = heroPlacement.hexPlacement;
            SpawnHero(_heroPrefab, _hexs[placement], placement.team, data);
        }
    }

    // Spawn hero on the specific hex
    public Hero SpawnHero(GameObject heroPrefab, Hex targetHex, Team team, HeroDataSO dataSO)
    {
        // spawn hero prefab
        GameObject heroInstance = Instantiate(heroPrefab, transform);
        Hero hero = heroInstance.GetComponent<Hero>();

        // init hero: move hero to initial hex, assign team, assign stat
        hero.SetBoard(this);
        hero.Init(targetHex, team, dataSO);

        // track that hero via _heroesOnBoard
        _heroesOnBoard.Add(hero);

        return hero;
    }
}
