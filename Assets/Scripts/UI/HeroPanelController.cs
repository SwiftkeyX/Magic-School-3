using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Contracts;
using MagicSchool.Combat.Heroes;

namespace MagicSchool.UI
{
    /// <summary>
    /// The inspect panel on the right. Opens when the player right-clicks a hero
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    internal class HeroPanelController : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset _heroPanelAsset;

        private VisualElement _panel;

        private Label _heroName, _heroTeam;
        private Label _hpLabel, _manaLabel;
        private VisualElement _hpFill, _manaFill;
        private Label _statAtk, _statDef, _statMag, _statMr, _statAs, _statRange;
        private VisualElement _activeSection, _passiveSection;
        private Label _activeName, _activeBody, _passiveBody;

        private Hero _shown;

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

            // when the hero selected is changed, update hero panel too.
            HeroSelection.Changed += Show;
            Show(HeroSelection.Selected);
        }

        private void OnDisable() => HeroSelection.Changed -= Show;

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

        // update hero panel accordign to new hero data consume
        private void Show(Hero hero)
        {
            _shown = hero;

            if (_panel == null) return;

            bool hasHero = hero != null && hero.IsInitialized;
            _panel.EnableInClassList("hero-panel--empty", !hasHero);
            if (!hasHero) return;

            _heroName.text = hero.HeroName;
            _heroTeam.text = hero.Team.ToString().ToUpperInvariant();

            _statAtk.text = hero.AttackDamage.ToString();
            _statDef.text = Mathf.RoundToInt(hero.GetStat(StatEnum.DF)).ToString();
            _statMag.text = Mathf.RoundToInt(hero.GetStat(StatEnum.MG)).ToString();
            _statMr.text = Mathf.RoundToInt(hero.GetStat(StatEnum.MR)).ToString();
            _statAs.text = hero.AttackSpeed.ToString("0.00");
            _statRange.text = hero.Range.ToString();

            ShowAbility(hero);
            Refresh();
        }

        // update ability text in hero panel
        private void ShowAbility(Hero hero)
        {
            bool hasActive = hero.HasSkill;
            SetShown(_activeSection, hasActive);
            if (hasActive)
            {
                _activeName.text = hero.SkillName;
                _activeBody.text = string.IsNullOrEmpty(hero.SkillDescription)
                    ? "No description written yet."
                    : hero.SkillDescription;
            }

            // only two heroes have one - an empty PASSIVE heading on the other fifteen reads as a bug
            bool hasPassive = hero.HasPassive;
            SetShown(_passiveSection, hasPassive);
            if (hasPassive)
            {
                _passiveBody.text = string.IsNullOrEmpty(hero.PassiveDescription)
                    ? "No description written yet."
                    : hero.PassiveDescription;
            }
        }

        // Some stat are always change dynamically 
        // e.g. update HP and Mana every frame
        private void Update()
        {
            if (_shown == null) return;

            // if hero dies
            if (!_shown.IsInitialized) { HeroSelection.Clear(); return; }

            Refresh();
        }

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
    }
}
