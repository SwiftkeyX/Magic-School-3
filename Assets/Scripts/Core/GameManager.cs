using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using MagicSchool.Engine;
using MagicSchool.Contracts;
using MagicSchool.Combat.Placements;
using MagicSchool.Core.States;
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

        public GamePhaseEnum Phase => _stateMachine == null ? GamePhaseEnum.Preparation : _stateMachine.CurrentType;
        public TeamEnum? Winner { get; private set; }

        private HeroMover _heroMover;
        private BattleBoardSeed _seed;
        private HeroSpawner _heroSpawner;
        private GameStateMachine _stateMachine;
        private IMatchStatusView _status;

        // ======================== what the states read ========================
        internal BattleBoard Board => _board;
        internal BattleBoardSeed Seed => _seed;
        internal IMatchStatusView Status => _status;

        // ======================================== getter ========================================
        internal void SetWinner(TeamEnum? winner) => Winner = winner;
        internal void ChangeState(GamePhaseEnum next) => _stateMachine.ChangeState(next);
        public void MoveHero(ICombatant hero, IPlacement placement) => _heroMover.MoveThisHeroTo(hero, placement);

        // init dependency 
        void Awake()
        {
            Instance = this;

            _heroMover = new HeroMover();
            _seed = new BattleBoardSeed(_placementSO, _board);
            _heroSpawner = new HeroSpawner(_heroMover, _bench, _seed, _templateActions);
            _stateMachine = new GameStateMachine(this);
            _status = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IMatchStatusView>()
                .FirstOrDefault();
        }

        // start the game at preparation state
        void Start()
        {
            _stateMachine.Start(GamePhaseEnum.Preparation);
        }

        void Update() => _stateMachine.Tick();

        // ========================================= public function for controlling game state =========================================
        // FIXLATER: If we implement GameManager contract, Restart() and StartCombat() look like a good candidate memeber.
        // restart by re-loading the scene => the game is back to preparation state
        public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // if game is in preparation state, then go combat state
        public void StartCombat()
        {
            if (Phase != GamePhaseEnum.Preparation) return;

            // spawn player team if seedMode activate.
            // before the guard, so a seeded team counts as heroes on the board
            if (_seedMode) _seed.SpawnTeamOnBoard(TeamEnum.Blue);

            // guard
            bool hasHeroesOnBoard = _board.HeroesOnBoard.Any(h => h.Team == TeamEnum.Blue);
            if (!hasHeroesOnBoard)
            {
                DebugTool.LogWarning("Can't start combat - place at least one hero on the board first.");
                return;
            }

            ChangeState(GamePhaseEnum.Combat);
        }
    }
}
