using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Use a empty main screen panel, then add each panel later:
/// 1) Bench Panel
/// 2) ...
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class BenchPanelController : MonoBehaviour
{
    // ================= SerializeField ======================
    [SerializeField] private VisualTreeAsset _benchPanelAsset;

    // ================= VisualElement ======================
    private VisualElement _benchPanel;
    private VisualElement _mainPanel;
    private VisualElement _ghost;
    // ================== etc =======================
    private bool _isDragging = false;

    // ================= setter & getter ===================
    // ...

    private void OnEnable()
    {
        // get main screen panel
        UIDocument _main = GetComponent<UIDocument>();
        _mainPanel = _main.rootVisualElement;
        if (_mainPanel == null || _benchPanelAsset == null)
            return;

        // find bench panel's name
        VisualElement benchPanel = _benchPanelAsset.CloneTree()[0];
        benchPanel.RemoveFromHierarchy();

        // use bench panel's name - to find where the "bench panel" should be put inside the "main panel" 
        VisualElement benchPanelWhereAboutInMainPanel = _mainPanel.Q<VisualElement>(benchPanel.name);
        if (benchPanelWhereAboutInMainPanel == null)
        {
            Debug.LogWarning($"BenchPanelController: no element named '{benchPanel.name}' found in the main document.");
            return;
        }

        // put the bench panel in "it"
        benchPanelWhereAboutInMainPanel.Clear();
        benchPanelWhereAboutInMainPanel.Add(benchPanel);

        // get the reference for later use
        _benchPanel = benchPanel.Q<VisualElement>("BenchPanel");

        InitializeGhost();

        // Make hero slot draggable
        MakeHeroDraggable();
    }

    // temporarily ghost sprite
    private void InitializeGhost()
    {
        _ghost = new VisualElement();
        _ghost.style.position = Position.Absolute;
        _ghost.style.width = 40;
        _ghost.style.height = 40;
        _ghost.style.backgroundColor = new Color(1f, 1f, 1f, 0.6f);
    }

    /// <summary>
    /// A lot of comment since I never use those event before
    /// </summary>
    private void MakeHeroDraggable()
    {
        List<VisualElement> heroSlots = _benchPanel.Query<VisualElement>("HeroSlot").ToList();

        // register event to every hero slot.
        foreach (var heroSlot in heroSlots)
        {
            // when your mouse hold inside heroslot bound
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

            // when you move your mouse inside heroslot bound
            // if your mouse exit heroslot bound, the event won't fired, BUT we use CapturePointer so we can actually move out of bound
            // pointer = the point you holding your mouse, so this pointer can move
            heroSlot.RegisterCallback<PointerMoveEvent>(pointer =>
            {
                if (!_isDragging) return;

                // move ghost using pointer
                MoveGhostTo(pointer.position);
            });

            // when your mouse release from holding
            // pointer = the point you release your mouse
            heroSlot.RegisterCallback<PointerUpEvent>(pointer =>
            {
                _isDragging = false;

                // undo the CapturePointer
                heroSlot.ReleasePointer(pointer.pointerId);

                // release ghost from main UI, this make ghost go invisible
                _ghost?.RemoveFromHierarchy();

                // placing hero on the release point
                PlaceHero();
            });
        }
    }

    // ghost copying the mouse position using "screen panel method"
    private void MoveGhostTo(Vector2 panelPosition)
    {
        _ghost.style.left = panelPosition.x - _ghost.resolvedStyle.width / 2f;
        _ghost.style.top = panelPosition.y - _ghost.resolvedStyle.height / 2f;
    }

    private void PlaceHero()
    {
        // This project has Active Input Handling set to the new Input System package
        // (see Packages/manifest.json: com.unity.inputsystem) - the legacy Input class is
        // disabled project-wide, so Input.mousePosition throws. Mouse.current is the
        // equivalent read in the new system.
        Vector3 screenPosition = Mouse.current.position.ReadValue();

        // screenPosition is in real screen pixels - Physics2D.OverlapPoint needs a WORLD
        // position, so convert through the camera first (see Step 3 of
        // .claude/docs/drag-hero-to-board.cs for why the .z line below is needed).
        screenPosition.z = -Camera.main.transform.position.z;
        Vector3 placementPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        bool isThePlacementHitSomething = Physics2D.OverlapPoint(placementPosition);

        if (isThePlacementHitSomething) Debug.Log("aaaa");
    }
}
