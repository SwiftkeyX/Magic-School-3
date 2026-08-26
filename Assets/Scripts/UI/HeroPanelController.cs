using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Contracts;

namespace MagicSchool.UI
{
    /// <summary>
    /// The inspect panel on the right. Opens when the player right-clicks a hero
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    internal class HeroPanelController : MonoBehaviour, IInspectorPanel
    {
        [SerializeField] private VisualTreeAsset _heroPanelAsset;

        private VisualElement _panel;

        private Label _heroName, _heroTeam;
        private Label _hpLabel, _manaLabel;
        private VisualElement _hpFill, _manaFill;
        private Label _statAtk, _statDef, _statMag, _statMr, _statAs, _statRange;
        private VisualElement _activeSection, _passiveSection;
        private Label _activeName, _activeBody, _passiveBody;

        // === IInspectorPanel interface ===
        private IInspectable _shown;
        // when a hero is inspected, show hero panel for that hero
        public void Inspect(IInspectable hero) => Show(hero);

        // init hero panel
        private void OnEnable()
        {
            // get main screen panel
            UIDocument document = GetComponent<UIDocument>();
            VisualElement mainPanel = document.rootVisualElement;
            if (mainPanel == null || _heroPanelAsset == null) return;

            // put itself in the main panel
            _panel = PanelMounter.MountInMainPanel(mainPanel, _heroPanelAsset);
            if (_panel == null) return;

            CacheElements();

            // nothing is selected until the player right-clicks something
            Show(null);
        }

        // get reference to every text in this hero panel
        private void CacheElements()
        {
            _heroName = _panel.Q<Label>("HeroName");
            _heroTeam = _panel.Q<Label>("HeroTeam");

            _hpFill = _panel.Q<VisualElement>("HpFill");
            _hpLabel = _panel.Q<Label>("HpLabel");
            _manaFill = _panel.Q<VisualElement>("ManaFill");
            _manaLabel = _panel.Q<Label>("ManaLabel");

            _statAtk = _panel.Q<Label>("StatAtk");
            _statDef = _panel.Q<Label>("StatDef");
            _statMag = _panel.Q<Label>("StatMag");
            _statMr = _panel.Q<Label>("StatMr");
            _statAs = _panel.Q<Label>("StatAs");
            _statRange = _panel.Q<Label>("StatRange");

            _activeSection = _panel.Q<VisualElement>("ActiveSection");
            _activeName = _panel.Q<Label>("ActiveName");
            _activeBody = _panel.Q<Label>("ActiveBody");
            _passiveSection = _panel.Q<VisualElement>("PassiveSection");
            _passiveBody = _panel.Q<Label>("PassiveBody");
        }

        // update hero panel according to new hero data consumed
        private void Show(IInspectable unit)
        {
            // if the hero selected is the currently the inspected on, return.
            if (_panel != null && _shown != null && ReferenceEquals(_shown, unit)) return;

            _shown = unit;

            if (_panel == null) return;

            bool hasHero = unit != null && unit.IsAlive;
            _panel.EnableInClassList("hero-panel--empty", !hasHero);
            if (!hasHero) return;

            _heroName.text = unit.HeroName;
            _heroTeam.text = unit.Team.ToString().ToUpperInvariant();
            _statAtk.text = unit.AttackDamage.ToString();
            _statDef.text = unit.Defence.ToString();
            _statMag.text = unit.Magic.ToString();
            _statMr.text = unit.MagicResist.ToString();
            _statAs.text = unit.AttackSpeed.ToString("0.00");
            _statRange.text = unit.Range.ToString();

            ShowAbility(unit);
            Refresh();
        }

        // update ability text in hero panel
        private void ShowAbility(IInspectable unit)
        {
            bool hasActive = unit.HasSkill;
            SetShown(_activeSection, hasActive);
            if (hasActive)
            {
                _activeName.text = unit.SkillName;
                _activeBody.text = string.IsNullOrEmpty(unit.SkillDescription)
                    ? "No description written yet."
                    : unit.SkillDescription;
            }

            // only two heroes have one - an empty PASSIVE heading on the other fifteen reads as a bug
            bool hasPassive = unit.HasPassive;
            SetShown(_passiveSection, hasPassive);
            if (hasPassive)
            {
                _passiveBody.text = string.IsNullOrEmpty(unit.PassiveDescription)
                    ? "No description written yet."
                    : unit.PassiveDescription;
            }
        }

        // Some stat are always change dynamically 
        // e.g. update HP and Mana every frame
        private void Update()
        {
            if (_shown == null) return;

            // if hero dies or is destroyed - never a null check, see IInspectable.IsAlive
            if (!_shown.IsAlive) { Show(null); return; }

            Refresh();
        }

        // FLAGGING: those helper look so generic that it don't have to be in this class. Let leave it here for now.
        #region helper
        // set bar to hp and mana
        private void Refresh()
        {
            SetBar(_hpFill, _hpLabel, _shown.CurrentHP, _shown.MaxHP);
            SetBar(_manaFill, _manaLabel, _shown.CurrentMana, _shown.MaxMana);
        }

        private static void SetBar(VisualElement fill, Label label, int current, int max)
        {
            float percent = max <= 0 ? 0f : Mathf.Clamp01((float)current / max) * 100f;
            fill.style.width = Length.Percent(percent);
            label.text = $"{current} / {max}";
        }

        private static void SetShown(VisualElement element, bool shown)
        {
            element.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
        }
        #endregion
    }
}
