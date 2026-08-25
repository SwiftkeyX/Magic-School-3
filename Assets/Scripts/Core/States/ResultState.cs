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

            _game.Status?.ShowResult(_game.Winner);
        }
    }
}
