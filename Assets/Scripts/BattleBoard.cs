using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleBoard : MonoBehaviour
{
    private Dictionary<HexPlacement, Hex> _hexs;
    [SerializeField] private GameObject _heroPrefab;
    [SerializeField] private List<HexPlacement> _heroPlacement;

    public IReadOnlyDictionary<HexPlacement, Hex> Hexs => _hexs;

    void Awake()
    {
        InitializeHex();
    }

    // BattleBoard find reference to each hex
    void InitializeHex()
    {
        var allHexes = new List<Hex>(GetComponentsInChildren<Hex>(true));
        _hexs = new Dictionary<HexPlacement, Hex>();

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
        foreach (var placement in _heroPlacement)
            SpawnHero(_heroPrefab, _hexs[placement]);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Hero SpawnHero(GameObject heroPrefab, Hex targetHex)
    {
        // spawn hero prefab
        GameObject heroInstance = Instantiate(heroPrefab, transform);
        Hero hero = heroInstance.GetComponent<Hero>();

        // move hero to initial hex
        hero.Init(targetHex);

        return hero;
    }
}
