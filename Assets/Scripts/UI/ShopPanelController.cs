using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    [SerializeField] private List<HeroDataSO> _heroDataSOs;
    [SerializeField] private Bench _bench;

    // ================= VisualElement ======================
    private VisualElement _shopPanel;
    private VisualElement _mainPanel;
    private VisualElement _ghost;
    private Dictionary<VisualElement, HeroDataSO> _heroSlots = new Dictionary<VisualElement, HeroDataSO>();

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
    }

    private VisualElement PutThisPanelInMainPanel(VisualTreeAsset panelTree)
    {
        // find this panel's name
        VisualElement panel = panelTree.CloneTree()[0];
        panel.RemoveFromHierarchy();

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
        List<VisualElement> slots = _shopPanel.Query<VisualElement>("HeroSlot").ToList();

        // assign hero data to each slots
        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= _heroDataSOs.Count) { Debug.LogError("HeroDataSO is not enough for all slot in shop panel"); return; }
            _heroSlots[slots[i]] = _heroDataSOs[i];
        }

        // register event to every slot.
        foreach (var slot in slots)
        {
            // when click on heroslot, spawn ghost, move ghost to click point
            HeroSlotOnClick(slot);

            // when hold on heroslot, move ghost to hold point, create dragging logic visually
            HeroSlotOnMove(slot);

            // when your mouse release from holding, resolve buy/cancel based on the release point
            HeroSlotOnRelease(slot);
        }
    }

    // ============================== Pointer Event ====================================
    private void HeroSlotOnClick(VisualElement heroSlot)
    {
        // PointerDownEvent = when your mouse hold inside heroslot bound
        // pointer = the point you start clicking
        heroSlot.RegisterCallback<PointerDownEvent>(pointer =>
        {
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
    // temporarily ghost sprite
    private void InitializeGhost()
    {
        _ghost = new VisualElement();
        _ghost.style.position = Position.Absolute;
        _ghost.style.width = 40;
        _ghost.style.height = 40;
        _ghost.style.backgroundColor = new Color(1f, 1f, 1f, 0.6f);
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

        Debug.Log($"Bought hero from slot '{_heroSlots[slot]}'.");

        // TODO: spend gold / add hero to bench once those systems exist.
        BuyHero(_heroSlots[slot]);
    }
    #endregion

    #region etc
    private void BuyHero(HeroDataSO data)
    {
        _bench.SpawnHeroOnBench(data);
    }
    #endregion

}
