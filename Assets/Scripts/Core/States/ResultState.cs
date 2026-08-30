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

            _game.Hint?.ShowResult(_game.Winner, _game.StageNumber, _game.StageCount, _game.IsRunCleared);
            _game.Banner?.ShowResult(_game.Winner, _game.StageNumber, _game.StageCount, _game.IsRunCleared);

            // if player win, show reward panel
            if (_game.Winner == TeamEnum.Blue) _game.Reward?.ShowReward();
        }

        public override void OnExit()
        {
            // close reward panel
            _game.Reward?.SetShown(false);
        }

        // if continue is requested, go to preparation state.
        // 1) if player won, go to next stage
        // 2) if player lose, restart this stage
        protected override void CheckSwitchState()
        {
            // if the reward isn't choose yet, return
            if (_game.Reward != null && _game.Reward.IsChoosing) return;

            // polling for player's continue request
            if (!_game.ConsumeContinueRequest()) return;

            bool playerWon = _game.Winner == TeamEnum.Blue;
            bool runCleared = _game.IsRunCleared;

            // if player won, go to next stage.
            // if plyaer lose, repeat this stage.
            if (playerWon)
            {
                _game.SetStageIndex(runCleared ? 0 : _game.StageIndex + 1);

                // FLAGGING: temporarily, this is to expand hero limit easily for demo version
                // clearing the run loops back to stage 1, so the team it is fought with
                // loops back too - otherwise the replay starts oversized against stage 1.
                if (runCleared) _game.ResetHeroLimit();
                else _game.GrowHeroLimit();
            }

            // go preparation state
            _game.ChangeState(GamePhaseEnum.Preparation);
        }
    }
}
