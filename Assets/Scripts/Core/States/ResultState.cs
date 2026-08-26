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

        // What to continue to after a game result:
        // 1) if player won, go to next stage - or start the run over, if that was the last one
        // 2) if player lose, restart this stage
        protected override void CheckSwitchState()
        {
            if (!_game.ConsumeContinueRequest()) return;

            bool playerWon = _game.Winner == TeamEnum.Blue;

            // read before StartStage - it clears the winner, and IsRunCleared is derived from it
            bool runCleared = _game.IsRunCleared;

            if (playerWon) _game.StartStage(runCleared ? 0 : _game.StageIndex + 1);
            else _game.StartStage(_game.StageIndex);
        }
    }
}
