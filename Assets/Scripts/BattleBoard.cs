using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleBoard : MonoBehaviour
{
    // ============================ Dependency ============================
    [SerializeField] private BattlePlacementSO _placementSO;

    // ======================== Runtime data ============================
    // track every hex
    private Dictionary<HexPlacement, Hex> _hexs = new Dictionary<HexPlacement, Hex>();

    // track every hero on the battle board
    private List<Hero> _heroesOnBoard = new List<Hero>();

    // ========================= etc =============================
    [SerializeField] private bool _seedMode;

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
        // If seedmode activate, spawn every hero to the board at the start of the combat
        SpawnHeroWithSeed();


    }

    // Spawn every hero to the board at the start of the combat.
    // This is used when you want to skip the dragging part, and to get start the battle quickly.
    private void SpawnHeroWithSeed()
    {
        if (!_seedMode) return;
        if (_placementSO == null) { Debug.LogError("Can't enter seed mode. Hero Placement is null"); return; }
        foreach (var heroPlacement in _placementSO.HeroesPlacement)
        {
            HeroDataSO data = heroPlacement.dataSO;
            HexPlacement placement = heroPlacement.hexPlacement;
            SpawnHeroOnBoardDirectly(_hexs[placement], placement.team, data);
        }
    }

    // "Spawn hero on board directly" will skip the bench logic part.
    // player normally go through bench logic first: buy hero => hero spawn on bench => drag hero to the board.
    // but for enemy side, their heroes just spawn on board directly. 
    // player side can also use this though, because sometime we need to test the combat quickly, and want to skip the bench part.
    private Hero SpawnHeroOnBoardDirectly(Hex targetHex, Team team, HeroDataSO dataSO)
    {
        // spawn hero prefab
        GameObject heroInstance = Instantiate(dataSO.Prefab, transform);
        Hero hero = heroInstance.GetComponent<Hero>();

        // init hero: move hero to initial hex, assign team
        hero.SetBoard(this);
        hero.SetTeam(team);
        hero.MoveHeroInPreparationState(targetHex);

        // track that hero via _heroesOnBoard
        _heroesOnBoard.Add(hero);

        return hero;
    }
}
