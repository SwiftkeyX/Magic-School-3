using System.Linq;
using MagicSchool.Contracts;
using MagicSchool.Engine;

namespace MagicSchool.Core.States
{
    /// <summary>
    /// Buying, and putting heroes on the board. Nothing fights.
    /// Left only when the player asks for it - see GameManager.StartCombat.
    /// </summary>
    internal class PreparationState : GameState
    {
        public override GamePhaseEnum StateType => GamePhaseEnum.Preparation;

        public PreparationState(GameManager game) : base(game) { }

        public override void OnEnter()
        {
            _game.Board.SetBattleOn(false);

            // spawn player team if seedMode activate.
            if (_game.IsPlayerSeed) _game.Seed.SpawnTeamOnBoard(TeamEnum.Blue);

            // spawn enemy team in preparation state
            _game.Seed.SpawnTeamOnBoard(TeamEnum.Red);

            _game.Status?.ShowPreparation(_game.StageNumber, _game.StageCount);
        }

        // if there's heroes from the player side on the board, allow to start combat
        protected override void CheckSwitchState()
        {
            if (!_game.ConsumeStartCombatRequest()) return;

            bool hasHeroesOnBoard = _game.Board.HeroesOnBoard.Any(h => h.Team == TeamEnum.Blue);
            if (!hasHeroesOnBoard)
            {
                DebugTool.LogWarning("Can't start combat - place at least one hero on the board first.");
                return;
            }

            _game.ChangeState(GamePhaseEnum.Combat);
        }
    }
}
