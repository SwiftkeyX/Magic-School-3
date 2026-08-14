using System;
using MagicSchool.Engine;
using MagicSchool.Contracts;
using MagicSchool.Combat.Heroes;

namespace MagicSchool.Combat.Placements
{
    // Spawn hero by skipping the bench logic part.
    // player normally go through bench logic first: buy hero => hero spawn on bench => drag hero to the board.
    // but for enemy side, their heroes just spawn on board directly, so they want to skip bench logic.
    // P.S. player side can also use this though, because sometime we need to test the combat quickly, and want to skip the bench part.
    public class BattleBoardSeed
    {
        private readonly BattlePlacementSO _placementSO;
        private readonly BattleBoard _board;     // need to know which board, it'll seed on (there maybe several board at once)
        private readonly bool _seedMode;

        // Raised to ask for a hero. Fires BEFORE anything is spawned - HeroSpawner does the work.
        public event Action<HeroDataSO, TeamEnum, IPlacement, BattleBoard> OnSpawnRequested;

        public BattleBoardSeed(BattlePlacementSO placementSO, BattleBoard board, bool seedMode)
        {
            _placementSO = placementSO;
            _board = board;
            _seedMode = seedMode;
        }

        /// <summary>
        /// Counterpart to Bench.SpawnHeroOnBench().
        /// </summary>
        public void SpawnHeroOnBoard()
        {
            if (!_seedMode) return;
            if (_placementSO == null) { DebugTool.LogError("Can't seed the hero. Hero Placement is null"); return; }
            if (OnSpawnRequested == null) { DebugTool.LogError("Can't seed the hero. Nothing is listening for spawn requests."); return; }

            foreach (var heroPlacement in _placementSO.HeroesPlacement)
            {
                HeroDataSO data = heroPlacement.dataSO;
                HexNumber placement = heroPlacement.hexPlacement;
                OnSpawnRequested.Invoke(data, placement.team, _board.Hexs[placement], _board);
            }
        }
    }
}
