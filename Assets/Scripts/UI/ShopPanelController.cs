using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Heroes;
using MagicSchool.Placements;

namespace MagicSchool.UI
{
    /// <summary>
    /// Use a empty main screen panel, then add each panel later:
    /// 1) Shop Panel
    /// 2) ...
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ShopPanelController : MonoBehaviour
    {
        // ================= SerializeField ======================
        [SerializeField] private VisualTreeAsset _shopPanelAsset;
        [SerializeField] private VisualTreeAsset _ghostAsset;
        [SerializeField] private List<HeroDataSO> _heroDataSOs;
        [SerializeField] private Bench _bench;

        // ================= VisualElement ======================
        private VisualElement _shopPanel;
        private VisualElement _mainPanel;
        private VisualElement _ghost;
        private Dictionary<VisualElement, HeroDataSO> _heroSlotsDict = new Dictionary<VisualElement, HeroDataSO>();
        private List<VisualElement> _heroSlots;

        // ================== etc =======================
        private bool _isDragging = false;

        // ================= setter & getter ===================
        // ...

        #region Initialize Panel
        private void OnEnable()
        {
            // get main screen panel
            UIDocument _main = GetComponent<UIDocument>();
            _mainPanel = _main.rootVisualElement;
            if (_mainPanel == null || _shopPanelAsset == null) return;

            // Put small modular panel in main panel
            VisualElement shopPanel = PutThisPanelInMainPanel(_shopPanelAsset);

            // get the reference for later use
            _shopPanel = shopPanel.Q<VisualElement>("ShopPanel");

            InitializeGhost();

            // Make hero slot draggable
            MakeShopUIDraggable();

            // Wire up the "Refresh" button to re-roll all hero slots
            MakeRefreshButtonWork();
        }

        // CloneTree() wraps a template's root in a TemplateContainer, and any <Style src="...">
        // referenced in its UXML attaches to that wrapper - not to the root itself. Copy it over
        // before discarding the wrapper, or the clone silently loses its stylesheet.
        private VisualElement CloneTemplateRoot(VisualTreeAsset asset)
        {
            VisualElement wrapper = asset.CloneTree();
            VisualElement root = wrapper[0];

            for (int i = 0; i < wrapper.styleSheets.count; i++)
            {
                root.styleSheets.Add(wrapper.styleSheets[i]);
            }

            root.RemoveFromHierarchy();
            return root;
        }

        private VisualElement PutThisPanelInMainPanel(VisualTreeAsset panelTree)
        {
            VisualElement panel = CloneTemplateRoot(panelTree);

            // use this panel's name - to find where it should be put inside the "main panel"
            VisualElement thisPanelWhereAboutInMainPanel = _mainPanel.Q<VisualElement>(panel.name);
            if (thisPanelWhereAboutInMainPanel == null)
            {
                Debug.LogWarning($"ShopPanelController: no element named '{panel.name}' found in the main document.");
                return null;
            }

            // put the shop panel in "it"
            thisPanelWhereAboutInMainPanel.Clear();
            thisPanelWhereAboutInMainPanel.Add(panel);

            return panel;
        }
        #endregion

        #region Drag Hero Function
        /// <summary>
        /// Main function for dragging
        /// A lot of comment since I never use those event before
        /// </summary>
        private void MakeShopUIDraggable()
        {
            // get all slots exist from the shop panel
            _heroSlots = _shopPanel.Query<VisualElement>("HeroSlot").ToList();

            // assign hero data to each slots, in order, for the initial roll
            for (int i = 0; i < _heroSlots.Count; i++)
            {
                if (i >= _heroDataSOs.Count) { Debug.LogError("HeroDataSO is not enough for all slot in shop panel"); return; }
                AssignHeroToSlot(_heroSlots[i], _heroDataSOs[i]);
            }

            // register event to every slot.
            foreach (var slot in _heroSlots)
            {
                // when click on heroslot, spawn ghost, move ghost to click point
                HeroSlotOnClick(slot);

                // when hold on heroslot, move ghost to hold point, create dragging logic visually
                HeroSlotOnMove(slot);

                // when your mouse release from holding, resolve buy/cancel based on the release point
                HeroSlotOnRelease(slot);
            }
        }

        // put a hero's data into a slot: what dragging/buying reads, and what's shown as its label.
        // A null data means "empty" (e.g. just bought). The slot stays in the layout at full size
        // either way (display is never toggled) so buying one doesn't reflow its siblings - it just
        // goes dim and unlabeled, which is derived here in one place rather than a separate flag.
        private void AssignHeroToSlot(VisualElement slot, HeroDataSO data)
        {
            _heroSlotsDict[slot] = data;

            slot.style.opacity = data == null ? 0.35f : 1f;

            Label nameLabel = slot.Q<Label>();
            if (nameLabel != null) nameLabel.text = data != null ? data.Name : string.Empty;
        }

        // ============================== Pointer Event ====================================
        private void HeroSlotOnClick(VisualElement heroSlot)
        {
            // PointerDownEvent = when your mouse hold inside heroslot bound
            // pointer = the point you start clicking
            heroSlot.RegisterCallback<PointerDownEvent>(pointer =>
            {
                // empty slot (already bought) has nothing to drag
                if (_heroSlotsDict[heroSlot] == null) return;

                _isDragging = true;

                // CapturePointer = All event from "heroslot" will continue working even though the "pointer" move out of bound
                heroSlot.CapturePointer(pointer.pointerId);

                // add ghost to main UI, this make ghost visible
                // ghost = the element that visually got drag together with your mouse e.g. hero sprite.
                // technically, ghost is element that copy your mouse pointer's position.
                _mainPanel.Add(_ghost);

                // move ghost to the point you just click
                MoveGhostTo(pointer.position);
            });
        }

        private void HeroSlotOnMove(VisualElement heroSlot)
        {
            // PointerMoveEvent = when you move your mouse inside heroslot bound
            // if your mouse exit heroslot bound, the event won't fired, BUT we use CapturePointer() so we can actually move out of bound
            // pointer = the point you holding your mouse, so this pointer can move
            heroSlot.RegisterCallback<PointerMoveEvent>(pointer =>
            {
                if (!_isDragging) return;

                // move ghost using pointer
                MoveGhostTo(pointer.position);
            });
        }

        private void HeroSlotOnRelease(VisualElement heroSlot)
        {
            // PointerUpEvent = when your mouse release from holding
            // pointer = the point you release your mouse
            heroSlot.RegisterCallback<PointerUpEvent>(pointer =>
            {
                _isDragging = false;

                // undo the CapturePointer
                heroSlot.ReleasePointer(pointer.pointerId);

                // release ghost from main UI, this make ghost go invisible
                _ghost?.RemoveFromHierarchy();

                // released back inside the shop bound = cancel, released outside = buy
                ResolveDrop(heroSlot, pointer.position);
            });
        }

        // ============================== Ghost ====================================
        // dragged-hero preview that follows the pointer - cloned from Ghost.uxml/.ghost in
        // Shop.uss instead of being hand-built in C#, same pattern as the shop panel itself.
        private void InitializeGhost()
        {
            _ghost = CloneTemplateRoot(_ghostAsset);
        }

        // ghost copying the mouse position using "screen panel method"
        private void MoveGhostTo(Vector2 panelPosition)
        {
            _ghost.style.left = panelPosition.x - _ghost.resolvedStyle.width / 2f;
            _ghost.style.top = panelPosition.y - _ghost.resolvedStyle.height / 2f;
        }

        // =========================== Buy / cancel on release ===============================
        private void ResolveDrop(VisualElement slot, Vector2 releasePosition)
        {
            // pointer.position and worldBound are both in the same panel coordinate space, no screen/world conversion needed
            bool releasedInsideShop = _shopPanel.worldBound.Contains(releasePosition);
            if (releasedInsideShop)
            {
                Debug.Log("Buy cancelled - dropped back inside the shop.");
                return;
            }

            // BLOCKED on: the gold/economy system. => Put spend gold logic here once it exists.
            bool bought = BuyHero(_heroSlotsDict[slot]);
            if (!bought) return;

            Debug.Log($"Bought hero from slot '{_heroSlotsDict[slot]}'.");

            // slot's hero is gone - clearing its data dims it and blocks re-buying (see AssignHeroToSlot)
            AssignHeroToSlot(slot, null);
        }
        #endregion

        #region etc
        private bool BuyHero(HeroDataSO data)
        {
            return _bench.SpawnHeroOnBench(data);
        }
        #endregion

        #region Refresh
        private void MakeRefreshButtonWork()
        {
            // "Refresh" is one of the two refresh-slot boxes on the left (the other is "Lock", not wired up yet)
            VisualElement randomSlot = _shopPanel.Q<VisualElement>("RandomSlot");
            if (randomSlot == null) return;

            foreach (var child in randomSlot.Children())
            {
                Label label = child.Q<Label>();
                if (label == null || label.text != "Refresh") continue;

                child.RegisterCallback<PointerUpEvent>(pointer => RerollShop());
                break;
            }
        }

        // Re-roll every hero slot with a random hero from the full roster (repeats allowed,
        // kept simple) - AssignHeroToSlot un-hides any slot that had been bought previously,
        // since its visibility is always derived from having data, never toggled separately.
        private void RerollShop()
        {
            foreach (var slot in _heroSlots)
            {
                HeroDataSO randomHero = _heroDataSOs[Random.Range(0, _heroDataSOs.Count)];
                AssignHeroToSlot(slot, randomHero);
            }
        }
        #endregion

    }
}
