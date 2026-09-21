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
| `web.py` | Computes the **SpiderWeb**. Writes `web.json`. Takes ~4 minutes; run it in the background. |
| `web_tab.py` | Renders `web.json` to the `SpiderWeb` tab. |

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
