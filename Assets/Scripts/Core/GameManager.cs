using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Combat.Placements;
using MagicSchool.Core.States;
using MagicSchool.Skills;

namespace MagicSchool.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ============================================= dependency =============================================
        [SerializeField] private BattleBoard _board;
        [SerializeField] private Bench _bench;
        [SerializeField] private BattlePlacementSO _currentStage;
        [SerializeField] private BattlePlacementSO[] _stages;
        [SerializeField] private TemplateActionRegistrySO _templateActions;
        [SerializeField] private bool _isPlayerSeed;
        private HeroMover _heroMover;
        private BattleBoardSeed _seed;
        private HeroSpawner _heroSpawner;
        private GameStateMachine _stateMachine;
        private IBannerPanel _status;
        private int _stageIndex;
        private bool _startCombatRequested;
        private bool _continueRequested;

        // ======================================== getter ========================================
        public GamePhaseEnum Phase => _stateMachine == null ? GamePhaseEnum.Preparation : _stateMachine.CurrentType;
        internal TeamEnum? Winner { get; private set; }
        internal bool IsPlayerSeed => _isPlayerSeed;
        internal int StageIndex => _stageIndex;
        internal bool HasStages => _stages != null && _stages.Length > 0;
        internal int StageCount => HasStages ? _stages.Length : 1;
        internal int StageNumber => _stageIndex + 1;
        internal bool IsRunCleared => Winner == TeamEnum.Blue && _stageIndex + 1 >= StageCount;

        // ======================================== setter ========================================
        internal BattlePlacementSO GetStage(int index) => HasStages ? _stages[index] : _currentStage;
        internal void SetWinner(TeamEnum? winner) => Winner = winner;

        // === forwarding ===
        internal void ChangeState(GamePhaseEnum next) => _stateMachine.ChangeState(next);
        public void MoveHero(ICombatant hero, IPlacement placement) => _heroMover.MoveThisHeroTo(hero, placement);
        internal BattleBoard Board => _board;
        internal BattleBoardSeed Seed => _seed;
        internal IBannerPanel Status => _status;

        // ======================================== life cycle ========================================
        // init dependency
        void Awake()
        {
            Instance = this;

            _heroMover = new HeroMover();
            _seed = new BattleBoardSeed(GetStage(_stageIndex), _board);
            _heroSpawner = new HeroSpawner(_heroMover, _bench, _seed, _templateActions);
            _stateMachine = new GameStateMachine(this);
            _status = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IBannerPanel>()
                .FirstOrDefault();
        }

        // start the game at preparation state
        void Start()
        {
            _stateMachine.Start(GamePhaseEnum.Preparation);
        }

        void Update() => _stateMachine.Tick();

        // ========================================= request handler =========================================
        public void StartCombat()
        {
            if (Phase != GamePhaseEnum.Preparation) return;

            _startCombatRequested = true;
        }

        public void ContinueFromResult()
        {
            if (Phase != GamePhaseEnum.Result) return;

            _continueRequested = true;
        }

        public void Restart()
        {
            StartStage(_stageIndex);
        }

        // consume a request, then clear the flag to false immediately:
        // 1) start combat => let hero fight between heroes
        // 2) continue => move to a next stage, or repeat the same stage
        internal bool ConsumeStartCombatRequest() => Consume(ref _startCombatRequested);
        internal bool ConsumeContinueRequest() => Consume(ref _continueRequested);

        // if request exist, consume it, and clear the flag
        private static bool Consume(ref bool requested)
        {
            if (!requested) return false;

            requested = false;
            return true;
        }

        // ========================================= other =========================================
        // start new stage
        internal void StartStage(int index)
        {
            _stageIndex = Mathf.Clamp(index, 0, StageCount - 1);
            
            // reset var
            Winner = null;
            _startCombatRequested = false;
            _continueRequested = false;

            // wipe all enemies
            _board.ClearTeam(TeamEnum.Red);

            // ...
            if (_isPlayerSeed) _board.ClearTeam(TeamEnum.Blue);
            else _board.ReviveTeam(TeamEnum.Blue);

            // switch to next enemies seed
            _seed.SwitchSeed(GetStage(_stageIndex));

            ChangeState(GamePhaseEnum.Preparation);
        }
    }
}
