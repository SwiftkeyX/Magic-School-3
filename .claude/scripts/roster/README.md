# Roster v3 pipeline

Scripts that build the MagicSchool Roster sheet's v3 hero roster and its SpiderWeb.
Moved here from a session scratchpad on 2026-09-21 so the work survives the session.

**Run them from the repo root**, not from this folder — they open
`google-service-credential.json` by relative path:

```bash
cd "<repo root>"
PYTHONIOENCODING=utf-8 python .claude/scripts/roster/stub_simple.py
```

(The console codepage is cp874, so `PYTHONIOENCODING=utf-8` is required or any
non-ASCII character in the sheet kills the run.)

## The chain

Each script imports the one above it, so running a later one runs all of them.
Every stage re-reads the `Origin` and `Class` tabs **live**, so a change made in the
sheet flows through without editing code.

| script | what it does |
| --- | --- |
| `roster25.py` | Solves the **cost 3-5** band: 22 units, classes assigned by backtracking search under A2, C4, C8, C10 and the one-class-per-cost preference. Slot list is hand-transcribed from the Origin tab's `Cost Archetype` column. |
| `cost12.py` | Solves the **cost 1-2** half: 19 units, origin/class/cost/role all searched. Enforces A2, B2, B5, B6, C1, C2, C3, C5, C8, C9, C10. |
| `render_full.py` | Draws `Grid - Cost Archetype` and `Grid - Origin x Class` for the 41 designed heroes. Also applies the Juggernaut relabel. |
| `stub_simple.py` | Adds the **19 stub heroes** to reach 60. Deliberately loose - see below. |
| `web.py` | The **free search**: best-scoring board per origin per tier, ignoring what a comp should look like. Writes `web.json`. ~7 minutes; run it in the background. Kept as an AUDIT pass, not the product - see below. |
| `web_tab.py` | Renders `web.json`. **Does not write the `SpiderWeb` tab any more** - that tab now holds the anchored web, so point `TAB` somewhere else before running it. |
| `anchored.py` | The **anchored web**, and the actual product: declared comps, solver + core resolution. Imported by the two below. |
| `anchored_run.py` | Solves every declared anchor. Writes `anchored.json`. ~4 minutes. |
| `anchored_tab.py` | Renders `anchored.json` to the `SpiderWeb` tab. |

`render_v2.py` and `layout.py` were earlier renderers and were not kept.

## Things that will bite you

- **`stub_simple.py` is intentionally loose.** It enforces only the origin quota, the
  class quota, A2/A3 and role legality under C1/C2/C8. It does **not** enforce C4,
  C10, B5, B6 or the one-class-per-cost preference. Stubs are disposable test
  material; the 41 designed heroes are fully rule-checked, the 19 stubs are not.
  Every stub carries `stub=True` and is marked `STUB` in its text.
- **A strict version of the stub generator exists and does not work.** `stubs.py`,
  `stubs57.py`, `stubs60.py` (not kept) tried to place stubs under every rule and
  failed at roster sizes 56, 57 and 60. Three separate bugs were found in that search
  along the way, so its failures are **not** evidence that the rules are
  over-constrained. If you rebuild it, rebuild it rather than trusting the old one.
- **`web.py` is not exhaustive.** C(60,9) is 14.6 billion boards. It enumerates the
  focus origin's required members exactly, then beam-searches (width 400) and
  hill-climbs. Results are the best *found*. The tab says so; keep it that way.
- The hand-transcribed `SLOTS` list in `roster25.py` must be kept in step with the
  Origin tab's `Cost Archetype` column by hand. It has drifted twice. When the column
  changes, edit `SLOTS` too or the grid silently renders the old design.
- A write into a merged cell's non-anchor cells is **silently dropped** by the Sheets
  API. The renderers unmerge first and then read the tab back and assert every unit
  cell landed. Keep both halves.

## The score, and why the free search is not the product

The curve is **C(b,2)** - a trait active with `b` members pays one point per PAIR of
them, so `b = 2..7` pays `1/3/6/10/15/21`. It is priced by the breakpoint's SIZE, not by
its tier index, because a tier index is not what a board spends: tier 3 costs 5 units in
Freeblade and 7 in Bloom and one number cannot pay for both. The old `1/3/5/7` paid a
flat +2 per tier against a board that trades at 2-3 points per unit, so going deeper was
a loss almost everywhere. Do not "fix" that by steepening a tier-indexed curve - it was
tested up to `1/6/20/50` and the 7-wide tiers still lose.

**`web.py` maximises that score and its top board has no carry on it.** `Warband 7 +
Resolute 4` scores 27 and every active class on it is a tank. The score counts members;
it does not know a carry from a tank, and no curve will teach it. That is why
`anchored.py` exists: the anchor and the Cost Archetype core are CONSTRAINTS, and the
score only ranks the spare slots. Keep `web.py` around as an audit - it is what caught
the all-tank board - but do not read its output as a list of comps.

## Things that will bite you in `anchored.py`

- **`ARCHETYPE` is hand-transcribed** from the Origin tab's `Cost Archetype` column, the
  same way `roster25.SLOTS` is. A "standard" origin contributes one carry and one tank,
  a "duo carry" two carries. If that column changes, edit `ARCHETYPE` or the core is
  silently solved against the old design. **Arcanum's cell is blank**, so it is the one
  anchor with no core.
- **A duo-carry origin names two carries at ONE cost** (Wildheart c3, Grave c4), so
  `core_of` consumes slots without replacement rather than requiring a unique hit. An
  earlier version asserted uniqueness and died on Wildheart; the assert is still there
  for the "no such unit" case and should stay.
- **Rule 4 is origin anchors only** and is measured on the breakpoint REACHED, not the
  raw member count - an anchor at 4 beside five Resolutes is still Resolute 4, a tie,
  and legal. Class anchors run uncapped on purpose, so they keep answering "which origin
  does this class want".
- **A random seed almost never satisfies the cap**, so `repair()` walks a board back into
  legality before the climb. Without it most restarts are thrown away and the short
  anchors come back empty.
