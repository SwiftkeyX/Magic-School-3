namespace MagicSchool.Core.States
{

    /// <summary>
    /// Control life cycle of GameManager's phase
    /// The phase = { Preparation, Combat, Result }
    /// </summary>
    internal class GameStateMachine
    {
        private readonly PreparationState _preparation;
        private readonly CombatState _combat;
        private readonly ResultState _result;

        public GameState Current { get; private set; }

        public GamePhaseEnum CurrentType => Current == null ? GamePhaseEnum.Preparation : Current.StateType;

        public GameStateMachine(GameManager game)
        {
            _preparation = new PreparationState(game);
            _combat = new CombatState(game);
            _result = new ResultState(game);
        }

        public void Start(GamePhaseEnum initial)
        {
            Current = GetState(initial);
            Current.OnEnter();
        }

        public void ChangeState(GamePhaseEnum next)
        {
            if (Current != null && next == CurrentType) return;

            Current?.OnExit();
            Current = GetState(next);
            Current.OnEnter();
        }

        public void Tick()
        {
            if (Current == null) return;

            Current.OnUpdate();
        }

        private GameState GetState(GamePhaseEnum type)
        {
            switch (type)
            {
                case GamePhaseEnum.Preparation: return _preparation;
                case GamePhaseEnum.Combat: return _combat;
                case GamePhaseEnum.Result: return _result;
                default: return null;
            }
        }
    }
}
