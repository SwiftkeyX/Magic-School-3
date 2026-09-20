---
name: sync-sheet
description: Read and write data to/from this project's Google Sheets (MagicSchool Roster, tft-skill, TFT Set 9, TFT Set 9 Analysis, TFT Set 10, TFT Set 11) using the background sheet_sync.py script.
---

# Google Sheet Sync Skill (Background API)

This skill allows the agent to read and write to the project's Google Sheets directly in the background using the [sheet_sync.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School%203/.claude/scripts/sheet_sync.py) command-line utility. This avoids launching a browser, dumping a giant unstructured text blob through the Drive MCP tool, or interrupting the user.

There are **6 registered sheets** (see [`sheets_config.json`](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School%203/.claude/scripts/sheets_config.json)):

| name | what it is |
| --- | --- |
| `roster` | **MagicSchool Roster** — this project's live hero/trait design grid |
| `tft-skill` | **default** — this project's own per-hero skill/step database |
| `tft-set9` | TFT Set 9 champions reference (real ability text + stat export) |
| `tft-set9-analysis` | TFT Set 9 carry / combat-efficiency analysis |
| `tft-set10`, `tft-set11` | TFT Set 10 / 11 champions reference |

All six are marked `writable: true`. Select a non-default sheet with `--sheet <name>` before the subcommand, e.g. `python .claude/scripts/sheet_sync.py --sheet roster read "Grid"`.

**The two project-owned sheets are split on purpose.** `roster` holds the roster *shape* — which hero costs what, which Class and Origin it sits in, which sprite it uses, and each trait's breakpoints. `tft-skill` holds what a hero *does* — the per-step skill data. When the user says "the sheet" about **traits or the roster** they mean `roster`; about **a hero's kit** they mean `tft-skill`.

## The MagicSchool Roster sheet (`roster`)

`https://docs.google.com/spreadsheets/d/1SufJzQ-7ccyCI_RbgRD5n8Siy2Ipd2epC1VA8TUBATI/edit`

The live design surface for the 50-hero roster and its 22 traits — see the
[[reference_magicschool_roster_sheet]] memory. Its tabs:

| tab | what it holds | local mirror |
| --- | --- | --- |
| `Grid` | the trait grid: **Class rows × Origin columns**, one hero per cell as `Name (cost)`, with a `Seats` total on each axis and `CLASS`/`ORIGIN` cost-spread tables off to the right | — |
| `Class` | one row per class trait: `Trait / Flavour Name / Units (target) / Breakpoints / Status / Note / Difficulty / Effect / Tier 1-3 / Engine Gap` | `.claude/docs/hero-classes.csv` |
| `Origin` | same shape, plus a `Tier 4` column | `.claude/docs/hero-origins.csv` |
| `Heroes` | the 50 heroes: `Name / Cost / Class / Origin / Range / Dmg / Status / Sprite / Identity / Changed from` | `.claude/docs/hero-grid.csv` |
| `Rules` | the numbered roster constraints (`A1`, `B1`, …) the grid is solved against, each with a `Status` and a `Note` recording where it was bent | — |
| `Hero Roster` | the pre-rename archive, **deliberately stale** — its Class/Origin names are the old ones (Fighter, Ironhold, …) and its member counts do not add up. Don't read trait membership out of it | `.claude/docs/hero-roster.csv` |

Two traps worth naming: `hero-grid.csv` mirrors the **`Heroes`** tab, not the `Grid` tab (the
grid itself has no CSV mirror, because it is a matrix rather than a row list); and the `Grid`
tab is derived from `Heroes` with no formulas behind it, so it goes stale silently whenever a
hero changes. **Use the `roster-grid` skill after any edit to `Heroes`** — it refreshes the
values *and* repaints the cells, which is one job, because the fill encodes each hero's art
status.

**The design conversation lives in this sheet's comments, which `sheet_sync.py` cannot read.**
Fetch them from the Drive v3 comments endpoint using the same service-account file — see
[[reference_magicschool_roster_sheet]] for the exact call. Unresolved comments are the open
asks. **Never re-sort rows in this sheet:** comment anchors are by cell position, so a sort
silently moves a note onto the wrong trait.

## Purpose of the Google Sheet (`tft-skill`, the default)

This is the design source of truth for this project's hero roster and skill data — see [[reference_tft_skill_sheet]] memory for the full picture. Its tabs (as of the last sync):

`Hero Roster`, `Archetype Review`, `Dashboard`, `Hero set 9`, `Hero set 10`, `Action Model`, `Column Explain`, `Effect Types`, `Collision Types`, `Effect Recipient Types`, `Volley Shape Types`, `Scaling Types`, `AOE shape`, `Trigger Types`, `Offset Types`, `Fire Timing Types`, `Aim Target Types`, `Action Source Types`, `Design Notes`, `Hero flat`, `Role Types`, `Damage Types`.

**`Hero set 9`** (and its Set 10 sibling) is the one that matters for authoring a hero's kit — it lays out per-hero skill data in the same shape this project's C# skill system mirrors: `Step / Skill Type / Trigger (When) / Condition (Only if) / Action Source / Legacy action / Aim Target / Offset / AOE (hex) / Skill Range / Count / Volley Shape / Fire Timing / Cast (s) / Effect Recipient / Effect Category / Effect Detail / Amount / Scaling Type / Scaling / Effect Cadence / Effect Duration (s)`. A champion's row is followed by blank-identity continuation rows for each additional step/effect — read several rows past the name match, not just the one row.

Cross-reference this tab first rather than guessing a kit from League/TFT knowledge alone. It's frequently *more precise* than what the current C# system can express yet (e.g. Teemo's row names a `Trigger (When)` of `On Projectile Hit` and an `Aim Target` of `Hex That Projectile Hit`, and a `Condition` column for star-level branching — none of which `TriggerEnum`/`AimTarget` support today, only an approximation via `OnExpired` + `AimTarget.Current`). When the sheet is more precise than the code, say so explicitly rather than silently rounding down to what's easy.

There are sibling read-only reference sheets too (`tft-set9`, `tft-set9-analysis`) holding the raw real-game ability text and stat exports these designs are adapted from — useful for filling gaps `tft-skill` doesn't spell out (e.g. its `Hero Roster`/`Hero set 9` tabs sometimes only have a summary row with no authored per-step breakdown yet).

## Credentials

The credentials for this connection are stored in the root credential file [google-service-credential.json](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School%203/google-service-credential.json) (gitignored — never commit it).

## Steps to Read the Sheet

To fetch data from any tab in the spreadsheet, run `.claude/scripts/sheet_sync.py` from the workspace root.

* **Format as Table (Human-Readable):**
  ```bash
  python .claude/scripts/sheet_sync.py read "Hero set 9"
  ```
* **Format as JSON (For parsing):**
  ```bash
  python .claude/scripts/sheet_sync.py read "Hero set 9" --format json
  ```
* **Format as CSV:**
  ```bash
  python .claude/scripts/sheet_sync.py read "Hero set 9" --format csv
  ```
* **A tab on another sheet** — `--sheet` goes *before* the subcommand:
  ```bash
  python .claude/scripts/sheet_sync.py --sheet roster read "Grid" --format csv
  python .claude/scripts/sheet_sync.py --sheet roster read "Class" --format csv
  ```

**On this machine, prefix reads with `PYTHONIOENCODING=utf-8`.** The console codepage is
cp874 (Thai), so printing a tab containing a bullet, an em dash or a box character dies with
`UnicodeEncodeError: 'charmap' codec can't encode character ...` — which looks like a script
bug but is only the terminal. Several tabs on `tft-set9` and `roster` contain such characters:

```bash
PYTHONIOENCODING=utf-8 python .claude/scripts/sheet_sync.py --sheet tft-set9 read "Classes" --format csv
```

## Steps to Write to the Sheet

To update a cell in the spreadsheet, run the script's `write` command:

```bash
python .claude/scripts/sheet_sync.py write "Hero set 9" B2 "Teemo"
```

## Steps to Bulk-Write Rows

To write many rows in a single API call (e.g. rebuilding a whole data block), use the `write-range` command. It takes a worksheet name, a start cell, and a `--file` pointing to a local JSON file containing a list of rows (each row itself a list of cell values — use JSON rather than CSV so fields containing commas don't need escaping):

```bash
python .claude/scripts/sheet_sync.py --sheet tft-set9 write-range "Meta Comps" A1 --file rows.json
```

If the named worksheet doesn't exist yet, it is created automatically (sized to fit the data) before writing.

## Building a trait grid on a reference sheet

`.claude/scripts/make_trait_grid.py` rebuilds a `Grid` tab on any registered TFT reference
sheet, in the roster `Grid` tab's shape and palette — Class rows × Origin columns, `Seats` on
both axes, `CLASS`/`ORIGIN` cost-spread tables to the right, and a footer that checks the set
against the roster's own `Rules` tab:

```bash
python .claude/scripts/make_trait_grid.py --sheet tft-set10
python .claude/scripts/make_trait_grid.py --sheet tft-set9 --subset 9.0
```

It reads the sheet's `Champions` tab and wipes the target tab before rebuilding, so it is safe
to re-run. `--subset` keeps only champions whose `Sub-Set` cell mentions the string, which is
how Set 9 is pinned to 9.0 — dropping the 9.5 additions also drops any trait whose every member
was one of them (Bilgewater, Ixtal, Vanquisher, Reaver King).

Three differences from the roster grid, all of them real and all called out in the footer:

- **A champion can hold two origins or two classes**, so it occupies several cells, and a cell
  can hold more than one champion. Those collision cells are the amber ones — the roster
  forbids them under rule A2. It is also why the class and origin seat totals differ from each
  other and from the champion count, where the roster's are all exactly 50.
- **A champion can have no class at all** (Set 9's Bel'Veth and Heimerdinger). They go in a
  trailing `— no class —` row rather than being dropped.
- **Two champions can share a name.** Set 10 ships a K/DA Akali and a True Damage Akali, both
  4-cost Executioners. Matrix cells stay unambiguous because the column names the origin, but
  the flat `Members` lists spell it out as `Akali [K/DA]`.

## Styling a tab

`sheet_sync.py` writes values only — it has no formatting command. For colours, widths and
frozen panes, go straight to the Sheets API with the same credentials
(`gspread.service_account(...).open_by_key(key).batch_update({"requests": [...]})`).

**The house palette**, taken from the `roster` sheet's `Grid` tab. Reuse it so a new grid looks
like the existing ones rather than like Sheets' defaults:

| role | background | text |
| --- | --- | --- |
| header bar (top row, label column, totals row/column) | `#37464F` | white, bold, centred, wrapped, 10pt |
| occupied cell | `#E2F1FC` | default, centred, wrapped, 10pt |
| empty cell | `#F6F6F6` | default, 10pt |
| cell needing attention (no art yet on the roster; a rule violation elsewhere) | `#FFF3DF` | `#8C6B19`, *italic*, centred |
| note / legend / footer strip | `#FFFBE0` | `#725402`, with a `#D8B759` solid border |
| side-table `Trait` column | white | `#0C46A0`, bold |
| side-table `Flags` column | white | `#B73F0C`, bold, 9pt |

Column widths: 105px for matrix columns, 24px for the divider column between the matrix and the
side tables, then 110 / 55 / 140 / 430 / 150 for `Trait` / `Seats` / `Cost spread` / `Members` /
`Flags`. Freeze one row and one column.

Three API traps, each of which cost a round trip:

- **`mergeCells` refuses to span the frozen column boundary** — `You can't merge frozen and
  non-frozen columns`. Start a full-width banner at column B, not column A.
- **Order matters inside one `batch_update`.** Requests apply in sequence, so a broad
  `repeatCell` background wipe issued *after* a header bar silently paints over it. Lay down the
  wide fills first, then the narrow ones.
- **`autoResizeDimensions` on `ROWS` leaves rows at 21px** even when a cell holds several lines
  of wrapped text. Set `pixelSize` explicitly from the line count instead (`19 * lines + 6`
  reads about right).

A `batch_update` is atomic: a 400 on any one request means none of them applied, so a failed
call leaves the tab exactly as it was.

## Steps to Dump All Sheet Content

To download all tabs of a sheet into a single local, self-describing JSON file (default name `<sheet>_dump_<date>.json`, embeds a `_meta` block with sheet name/key/timestamp) — put these under `.claude/reference/json/` so they're grep-able without re-fetching:

```bash
python .claude/scripts/sheet_sync.py dump -o .claude/reference/json/tft-skill_dump_$(date +%Y%m%d).json
python .claude/scripts/sheet_sync.py --sheet roster dump -o .claude/reference/json/roster_dump_$(date +%Y%m%d).json
python .claude/scripts/sheet_sync.py --sheet tft-set9 dump -o .claude/reference/json/tft-set9_dump_$(date +%Y%m%d).json
```

A dump is much easier to search (`python -c "import json; ..."` or grep the file directly) than re-reading one huge tab through the API each time, and doesn't hit the same "result exceeds max tokens" wall the Drive MCP tool does on this sheet (250k+ chars).
