using MagicSchool.Combat.Placements;
using MagicSchool.Contracts;
using MagicSchool.Core;

namespace MagicSchool.Player
{
    internal class PlayerTeamSize
    {
        private IHeroCountPanel _heroCountPanel;
        private BattleBoard _board;
        private int _shownHeroCount;
        private int _shownHeroLimit => GameManager.Instance.HeroLimit;

        public int ShownHeroCount => _shownHeroCount;

        internal PlayerTeamSize(IHeroCountPanel heroCountPanel, BattleBoard board)
        {
            _heroCountPanel = heroCountPanel;
            _board = board;
            RefreshHeroCountPanel();
        }

        public void RefreshHeroCountPanel()
        {
            if (_heroCountPanel == null) return;

            // if not in preparation state, close the hero count panel
            if (GameManager.Instance.Phase != GamePhaseEnum.Preparation)
            {
                _heroCountPanel.SetShown(false);
                return;
            }

            // open hero count panel
            _shownHeroCount = _board.CountTeamOnBoard(TeamEnum.Blue);
            _heroCountPanel.ShowHeroCount(_shownHeroCount, _shownHeroLimit);
        }

        public bool IsAddingOK()
        {
            if (_shownHeroCount >= _shownHeroLimit) return false;

            return true;
        }
    }
}