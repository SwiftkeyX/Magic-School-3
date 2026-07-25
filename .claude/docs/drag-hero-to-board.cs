// REFERENCE ONLY - lives outside Assets/, so Unity never compiles this file.
// Five illustrative snippets, one per step of "drag a hero from the Bench onto the board."
// None of this is wired into the real game - copy the ideas into your own scripts under
// Assets/Scripts, adjusting names/paths to match your actual Bench/BenchSlot/Hero setup.
//
// SUPERSEDES an earlier version of this doc that assumed the Bench was a UI Toolkit panel
// (BenchPanelController + HeroSlot VisualElements, PointerDownEvent/PointerUpEvent). That's
// no longer the plan - the Bench is now real world-space Hero GameObjects standing on
// Bench/BenchSlot prefabs (see Assets/Scripts/Bench/), because UI Toolkit can only show a
// flat icon, not an actual animated Hero. So this version drags the real Hero GameObject
// directly, the same way the board itself works, instead of a UI ghost element.
// The Shop panel (buying heroes) is still UI Toolkit, drag-to-buy - see
// Assets/Scripts/UI/ShopPanelController.cs. This doc is only about Bench -> Board.

using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// This project has Active Input Handling set to "Input System Package (New)" exclusively
// (confirmed 2026-07-25 via a runtime InvalidOperationException) - UnityEngine.Input throws
// at runtime the moment anything reads it, even though it still compiles fine. Always use
// Mouse.current from UnityEngine.InputSystem instead, everywhere below.

// =====================================================================================
// STEP 1 - Give Hero and Hex a Collider2D
// =====================================================================================
// Neither has one yet (checked 2026-07-25: zero Collider2D components anywhere in
// Board.unity). Physics2D.OverlapPoint needs a collider to hit. Mark both isTrigger =
// true so they never produce physics collision response - they're purely for point-picking.
// Hex is static, so it needs no Rigidbody2D (that requirement only applies to trigger
// *callbacks* like OnTriggerEnter2D, not overlap queries like OverlapPoint).
public class Step1_Colliders
{
    // Add via Inspector or add_component on both the Hero and Hex prefabs:
    //   Hero prefab -> CircleCollider2D or BoxCollider2D, isTrigger = true
    //   Hex prefab  -> matches its tile shape, isTrigger = true
}

// =====================================================================================
// STEP 2 - Pick up: mouse down on a benched Hero
// =====================================================================================
public class Step2_PickUp
{
    private Hero _draggingHero;
    private BenchSlot _sourceSlot;

    public void TryStartDrag(Camera cam, Bench bench)
    {
        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = -cam.transform.position.z;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        Hero hero = hit != null ? hit.GetComponent<Hero>() : null;
        if (hero == null) return;

        // TODO: Bench/BenchSlot need a way to answer "which slot (if any) is this hero
        // sitting in right now?" - e.g. BenchSlot could track an Occupant reference
        // (mirroring how Hex conceptually tracks Occupant on the board side), or Bench
        // itself could expose a lookup. Neither exists yet.
        BenchSlot slot = bench.FindSlotFor(hero); // <- illustrative, not real yet
        if (slot == null) return; // hero isn't on the bench (already placed, or mid-drag)

        _draggingHero = hero;
        _sourceSlot = slot;
    }
}

// =====================================================================================
// STEP 3 - Drag: follow the mouse every frame
// =====================================================================================
// No ghost element needed here, unlike the Shop's UI Toolkit drag - this IS the real
// Hero GameObject, so just move it directly.
public class Step3_FollowPointer
{
    public void UpdateDrag(Camera cam, Hero draggingHero)
    {
        if (draggingHero == null) return;

        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = -cam.transform.position.z;
        draggingHero.transform.position = cam.ScreenToWorldPoint(screenPos);
    }
}

// =====================================================================================
// STEP 4 - Drop resolution: mouse up
// =====================================================================================
public class Step4_ResolveDrop
{
    public void OnRelease(Camera cam, BattleBoard board, Hero draggingHero, BenchSlot sourceSlot)
    {
        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = -cam.transform.position.z;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        Hex targetHex = hit != null ? hit.GetComponent<Hex>() : null;

        // "Is this hex occupied?" isn't a direct O(1) lookup yet - Hex has no Occupant
        // field despite the architecture note in CLAUDE.md. Cheapest check available today:
        bool isOccupied = targetHex != null && board.HeroesOnBoard.Any(h => h.GetCurrentHex() == targetHex);

        if (targetHex == null || isOccupied)
        {
            // Cancel - snap back to the bench slot, same "drop back = cancel" idea as the Shop.
            draggingHero.transform.position = sourceSlot.transform.position;
            return;
        }

        // Valid drop - reuse the existing prep-phase placement method (sets CurrentHex/
        // ReservedHex and snaps transform.position, same as BattleBoard.SpawnHero does).
        draggingHero.MoveHeroInPreparation(targetHex);
        sourceSlot.SetReserved(false);

        // See Step 5 - BattleBoard has no public entry point for "register a hero that
        // already exists" yet, only SpawnHero which instantiates a brand new prefab.
    }
}

// =====================================================================================
// STEP 5 - Gap: BattleBoard can't register an already-existing Hero
// =====================================================================================
// BattleBoard.SpawnHero(Hex, Team, HeroDataSO) always does Instantiate(dataSO.Prefab, ...)
// internally - it has no path for "here's a Hero GameObject that already exists (it was
// sitting on the bench), just start tracking it." Dragging needs the latter: the hero
// was already instantiated once, by Bench.SpawnHeroOnBench when it was bought.
public class Step5_RegisterExistingHero
{
    // Illustrative addition to BattleBoard.cs:
    //
    // public void RegisterHeroOnBoard(Hero hero)
    // {
    //     hero.SetBoard(this);
    //     _heroesOnBoard.Add(hero);
    // }
    //
    // Call this from Step4 instead of SpawnHero once a drop is valid.
}
