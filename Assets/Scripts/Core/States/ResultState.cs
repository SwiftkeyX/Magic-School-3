using MagicSchool.Contracts;

namespace MagicSchool.Core.States
{
    /// <summary>
    /// The combat is over, annouce the winner.
    /// </summary>
    internal class ResultState : GameState
    {
        public override GamePhaseEnum StateType => GamePhaseEnum.Result;

        public ResultState(GameManager game) : base(game) { }

        public override void OnEnter()
        {
            _game.Board.SetBattleOn(false);

            _game.Status?.ShowResult(_game.Winner, _game.StageNumber, _game.StageCount, _game.IsRunCleared);
        }

        // if continue is requested, go to preparation state.
        // 1) if player won, go to next stage
        // 2) if player lose, restart this stage
        protected override void CheckSwitchState()
        {
            if (!_game.ConsumeContinueRequest()) return;

            bool playerWon = _game.Winner == TeamEnum.Blue;
            bool runCleared = _game.IsRunCleared;

            // if player won, go to next stage.
            // if plyaer lose, repeat this stage.
            if (playerWon) _game.SetStageIndex(runCleared ? 0 : _game.StageIndex + 1);

            // go preparation state
            _game.ChangeState(GamePhaseEnum.Preparation);
        }
    }
}
