using System.Linq;
using MagicSchool.Contracts;

namespace MagicSchool.Core.States
{
    /// <summary>
    /// The fight. Heroes act on their own from here; this only watches for it being over.
    /// </summary>
    internal class CombatState : GameState
    {
        public override GamePhaseEnum StateType => GamePhaseEnum.Combat;

        public CombatState(GameManager game) : base(game) { }

        public override void OnEnter()
        {
            // FIXLATER: Set battleon seem ugly to me. Since it's use exactly like a _gameManager.Phase.
            // But the deal with it, is that Hero module can't ref to Core module, else it would casue circular dependency.
            // How about using contract for GameManager instead, so Hero can ref to current phase via the contract.
            _game.Board.SetBattleOn(true);

            _game.Status?.ShowCombat();
        }

        public override void OnUpdate()
        {
            CheckForWinner();
        }

        private void CheckForWinner()
        {
            var alive = _game.Board.HeroesOnBoard.Where(h => h.StateType != HeroStateEnum.Dead);
            bool blueAlive = alive.Any(h => h.Team == TeamEnum.Blue);
            bool redAlive = alive.Any(h => h.Team == TeamEnum.Red);

            if (blueAlive && redAlive) return;

            _game.SetWinner(blueAlive ? TeamEnum.Blue : redAlive ? TeamEnum.Red : (TeamEnum?)null);
            _game.ChangeState(GamePhaseEnum.Result);
        }
    }
}
