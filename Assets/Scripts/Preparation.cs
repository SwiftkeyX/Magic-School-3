using UnityEngine;

// <summary>
// In Preparation state, it have so many function that alike to Combat state which cause a lot of confusion.
// So we make a new script just for this state.
// </summary>
public class Preparation : MonoBehaviour
{
    [SerializeField] private BattlePlacementSO _placementSO;
    [SerializeField] private bool _seedMode;
    [SerializeField] private BattleBoard _board;
    public static Preparation Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // If seedmode activate, spawn every hero from seed to the board at the start of the combat
    void Start()
    {
        SpawnHeroWithSeed();
    }

    // basically spawn hero
    public void SpawnHero(HeroDataSO data, Team team, Placement placement)
    {
        GameObject heroPrefab = Instantiate(data.Prefab);
        Hero hero = heroPrefab.GetComponent<Hero>();

        // init hero by assigning their team, and give correct board reference to them
        HeroInit(hero, team);

        // move them
        MoveThisHeroInPreParationState(hero, placement);
    }

    // This will skip the bench logic part.
    // player normally go through bench logic first: buy hero => hero spawn on bench => drag hero to the board.
    // but for enemy side, their heroes just spawn on board directly, so they want to skip bench logic. 
    // P.S. player side can also use this though, because sometime we need to test the combat quickly, and want to skip the bench part.
    private void SpawnHeroWithSeed()
    {
        if (!_seedMode) return;
        if (_placementSO == null) { Debug.LogError("Can't seed the hero. Hero Placement is null"); return; }
        foreach (var heroPlacement in _placementSO.HeroesPlacement)
        {
            HeroDataSO data = heroPlacement.dataSO;
            HexPlacement placement = heroPlacement.hexPlacement;
            SpawnHero(data, placement.team, _board.Hexs[placement]);
        }
    }

    private void HeroInit(Hero hero, Team team)
    {
        hero.SetBoard(_board);
        hero.SetTeam(team);
    }

    public void MoveThisHeroInPreParationState(Hero hero, Placement placement)
    {
        placement.OnHeroPlaced(hero);
        hero.transform.position = placement.transform.position;
    }
}
