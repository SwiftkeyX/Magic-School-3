using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Contracts;

namespace MagicSchool.UI
{
    /// <summary>
    /// The hint line and the win banner
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    internal class MatchStatusController : MonoBehaviour, IBannerPanel
    {
        [SerializeField] private VisualTreeAsset _matchStatusAsset;

        private VisualElement _panel;
        private VisualElement _banner;
        private Label _resultText;
        private Label _phaseHint;

        // =================================== IMatchStatusView interface ===================================
        public void ShowPreparation(int stage, int stageCount)
        {
            SetHint($"{Stage(stage, stageCount)} - drag heroes onto your hexes, then press SPACE to fight");
            SetShown(_banner, false);
        }

        public void ShowCombat(int stage, int stageCount)
        {
            SetHint($"{Stage(stage, stageCount)} - fighting. Right-click a hero to inspect it");
            SetShown(_banner, false);
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

            SetShown(_banner, true);
        }

        // init the status layer
        private void OnEnable()
        {
            UIDocument document = GetComponent<UIDocument>();
            VisualElement mainPanel = document.rootVisualElement;
            if (mainPanel == null || _matchStatusAsset == null) return;

            _panel = PanelMounter.MountInMainPanel(mainPanel, _matchStatusAsset);
            if (_panel == null) return;

            _banner = _panel.Q<VisualElement>("ResultBanner");
            _resultText = _panel.Q<Label>("ResultText");
            _phaseHint = _panel.Q<Label>("PhaseHint");

            // the UXML is authored with the banner visible so it can be laid out in UI Builder;
            // a match always starts in preparation on the first stage, with nothing won yet
            ShowPreparation(1, 1);
        }

        private static string Stage(int stage, int stageCount)
        {
            return stageCount > 1 ? $"Stage {stage} of {stageCount}" : "Stage";
        }

        private void SetHint(string text)
        {
            if (_phaseHint != null) _phaseHint.text = text;
        }

        private static void SetShown(VisualElement element, bool shown)
        {
            if (element == null) return;
            element.EnableInClassList("is-hidden", !shown);
        }
    }
}
