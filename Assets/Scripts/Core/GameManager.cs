using System.Linq;
using UnityEngine;

namespace MagicSchool
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BattleBoard _board;
        [SerializeField] private Bench _bench;
        [SerializeField] private BattlePlacementSO _placementSO;
        [SerializeField] private bool _seedMode;

        public GamePhase Phase { get; private set; } = GamePhase.Preparation;
        public Team? Winner { get; private set; }

        // ======================== composition root ========================
        private HeroMover _heroMover;
        private BattleBoardSeed _seed;
        private HeroSpawner _heroSpawner;

        void Awake()
        {
            Instance = this;

            _heroMover = new HeroMover();
            _seed = new BattleBoardSeed(_placementSO, _board, _seedMode);
            _heroSpawner = new HeroSpawner(_heroMover, _bench, _seed);
        }

        // The system moving a hero, as opposed to a hero walking itself during combat.
        public void MoveHero(Hero hero, Placement placement) => _heroMover.MoveThisHeroTo(hero, placement);

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

            _seed.SpawnHeroOnBoard();
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
