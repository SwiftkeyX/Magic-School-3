using UnityEngine;
using MagicSchool.Engine;
using MagicSchool.Contracts;
using MagicSchool.Combat.Heroes;
using MagicSchool.Combat.Placements;
using MagicSchool.Skills;

namespace MagicSchool.Core
{
    public class HeroSpawner
    {
        private readonly HeroMover _heroMover;
        private readonly TemplateActionRegistrySO _templateActions;

        public HeroSpawner(HeroMover heroMover, Bench bench, HeroSeed seed, TemplateActionRegistrySO templateActions)
        {
            _heroMover = heroMover;
            _templateActions = templateActions;

            if (bench != null) bench.OnSpawnRequested += SpawnHero;
            if (seed != null) seed.OnSpawnRequested += SpawnHero;
        }

        // basically spawn hero
        private void SpawnHero(HeroDataSO data, TeamEnum team, IPlacement placement, BattleBoard board)
        {
            // spawn hero
            GameObject heroPrefab = SceneHelper.Instantiate(data.Prefab);
            Hero hero = heroPrefab.GetComponent<Hero>();
            hero.Init(data, board, team, SkillLibrary.Resolve(data.SkillId, _templateActions));

            // move them to "placement"
            _heroMover.MoveThisHeroTo(hero, placement);
        }
    }
}