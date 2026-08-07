using System.Linq;
using UnityEngine;

namespace MagicSchool
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BattleBoard _board;

        public GamePhase Phase { get; private set; } = GamePhase.Preparation;
        public Team? Winner { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            // BLOCKED on: a "Start Battle" UI button. => Now use manual space-bar to trigger the game for easy testing.
            if (Phase == GamePhase.Preparation && PlayerInputSystem.SpacePressedThisFrame)
            {
                StartCombat();
            }

            if (Phase == GamePhase.Combat)
            {
                CheckForWinner();
            }
        }

        public void StartCombat()
        {
            Phase = GamePhase.Combat;
        }

        private void CheckForWinner()
        {
            var alive = _board.HeroesOnBoard.Where(h => h.StateType != HeroStateType.Dead);
            bool blueAlive = alive.Any(h => h.Team == Team.Blue);
            bool redAlive = alive.Any(h => h.Team == Team.Red);

            if (blueAlive && redAlive) return;

            Winner = blueAlive ? Team.Blue : redAlive ? Team.Red : (Team?)null;
            Phase = GamePhase.Result;
        }
    }
}
