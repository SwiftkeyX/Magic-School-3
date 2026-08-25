using System.Linq;
using UnityEngine;
using MagicSchool.Engine;
using MagicSchool.Contracts;
using MagicSchool.Combat.Placements;
using MagicSchool.Skills;

namespace MagicSchool.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BattleBoard _board;
        [SerializeField] private Bench _bench;
        [SerializeField] private BattlePlacementSO _placementSO;
        [SerializeField] private TemplateActionRegistrySO _templateActions;
        [SerializeField] private bool _seedMode;

        public GamePhaseEnum Phase { get; private set; } = GamePhaseEnum.Preparation;
        public TeamEnum? Winner { get; private set; }

        // ======================== composition root ========================
        private HeroMover _heroMover;
        private BattleBoardSeed _seed;
        private HeroSpawner _heroSpawner;

        void Awake()
        {
            Instance = this;

            _heroMover = new HeroMover();
            _seed = new BattleBoardSeed(_placementSO, _board);
            _heroSpawner = new HeroSpawner(_heroMover, _bench, _seed, _templateActions);
        }

        // at start, spawn enemy team
        void Start()
        {
            _seed.SpawnTeamOnBoard(TeamEnum.Red);
        }

        // The system moving a hero, as opposed to a hero walking itself during combat.
        public void MoveHero(ICombatant hero, IPlacement placement) => _heroMover.MoveThisHeroTo(hero, placement);

        void Update()
        {
            if (Phase == GamePhaseEnum.Combat)
            {
                CheckForWinner();
            }
        }

        public void StartCombat()
        {
            if (Phase != GamePhaseEnum.Preparation) return;

            // spawn player team if seedMode activate
            if (_seedMode) _seed.SpawnTeamOnBoard(TeamEnum.Blue);

            // guard
            if (!HasHeroesOnBoard(TeamEnum.Blue))
            {
                DebugTool.LogWarning("Can't start combat - place at least one hero on the board first.");
                return;
            }

            SetPhase(GamePhaseEnum.Combat);
        }

        private bool HasHeroesOnBoard(TeamEnum team) => _board.HeroesOnBoard.Any(h => h.Team == team);

        // The board is the one thing heroes already hold a reference to, so the phase is pushed
        // there rather than pulled back out of this singleton.
        private void SetPhase(GamePhaseEnum phase)
        {
            Phase = phase;
            
            if (_board != null) _board.SetBattleOn(phase == GamePhaseEnum.Combat);
        }

        private void CheckForWinner()
        {
            var alive = _board.HeroesOnBoard.Where(h => h.StateType != HeroStateEnum.Dead);
            bool blueAlive = alive.Any(h => h.Team == TeamEnum.Blue);
            bool redAlive = alive.Any(h => h.Team == TeamEnum.Red);

            if (blueAlive && redAlive) return;

            Winner = blueAlive ? TeamEnum.Blue : redAlive ? TeamEnum.Red : (TeamEnum?)null;
            SetPhase(GamePhaseEnum.Result);
        }
    }
}
