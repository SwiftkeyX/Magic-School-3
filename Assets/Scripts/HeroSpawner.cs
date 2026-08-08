using UnityEngine;

namespace MagicSchool
{
    public class HeroSpawner
    {
        private HeroMover _heroMover;

        public HeroSpawner(Bench bench, BattleBoardSeed board)
        {
            bench.OnHeroSpawned += SpawnHero;
            board.OnHeroSpawned += SpawnHero;
        }

        // basically spawn hero
        public void SpawnHero(HeroDataSO data, Team team, Placement placement, BattleBoard board)
        {
            // spawn hero
            GameObject heroPrefab = SceneHelper.Instantiate(data.Prefab);
            Hero hero = heroPrefab.GetComponent<Hero>();
            hero.Init(data, board, team);

            // move them to "placement"
            _heroMover.MoveThisHeroTo(hero, placement);
        }
    }

}