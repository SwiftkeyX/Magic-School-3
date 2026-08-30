using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Contracts;

namespace MagicSchool.UI
{
    /// <summary>
    /// everything layered over the board.
    /// e.g. The hint line, the win banner, and the hero counter 
    /// </summary>
    internal class MatchStatusController : PanelController, IBannerPanel, IHeroCountPanel
    {
        private VisualElement _banner;
        private VisualElement _heroCount;
        private Label _resultText;
        private Label _phaseHint;
        private Label _heroCountValue;

        // =================================== IBannerPanel interface ===================================
        public void ShowPreparation(int stage, int stageCount)
        {
            SetHint($"{Stage(stage, stageCount)} - drag heroes onto your hexes, then press SPACE to fight");
            SetShown(false);
        }

        public void ShowCombat(int stage, int stageCount)
        {
            SetHint($"{Stage(stage, stageCount)} - fighting. Right-click a hero to inspect it");
            SetShown(false);
        }

        public void ShowResult(TeamEnum? winner, int stage, int stageCount, bool runCleared)
        {
            // one key does everything from here, so the hint only has to say what it leads to
            if (runCleared) SetHint("press SPACE to run it again from stage 1");
            else if (winner == TeamEnum.Blue) SetHint($"press SPACE for stage {stage + 1}");
            else SetHint($"press SPACE to retry {Stage(stage, stageCount).ToLowerInvariant()}");

            if (_resultText == null) return;

            // one class at a time, so a second result does not inherit the first one's colour
            _resultText.RemoveFromClassList("banner__text--blue");
            _resultText.RemoveFromClassList("banner__text--red");
            _resultText.RemoveFromClassList("banner__text--draw");

            if (winner == TeamEnum.Blue)
            {
                _resultText.text = runCleared ? "RUN CLEARED" : $"STAGE {stage} CLEARED";
                _resultText.AddToClassList("banner__text--blue");
            }
            else if (winner == TeamEnum.Red)
            {
                _resultText.text = "DEFEATED";
                _resultText.AddToClassList("banner__text--red");
            }
            else
            {
                // nobody left standing on either side
                _resultText.text = "DRAW";
                _resultText.AddToClassList("banner__text--draw");
            }

            SetShown(true);
        }

        // =================================== IHeroCountPanel interface ===================================
        public void ShowHeroCount(int placed, int limit)
        {
            if (_heroCountValue != null) _heroCountValue.text = $"{placed} / {limit}";

            // warn at the limit, not past it - a drop that would exceed it is simply refused,
            // so the count never actually reads higher than the limit
            if (_heroCount != null) _heroCount.EnableInClassList("hero-count--full", placed >= limit);

            SetShown(true);
        }

        public void HideHeroCount()
        {
            SetShown(false);
        }

        // =================================== Life cycle ===================================
        // OnMounted, get all element, show preparation banner
        protected override void OnMounted(VisualElement panel)
        {
            _banner = panel.Q<VisualElement>("ResultBanner");
            _resultText = panel.Q<Label>("ResultText");
            _phaseHint = panel.Q<Label>("PhaseHint");
            _heroCount = panel.Q<VisualElement>("HeroCount");
            _heroCountValue = panel.Q<Label>("HeroCountValue");

            ShowPreparation(stage: 1, stageCount: 1);
            HideHeroCount();
        }

        // =================================== private ===================================
        private static string Stage(int stage, int stageCount)
        {
            return stageCount > 1 ? $"Stage {stage} of {stageCount}" : "Stage";
        }

        private void SetHint(string text)
        {
            if (_phaseHint != null) _phaseHint.text = text;
        }

    }
}
