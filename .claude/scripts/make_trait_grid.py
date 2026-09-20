#!/usr/bin/env python3
"""Build a Class x Origin trait grid on one of the registered TFT reference sheets,
in the same shape and palette as the MagicSchool Roster sheet's own Grid tab.

    python .claude/scripts/make_trait_grid.py --sheet tft-set10
    python .claude/scripts/make_trait_grid.py --sheet tft-set9 --subset 9.0

The source is the sheet's `Champions` tab (`Champion Name / Cost / Origin 1-2 /
Class 1-2`). The target `Grid` tab is wiped and rebuilt, so the script is safe to
re-run. `--subset` keeps only rows whose `Sub-Set` cell mentions the given string,
which is how Set 9 is narrowed to 9.0 and its 9.5 additions dropped.

Unlike the roster, a TFT champion may carry two origins or two classes, so it
occupies several cells and a cell may hold more than one champion. Those
collisions are what the amber cells mark: they are the roster's rule A2 being
broken, and the reason the seat totals do not add up to the champion count.
"""
import argparse
import collections
import json
import math
import pathlib
import sys

import gspread

CREDENTIALS_FILE = "google-service-credential.json"
SHEETS_CONFIG_FILE = pathlib.Path(__file__).parent / "sheets_config.json"

# --- palette, lifted verbatim from the roster sheet's own Grid tab -----------
def rgb(r, g, b):
    return {"red": r, "green": g, "blue": b}


DARK      = rgb(0.21568628, 0.27450982, 0.30980393)   # #37464F header bar
WHITE     = rgb(1, 1, 1)
FILL      = rgb(0.8862745, 0.94509804, 0.9882353)     # #E2F1FC occupied cell
EMPTY     = rgb(0.9647059, 0.9647059, 0.9647059)      # #F6F6F6 empty cell
AMBER_BG  = rgb(1, 0.9529412, 0.8745098)              # #FFF3DF collision
AMBER_FG  = rgb(0.54901963, 0.41960785, 0.09803922)
GOLD_BG   = rgb(1, 0.98431373, 0.8784314)             # #FFFBE0 legend / footer
GOLD_FG   = rgb(0.44705883, 0.32941177, 0.02745098)
GOLD_EDGE = rgb(0.84705883, 0.7176471, 0.34901962)
TRAIT_FG  = rgb(0.047058824, 0.27450982, 0.627451)    # side-table Trait column
FLAG_FG   = rgb(0.7176471, 0.24705882, 0.047058824)   # side-table Flags column

NO_CLASS = "— no class —"
SIDE_WIDTHS = (110, 55, 140, 430, 150)


def load_champions(ws, subset):
    """Returns (kept champions, how many the subset filter excluded)."""
    rows = ws.get_all_records()
    champs, excluded = [], 0
    for r in rows:
        name = str(r.get("Champion Name", "")).strip()
        if not name:
            continue
        if subset and subset not in str(r.get("Sub-Set", "")):
            excluded += 1
            continue
        champs.append({
            "name": name,
            "cost": int(str(r["Cost"]).split()[0]),
            "origins": [str(r[k]).strip() for k in ("Origin 1", "Origin 2") if str(r[k]).strip()],
            "classes": [str(r[k]).strip() for k in ("Class 1", "Class 2") if str(r[k]).strip()],
        })
    return champs, excluded


def build_rows(champs, label, subset=None, excluded=0):
    origin_seats, class_seats = collections.Counter(), collections.Counter()
    for c in champs:
        for o in c["origins"]:
            origin_seats[o] += 1
        for k in (c["classes"] or [NO_CLASS]):
            class_seats[k] += 1

    # seats descending, alphabetical tie-break; the no-class row always sits last
    origins = sorted(origin_seats, key=lambda t: (-origin_seats[t], t))
    classes = sorted((k for k in class_seats if k != NO_CLASS),
                     key=lambda t: (-class_seats[t], t))
    if NO_CLASS in class_seats:
        classes.append(NO_CLASS)

    cell = collections.defaultdict(list)
    for c in champs:
        for k in (c["classes"] or [NO_CLASS]):
            for o in c["origins"]:
                cell[(k, o)].append(c)

    # A set can ship two different champions under one name - Set 10 has a K/DA
    # Akali and a True Damage Akali, both 4-cost Executioners. The matrix cells
    # are unambiguous because the column says which origin, but a flat member
    # list is not, so there the origin is spelled out.
    dupes = {n for n, k in collections.Counter(c["name"] for c in champs).items() if k > 1}

    def member_name(c):
        if c["name"] not in dupes:
            return c["name"]
        return f"{c['name']} [{'/'.join(c['origins'])}]"

    def members(cs):
        return sorted(cs, key=lambda c: (c["cost"], c["name"]))

    def cell_text(k, o):
        return "\n".join(f"{c['name']} ({c['cost']})" for c in members(cell.get((k, o), [])))

    grid_w = 1 + len(origins) + 1          # class name + origins + Seats
    side = grid_w + 1                      # one blank divider column

    sheet = [["Class \\ Origin"] + origins + ["Seats", ""]]
    for k in classes:
        sheet.append([k] + [cell_text(k, o) for o in origins] + [class_seats[k], ""])
    sheet.append(["Seats"] + [origin_seats[o] for o in origins] + ["", ""])
    sheet.append([""] * side)

    def flags(ms):
        f, costs = [], [c["cost"] for c in ms]
        if not any(c <= 2 for c in costs):
            f.append("no cost-1/2 opener (breaks roster rule B2)")
        if costs.count(5) > 1:
            f.append(f"{costs.count(5)} cost-5s (breaks roster rule B4)")
        dupes = [f"{n}x cost {c}"
                 for c, n in sorted(collections.Counter(costs).items()) if n > 2]
        if dupes:
            f.append("; ".join(dupes) + " (breaks roster rule B5)")
        if len(ms) == 1:
            f.append("unique trait (1 unit)")
        return ". ".join(f)

    def block(title, order, seats, of):
        out = [[title, "", "", "", ""],
               ["Trait", "Seats", "Cost spread", "Members (name cost)", "Flags"]]
        for t in order:
            ms = members(of(t))
            out.append([t, seats[t],
                        " / ".join(str(c["cost"]) for c in ms),
                        ", ".join(f"{member_name(c)} {c['cost']}" for c in ms),
                        flags(ms)])
        return out

    side_rows = block("CLASS - cost spread", classes, class_seats,
                      lambda t: [c for c in champs if t in (c["classes"] or [NO_CLASS])])
    side_rows.append([""] * 5)
    side_rows += block("ORIGIN - cost spread", origins, origin_seats,
                       lambda t: [c for c in champs if t in c["origins"]])

    curve = collections.Counter(c["cost"] for c in champs)
    class_total = sum(v for k, v in class_seats.items() if k != NO_CLASS)
    classless = [c["name"] for c in champs if not c["classes"]]
    multi = [c for c in champs if len(c["origins"]) > 1 or len(c["classes"]) > 1]
    side_rows += [
        [""] * 5,
        ["Cost curve", "", " / ".join(str(curve[i]) for i in range(1, 6)),
         f"costs 1-5 across all {len(champs)} champions", ""],
        ["Source", "", label,
         f"built from this sheet's Champions tab; {len(origins)} origins x "
         f"{len(classes)} class rows"
         + (f". The {excluded} champions outside {subset} are NOT in this grid, and "
            "neither are the traits whose every member was one of them." if excluded else ""),
         ""],
        ["Seat totals", "",
         f"{class_total} class / {sum(origin_seats.values())} origin",
         f"NOT equal to the {len(champs)} champions, and not equal to each other. TFT lets "
         f"a champion carry two origins or two classes ({len(multi)} do here), so it "
         "occupies several cells. The MagicSchool roster forbids this under rule A2, which "
         "is why its own grid totals exactly 50 on both axes."
         + (f" {len(classless)} champion(s) carry no class at all ({', '.join(classless)}) "
            f"and sit in the '{NO_CLASS}' row." if classless else ""), ""],
        ["Rules check", "", "vs the roster's Rules tab",
         "A2 one-hero-per-pair: broken wherever a cell is amber. B1 flat cost curve: this "
         "set runs " + " / ".join(str(curve[i]) for i in range(1, 6))
         + ". B2/B4/B5 violations are flagged per trait above.", ""],
    ]

    out = []
    for i in range(max(len(sheet), len(side_rows) + 1)):
        left = list(sheet[i]) if i < len(sheet) else [""] * side
        left += [""] * (side - len(left))
        right = side_rows[i - 1] if 1 <= i <= len(side_rows) else [""] * 5
        out.append(left + list(right))
    return out


def style_requests(rows, sid, subset=None):
    n_rows, n_cols = len(rows), len(rows[0])
    seats_col = n_cols - 7
    gap_col = n_cols - 6
    side_col = n_cols - 5
    side_end = side_col + 5

    seats_row = next(i for i, r in enumerate(rows) if r[0] == "Seats")
    class_title = next(i for i, r in enumerate(rows) if r[side_col] == "CLASS - cost spread")
    orig_title = next(i for i, r in enumerate(rows) if r[side_col] == "ORIGIN - cost spread")
    footer_row = next(i for i, r in enumerate(rows) if r[side_col] == "Cost curve")
    legend_row = seats_row + 1

    subset_note = f"{subset} only — champions outside it are not in this grid.       " if subset else ""

    req = []

    def rng(r0, r1, c0, c1):
        return {"sheetId": sid, "startRowIndex": r0, "endRowIndex": r1,
                "startColumnIndex": c0, "endColumnIndex": c1}

    def fmt(r0, r1, c0, c1, **f):
        req.append({"repeatCell": {
            "range": rng(r0, r1, c0, c1), "cell": {"userEnteredFormat": f},
            "fields": "userEnteredFormat(" + ",".join(sorted(f)) + ")"}})

    def text(size=10, bold=False, italic=False, fg=None):
        t = {"fontSize": size, "bold": bold, "italic": italic}
        if fg:
            t["foregroundColor"] = fg
        return t

    head = dict(backgroundColor=DARK, horizontalAlignment="CENTER",
                verticalAlignment="MIDDLE", wrapStrategy="WRAP",
                textFormat=text(bold=True, fg=WHITE))

    req.append({"updateSheetProperties": {
        "properties": {"sheetId": sid,
                       "gridProperties": {"frozenRowCount": 1, "frozenColumnCount": 1}},
        "fields": "gridProperties.frozenRowCount,gridProperties.frozenColumnCount"}})

    def width(c0, c1, px):
        req.append({"updateDimensionProperties": {
            "range": {"sheetId": sid, "dimension": "COLUMNS",
                      "startIndex": c0, "endIndex": c1},
            "properties": {"pixelSize": px}, "fields": "pixelSize"}})

    width(0, seats_col, 105)
    width(seats_col, seats_col + 1, 60)
    width(gap_col, gap_col + 1, 24)
    for i, px in enumerate(SIDE_WIDTHS):
        width(side_col + i, side_col + 1 + i, px)

    fmt(0, 1, 0, seats_col + 1, **head)
    fmt(0, seats_row + 1, 0, 1, **head)
    fmt(0, seats_row + 1, seats_col, gap_col + 1, **head)
    fmt(seats_row, seats_row + 1, 0, gap_col + 1, **head)
    fmt(1, seats_row, 1, seats_col,
        backgroundColor=EMPTY, textFormat=text(), wrapStrategy="WRAP")

    multi = 0
    for r in range(1, seats_row):
        for c in range(1, seats_col):
            v = rows[r][c]
            if not v:
                continue
            if "\n" in v:                      # more than one champion in the cell
                multi += 1
                fmt(r, r + 1, c, c + 1, backgroundColor=AMBER_BG,
                    horizontalAlignment="CENTER", wrapStrategy="WRAP",
                    textFormat=text(italic=True, fg=AMBER_FG))
            else:
                fmt(r, r + 1, c, c + 1, backgroundColor=FILL,
                    horizontalAlignment="CENTER", wrapStrategy="WRAP",
                    textFormat=text())

    legend = (subset_note + "Legend:   blue = one champion holds this (class, origin) pair   |   "
              "amber italic = MORE THAN ONE champion holds it, which the MagicSchool "
              "roster forbids under rule A2   |   grey = pair unused")
    # the merge starts at column B: column A is frozen, and Sheets refuses to
    # merge a frozen column with a non-frozen one.
    req.append({"updateCells": {
        "range": rng(legend_row, legend_row + 1, 1, 2),
        "rows": [{"values": [{"userEnteredValue": {"stringValue": legend}}]}],
        "fields": "userEnteredValue"}})
    req.append({"mergeCells": {"range": rng(legend_row, legend_row + 1, 1, gap_col + 1),
                               "mergeType": "MERGE_ALL"}})
    fmt(legend_row, legend_row + 1, 0, gap_col + 1,
        backgroundColor=GOLD_BG, horizontalAlignment="CENTER",
        verticalAlignment="MIDDLE", wrapStrategy="WRAP", textFormat=text(fg=GOLD_FG))
    req.append({"updateBorders": dict(
        range=rng(legend_row, legend_row + 1, 0, gap_col + 1),
        **{s: {"style": "SOLID", "width": 1, "color": GOLD_EDGE}
           for s in ("top", "bottom", "left", "right")})})

    # the white body wipe spans both side tables, so it runs BEFORE the dark
    # header bars - the other way round it paints over the ORIGIN table's headers
    fmt(class_title + 2, footer_row, side_col, side_end,
        backgroundColor=WHITE, wrapStrategy="WRAP", textFormat=text())
    for title in (class_title, orig_title):
        fmt(title, title + 1, side_col, side_end, backgroundColor=DARK,
            wrapStrategy="WRAP", textFormat=text(size=11, bold=True, fg=WHITE))
        fmt(title + 1, title + 2, side_col, side_end, backgroundColor=DARK,
            wrapStrategy="WRAP", textFormat=text(bold=True, fg=WHITE))
    for lo, hi in ((class_title + 2, orig_title - 1), (orig_title + 2, footer_row - 1)):
        fmt(lo, hi, side_col, side_col + 1, textFormat=text(bold=True, fg=TRAIT_FG))
        fmt(lo, hi, side_col + 1, side_col + 2, horizontalAlignment="CENTER")
        fmt(lo, hi, side_end - 1, side_end, textFormat=text(size=9, bold=True, fg=FLAG_FG))

    fmt(footer_row, n_rows, side_col, side_end,
        backgroundColor=GOLD_BG, wrapStrategy="WRAP", textFormat=text(fg=GOLD_FG))
    fmt(footer_row, n_rows, side_col, side_col + 1, textFormat=text(bold=True, fg=GOLD_FG))

    # autoResizeDimensions leaves rows at 21px however many lines a cell holds,
    # so heights are set explicitly from the tallest cell in the row.
    for r in range(1, seats_row):
        lines = max(str(v).count("\n") + 1 for v in rows[r][1:seats_col])
        if lines > 1:
            req.append({"updateDimensionProperties": {
                "range": {"sheetId": sid, "dimension": "ROWS",
                          "startIndex": r, "endIndex": r + 1},
                "properties": {"pixelSize": 19 * lines + 6}, "fields": "pixelSize"}})
    for r, px in ((0, 40), (legend_row, 46)):
        req.append({"updateDimensionProperties": {
            "range": {"sheetId": sid, "dimension": "ROWS", "startIndex": r, "endIndex": r + 1},
            "properties": {"pixelSize": px}, "fields": "pixelSize"}})

    return req, multi


def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--sheet", required=True, help="key in sheets_config.json, e.g. tft-set10")
    ap.add_argument("--champions-tab", default="Champions")
    ap.add_argument("--tab", default="Grid", help="target tab, created if absent")
    ap.add_argument("--subset", default=None,
                    help="keep only champions whose Sub-Set cell contains this, e.g. 9.0")
    args = ap.parse_args()

    config = json.loads(SHEETS_CONFIG_FILE.read_text(encoding="utf-8"))
    if args.sheet not in config:
        sys.exit(f"Unknown sheet '{args.sheet}'. Available: {list(config)}")

    client = gspread.service_account(filename=CREDENTIALS_FILE)
    sh = client.open_by_key(config[args.sheet]["key"])

    champs, excluded = load_champions(sh.worksheet(args.champions_tab), args.subset)
    if not champs:
        sys.exit("No champions matched - check --champions-tab and --subset.")

    label = args.sheet + (f" ({args.subset} only)" if args.subset else "")
    rows = build_rows(champs, label, args.subset, excluded)

    try:
        ws = sh.worksheet(args.tab)
        full = {"sheetId": ws.id, "startRowIndex": 0, "endRowIndex": ws.row_count,
                "startColumnIndex": 0, "endColumnIndex": ws.col_count}
        # wipe values, formats, merges and widths: a rebuild may be smaller than
        # what is already there, and leftovers would show through underneath.
        sh.batch_update({"requests": [
            {"unmergeCells": {"range": full}},
            {"updateCells": {"range": full, "fields": "*"}},
            {"updateDimensionProperties": {
                "range": {"sheetId": ws.id, "dimension": "COLUMNS",
                          "startIndex": 0, "endIndex": ws.col_count},
                "properties": {"pixelSize": 100}, "fields": "pixelSize"}},
            {"updateDimensionProperties": {
                "range": {"sheetId": ws.id, "dimension": "ROWS",
                          "startIndex": 0, "endIndex": ws.row_count},
                "properties": {"pixelSize": 21}, "fields": "pixelSize"}},
        ]})
    except gspread.WorksheetNotFound:
        ws = sh.add_worksheet(title=args.tab, rows=max(len(rows) + 10, 50),
                              cols=len(rows[0]))

    if ws.col_count < len(rows[0]) or ws.row_count < len(rows):
        ws.resize(rows=max(ws.row_count, len(rows) + 10),
                  cols=max(ws.col_count, len(rows[0])))

    ws.update(values=rows, range_name="A1")
    req, multi = style_requests(rows, ws.id, args.subset)
    sh.batch_update({"requests": req})

    print(f"{args.tab} on {args.sheet}: {len(champs)} champions, "
          f"{len(rows)} rows x {len(rows[0])} cols, {multi} collision cell(s)")
    print(f"https://docs.google.com/spreadsheets/d/{config[args.sheet]['key']}"
          f"/edit?gid={ws.id}#gid={ws.id}")


if __name__ == "__main__":
    main()
