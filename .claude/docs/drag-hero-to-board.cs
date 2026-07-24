// REFERENCE ONLY - lives outside Assets/, so Unity never compiles this file.
// Four illustrative snippets, one per step of "drag a hero from the Bench onto the board."
// None of this is wired into the real game - copy the ideas into your own scripts under
// Assets/Scripts, adjusting names/paths to match your actual BenchPanelController setup.

using UnityEngine;
using UnityEngine.UIElements;

// =====================================================================================
// STEP 1 - Detect the drag start on a HeroSlot (UI Toolkit side)
// =====================================================================================
// Attach this kind of logic wherever you currently build/reference each "HeroSlot"
// VisualElement (e.g. inside BenchPanelController, once slots are wired to real data).
public class Step1_DragStart
{
    public void RegisterSlot(VisualElement heroSlot)
    {
        heroSlot.RegisterCallback<PointerDownEvent>(evt =>
        {
            // CapturePointer means THIS element keeps getting PointerMove/PointerUp events
            // even after the cursor moves outside its bounds - essential for dragging.
            heroSlot.CapturePointer(evt.pointerId);

            Debug.Log($"Started dragging from slot: {heroSlot.name}");
        });
    }
}

// =====================================================================================
// STEP 2 - Track the drag: a floating element that follows the pointer
// =====================================================================================
// A minimal "ghost" that mirrors cursor position while the slot has pointer capture.
// In a real version you'd show a hero icon/sprite here instead of a plain box.
public class Step2_DragGhost
{
    private VisualElement _ghost;
    private VisualElement _root; // e.g. BenchPanelController's UIDocument.rootVisualElement

    public void RegisterSlot(VisualElement heroSlot, VisualElement rootVisualElement)
    {
        _root = rootVisualElement;

        heroSlot.RegisterCallback<PointerDownEvent>(evt =>
        {
            heroSlot.CapturePointer(evt.pointerId);

            _ghost = new VisualElement();
            _ghost.style.position = Position.Absolute;
            _ghost.style.width = 40;
            _ghost.style.height = 40;
            _ghost.style.backgroundColor = new Color(1f, 1f, 1f, 0.6f);
            _root.Add(_ghost);

            MoveGhostTo(evt.position);
        });

        heroSlot.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (_ghost == null) return; // not currently dragging
            MoveGhostTo(evt.position);
        });

        heroSlot.RegisterCallback<PointerUpEvent>(evt =>
        {
            heroSlot.ReleasePointer(evt.pointerId);
            _ghost?.RemoveFromHierarchy();
            _ghost = null;
        });
    }

    private void MoveGhostTo(Vector2 panelPosition)
    {
        // panelPosition is in PANEL space here (fine for positioning a UI Toolkit element -
        // see Step 3 for why this is NOT the same thing as screen space).
        _ghost.style.left = panelPosition.x - _ghost.resolvedStyle.width / 2f;
        _ghost.style.top = panelPosition.y - _ghost.resolvedStyle.height / 2f;
    }
}

// =====================================================================================
// STEP 3 - The bridge: figure out which Hex (if any) is under the drop point
// =====================================================================================
// This is the part that trips people up: UI Toolkit events give you PANEL-space
// coordinates, not necessarily the same as screen pixels (depends on PanelSettings'
// scale mode/reference resolution). Input.mousePosition is always real screen pixels,
// so using that for the world-space conversion sidesteps the whole question.
//
// Hex has a Collider2D now (decided 2026-07-24, see .claude memory:
// project_combat_hitbox_design) - purely for consistency with the projectile hitboxes
// Hero is getting, so board-tile picking goes through Physics2D like everything else
// instead of a manual nearest-distance search. Hex is static, so it needs no
// Rigidbody2D - Physics2D.OverlapPoint detects Rigidbody2D-less colliders just fine
// (that requirement only applies to trigger *callbacks* like OnTriggerEnter2D, not
// overlap queries like this one). Mark the collider isTrigger = true so it never
// produces physics collision response.
public class Step3_FindDropHex
{
    public Hex FindHexUnderPointer(Camera cam)
    {
        Vector3 screenPos = Input.mousePosition; // real screen pixels, NOT the UI event's panel position
        screenPos.z = -cam.transform.position.z; // distance from an orthographic camera to the board's z=0 plane
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null ? hit.GetComponent<Hex>() : null;
    }
}

// =====================================================================================
// STEP 4 - What a valid drop actually does
// =====================================================================================
// This is a DESIGN decision, not just a mechanical one - sketched here, not prescribed.
// Open questions you'll need to answer before this is real (nothing in the codebase
// currently tracks "which HeroDataSO is sitting in which bench slot" - Bench.uxml's
// HeroSlots are still just placeholder Labels):
//   - Which HeroDataSO does this particular slot represent?
//   - Is dropping only legal on hexes belonging to the player's own team?
//   - Is the target hex already occupied by another hero?
//   - Does a successful drop remove the hero from the bench (so it can't be placed twice)?
public class Step4_HandleDrop
{
    public void OnDroppedOnHex(BattleBoard board, GameObject heroPrefab, HeroDataSO heroData, Hex targetHex, Team team)
    {
        if (targetHex == null)
        {
            Debug.Log("Dropped outside the board - cancel, snap the ghost back to the bench slot.");
            return;
        }

        // TODO: reject if targetHex is already occupied, or belongs to the wrong team.

        board.SpawnHero(heroPrefab, targetHex, team, heroData);

        // TODO: mark the bench slot empty / remove the hero from the roster now that it's placed.
    }
}


