using System;

namespace MagicSchool
{
    // Spawn hero by skipping the bench logic part.
    // player normally go through bench logic first: buy hero => hero spawn on bench => drag hero to the board.
    // but for enemy side, their heroes just spawn on board directly, so they want to skip bench logic. 
    // P.S. player side can also use this though, because sometime we need to test the combat quickly, and want to skip the bench part.
    public class BattleBoardSeed
    {
        private BattlePlacementSO _placementSO;
        private BattleBoard _board;     // need to know which board, it'll seed on (there maybe several board at once)
        private bool _seedMode;
        public event Action<HeroDataSO, Team, Placement, BattleBoard> OnHeroSpawned;

        /// <summary>
        /// Counterpart to Bench.SpawnHeroOnBench().
        /// </summary>
        public void SpawnHeroOnBoard()
        {
            if (!_seedMode) return;
            if (_placementSO == null) { DebugTool.DebugLogConsole("Can't seed the hero. Hero Placement is null"); return; }

            foreach (var heroPlacement in _placementSO.HeroesPlacement)
            {
                HeroDataSO data = heroPlacement.dataSO;
                HexNumber placement = heroPlacement.hexPlacement;
                OnHeroSpawned.Invoke(data, placement.team, _board.Hexs[placement], _board);
            }
        }
    }
}

