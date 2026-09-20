---
name: roster-grid
description: Refresh the MagicSchool Roster sheet's Grid tab after any change to its heroes or traits - adding, removing, repricing or re-trait-ing a hero, or changing a trait's seat target. Recomputes the matrix, both Seats lines, both cost-spread tables and the footer, repaints the cells, and checks the roster against its own Rules tab. Use whenever the Heroes tab changes.
---

# Roster Grid refresh

The `Grid` tab is **derived** from the `Heroes` tab, but nothing in the sheet derives it —
there are no formulas. Change a hero and the Grid silently goes stale.

```bash
python .claude/scripts/refresh_roster_grid.py
```

Run it after **any** edit to `Heroes`: a hero added, removed, repriced, or moved between a
class or origin. It is idempotent, so running it when nothing changed is harmless.

## What a cell says

A matrix cell reads `Name (cost) (hidden role)` — `Vane (5) (Fighter)`. A hidden role that is
still undecided keeps its options and a question mark, `Pike (3) (Tank/Fighter?)`, so the grid
never implies a decision nobody made. The role comes from the `Hidden role` column on `Heroes`,
which is derived from the hero's class (see the `Class` tab's own `Hidden role` column) and then
narrowed by its Range and Dmg: Marksmen needs Ranged + AD, Mage and Support need AP, Fighter
needs Melee.

Cells wrap at this width, and **a wrapped line is not a newline** — so row heights are estimated
from the column width (~6px per character at 10pt), not counted from line breaks. Don't switch
that back to counting line breaks; the roles would clip.

## Three things that are easy to get wrong

**Values and colour are one job, not two.** The matrix fill is not decoration — it encodes
each hero's `Status`:

| fill | meaning |
| --- | --- |
| `#FFF3DF` amber italic | `Status = NEW - unnamed art` — named, but no sprite picked yet |
| `#E2F1FC` blue | hero exists with art reserved (`Built` / `Name reserved` / `New`) |
| `#F6F6F6` grey | no hero holds this (class, origin) pair |

Write values without repainting and a new hero sits on the grey fill of the empty cell it
replaced, which reads as *"no hero here"*. The script does both halves in one pass for exactly
this reason. If you ever edit the Grid by hand instead, repaint the cells you touched.

**Never re-sort the rows or columns.** Comment threads on this sheet anchor by cell position,
and several sit on the Grid's own headers (`Class \ Origin`, `Scale`, `Arcanum`, `Warband`,
`Grave`, `Wildheart`). Sorting moves a note onto the wrong trait. The script reads the existing
class-row and origin-column order off the tab and reuses it rather than choosing one — keep it
that way. A parse that runs past the `Seats` column will pick up `CLASS - cost spread` as an
extra origin, so stop at `Seats`.

## Check the rules after

The roster is solved against the `Rules` tab, and nothing enforces that either. After a hero
change, verify from the live sheet rather than by hand:

- **A1** class seats == origin seats == roster size (each hero is one of each)
- **A2** one hero per `(class, origin)` pair
- **B2** every trait holds a cost-1 or cost-2 hero
- **B4** at most one cost-5 per trait
- **B5/B6** at most one duplicated cost per trait — which caps **every trait at 6 members**
  (5 costs + 1 duplicate). A breakpoint above 6 can never fire.
- **C3** every origin holds at least one AD and at least one AP hero

B1 (the cost curve) is already bent and is a shape, not a hard rule.

Open **comments** on the sheet are live constraints too, and `sheet_sync.py` cannot read them —
fetch them from the Drive v3 comments endpoint (see the `sync-sheet` skill). They have blocked
real edits: "Scale shouldn't have tank at all" and "Wildheart should be pair with
Finisher/Gambit/Echo/Reviled/Adrenaline" together forced which six origins could take the
Resolute heroes.

Related: `sync-sheet` covers reading and writing these tabs and the house palette;
`make_trait_grid.py` builds the same-shaped grid on the TFT reference sheets.
