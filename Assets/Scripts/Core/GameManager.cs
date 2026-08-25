using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        private HeroMover _heroMover;
        private BattleBoardSeed _seed;
        private HeroSpawner _heroSpawner;
        private IMatchStatusView _status;

        // init
        void Awake()
        {
            Instance = this;

            _heroMover = new HeroMover();
            _seed = new BattleBoardSeed(_placementSO, _board);
            _heroSpawner = new HeroSpawner(_heroMover, _bench, _seed, _templateActions);
            _status = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IMatchStatusView>()
                .FirstOrDefault();
        }

        void Start()
        {
            // at start, spawn enemy team
            _seed.SpawnTeamOnBoard(TeamEnum.Red);

            // in preparation state, show banner
            _status?.ShowPreparation();
        }

        // restart by re-loading the scene 
        public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

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

        private void SetPhase(GamePhaseEnum phase)
        {
            Phase = phase;

            if (_board != null) _board.SetBattleOn(phase == GamePhaseEnum.Combat);

            if (_status == null) return;
            if (phase == GamePhaseEnum.Preparation) _status.ShowPreparation();
            else if (phase == GamePhaseEnum.Combat) _status.ShowCombat();
            else _status.ShowResult(Winner);   // Winner is set before this is called
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
