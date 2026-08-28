using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Contracts;

namespace MagicSchool.UI
{
    /// <summary>
    /// The inspect panel on the right. Opens when the player right-clicks a inspectable
    /// e.g. hero, item.
    /// </summary>
    /// 
    /// FIXLATER: This isn't HeroPaenlController anymore, it was InspectorController
    /// Let do polymorphism to separate item and hero apart.
    [RequireComponent(typeof(UIDocument))]
    internal class HeroPanelController : MonoBehaviour, IInspectorPanel
    {
        [SerializeField] private VisualTreeAsset _heroPanelAsset;

        private VisualElement _panel;

        private Label _heroName, _heroTeam;
        private Label _hpLabel, _manaLabel;
        private VisualElement _hpBar, _manaBar, _statGrid;
        private VisualElement _hpFill, _manaFill;
        private Label _statAtk, _statDef, _statMag, _statMr, _statAs, _statRange;
        private VisualElement _activeSection, _passiveSection, _itemSection;
        private Label _activeName, _activeBody, _passiveBody, _itemBody;

        // === IInspectorPanel interface ===
        private IInspectable _shown;
        // when a hero is inspected, show hero panel for that hero
        public void Inspect(IInspectable target) => Show(target);

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

            _hpBar = _panel.Q<VisualElement>("HpBar");
            _manaBar = _panel.Q<VisualElement>("ManaBar");
            _statGrid = _panel.Q<VisualElement>("StatGrid");

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
            _itemSection = _panel.Q<VisualElement>("ItemSection");
            _itemBody = _panel.Q<Label>("ItemBody");
        }

        // update the panel according to whatever was just inspected
        private void Show(IInspectable unit)
        {
            // if the thing selected is already the inspected one, return.
            if (_panel != null && _shown != null && ReferenceEquals(_shown, unit)) return;

            _shown = unit;

            if (_panel == null) return;

            bool hasSomething = unit != null && unit.IsAlive;
            _panel.EnableInClassList("hero-panel--empty", !hasSomething);
            if (!hasSomething) return;

            // the one line both kinds share
            _heroName.text = unit.DisplayName;

            // // FLAGGING: polymorphism fix this
            // Which half of the panel this is depends on what came in, and the sections not
            // wanted are hidden rather than left showing a hero's blank stat grid over an item.
            if (unit is IInspectableHero hero) ShowHero(hero);
            else if (unit is IInspectableItem item) ShowItem(item);
        }

        // the hero half: team, bars, stat grid, ability text
        private void ShowHero(IInspectableHero hero)
        {
            SetShown(_itemSection, false);

            SetShown(_heroTeam, true);
            SetShown(_hpBar, true);
            SetShown(_manaBar, true);
            SetShown(_statGrid, true);

            _heroTeam.text = hero.Team.ToString().ToUpperInvariant();
            _statAtk.text = hero.AttackDamage.ToString();
            _statDef.text = hero.Defence.ToString();
            _statMag.text = hero.Magic.ToString();
            _statMr.text = hero.MagicResist.ToString();
            _statAs.text = hero.AttackSpeed.ToString("0.00");
            _statRange.text = hero.Range.ToString();

            ShowAbility(hero);
            Refresh();
        }

        // the item half: a name and what it does, with every hero-shaped row hidden
        private void ShowItem(IInspectableItem item)
        {
            SetShown(_heroTeam, false);
            SetShown(_hpBar, false);
            SetShown(_manaBar, false);
            SetShown(_statGrid, false);
            SetShown(_activeSection, false);
            SetShown(_passiveSection, false);

            SetShown(_itemSection, true);
            _itemBody.text = string.IsNullOrEmpty(item.Description)
                ? "No description written yet."
                : item.Description;
        }

        // update ability text in hero panel
        private void ShowAbility(IInspectableHero unit)
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

            // if it dies or is destroyed - never a null check, see IInspectable.IsAlive
            if (!_shown.IsAlive) { Show(null); return; }

            // FLAGGING: polymorphism fix this
            // only a hero has bars that move; an item's panel is static once shown
            if (_shown is IInspectableHero) Refresh();
        }

        // FLAGGING: those helper look so generic that it don't have to be in this class. Let leave it here for now.
        #region helper
        // set bar to hp and mana
        private void Refresh()
        {
            // FLAGGING: polymorphism fix this
            if (!(_shown is IInspectableHero hero)) return;

            SetBar(_hpFill, _hpLabel, hero.CurrentHP, hero.MaxHP);
            SetBar(_manaFill, _manaLabel, hero.CurrentMana, hero.MaxMana);
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
