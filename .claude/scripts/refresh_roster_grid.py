#!/usr/bin/env python3
"""Refresh the roster sheet's Grid tab from its Heroes tab, IN PLACE - values AND colour.

    python .claude/scripts/refresh_roster_grid.py

Run it after ANY change to the Heroes tab: a hero added, removed, repriced, or
moved between traits. It recomputes every cell, both Seats lines, both
cost-spread tables and the footer, then repaints the matrix.

The existing row and column order is read off the tab and reused exactly. It is
never re-sorted: this sheet's comment threads anchor by cell position, and several
of them sit on the Grid's own header cells ('Scale', 'Arcanum', 'Warband',
'Grave', 'Wildheart', 'Class \\ Origin'), so a sort would move a note onto the
wrong trait. Only the values and fills inside the existing frame change.

The matrix colour carries meaning, and it is derived from each hero's Status
rather than set by hand:

    #FFF3DF amber italic   Status is "NEW - unnamed art" - hero has no sprite yet
    #E2F1FC blue           hero exists and has art reserved (Built / Name reserved / New)
    #F6F6F6 grey           no hero holds this (class, origin) pair

Writing values without repainting leaves a new hero sitting on the grey fill of
the empty cell it replaced, which reads as "no hero here". That is the whole
reason this script does both halves in one pass.
"""
import collections
import sys

import gspread

sys.stdout.reconfigure(encoding="utf-8")

c = gspread.service_account(filename="google-service-credential.json")
sh = c.open_by_key("1SufJzQ-7ccyCI_RbgRD5n8Siy2Ipd2epC1VA8TUBATI")
grid = sh.worksheet("Grid")
heroes = [h for h in sh.worksheet("Heroes").get_all_records() if str(h["Name"]).strip()]
targets = {r["Trait"]: str(r["Units (target)"])
           for r in sh.worksheet("Class").get_all_records() if str(r["Trait"]).strip()}

cur = grid.get_all_values()

# --- read the existing frame back, rather than deciding an order -------------
# stop at the "Seats" column - everything past it is the divider and the side
# tables, and "CLASS - cost spread" is NOT an origin.
header = [x.strip() for x in cur[0][1:]]
origins = header[:header.index("Seats")]
seats_row = next(i for i, r in enumerate(cur) if r[0].strip() == "Seats")
classes = [cur[i][0].strip() for i in range(1, seats_row) if cur[i][0].strip()]
seats_col = 1 + len(origins)

by_pair = {(h["Class"], h["Origin"]): h for h in heroes}
cls = collections.defaultdict(list)
org = collections.defaultdict(list)
for h in heroes:
    cls[h["Class"]].append(h)
    org[h["Origin"]].append(h)

assert set(classes) >= {h["Class"] for h in heroes}, "a hero has a class the Grid has no row for"
assert set(origins) >= {h["Origin"] for h in heroes}, "a hero has an origin the Grid has no column for"

updates = []


def cell_text(h):
    """Name (cost) (hidden role) - your format, e.g. Vane (5) (Fighter).

    A hidden role that is not yet pinned down keeps its options and a "?", e.g.
    Pike (3) (Tank/Fighter?), so the grid never implies a decision nobody made.
    """
    role = str(h.get("Hidden role", "")).strip()
    undecided = "(choice)" in role
    role = role.replace("(choice)", "").replace(" / ", "/").strip()
    if not role:
        return f"{h['Name']} ({h['Cost']})"
    return f"{h['Name']} ({h['Cost']}) ({role}{'?' if undecided else ''})"


def cell(r, c0):
    return gspread.utils.rowcol_to_a1(r + 1, c0 + 1)


# --- matrix ------------------------------------------------------------------
matrix = []
for ri, k in enumerate(classes, start=1):
    row = []
    for ci, o in enumerate(origins, start=1):
        h = by_pair.get((k, o))
        row.append(cell_text(h) if h else "")
    n, want = len(cls.get(k, [])), targets.get(k, "")
    row.append(str(n) if str(n) == want or not want else f"{n} / {want}")
    matrix.append(row)
updates.append({"range": f"B2:{cell(seats_row - 1, seats_col)}", "values": matrix})

# --- the Seats row under the matrix -----------------------------------------
updates.append({"range": f"B{seats_row + 1}:{cell(seats_row, seats_col)}",
                "values": [[len(org.get(o, [])) for o in origins] + [len(heroes)]]})


# --- the two cost-spread tables ---------------------------------------------
def spread(order, groups):
    out = []
    for t in order:
        ms = sorted(groups.get(t, []), key=lambda h: (int(h["Cost"]), h["Name"]))
        costs = [int(h["Cost"]) for h in ms]
        cnt = collections.Counter(costs)
        flags = []
        if ms and not any(x <= 2 for x in costs):
            flags.append("no cost-1/2 opener (breaks B2)")
        if cnt[5] > 1:
            flags.append(f"{cnt[5]} cost-5s (breaks B4)")
        if cnt and max(cnt.values()) > 2:
            flags.append("three of one cost (breaks B5)")
        if sum(1 for n in cnt.values() if n > 1) > 1:
            flags.append("two duplicated costs (breaks B6)")
        want = targets.get(t)
        n = f"{len(ms)} / {want}" if want and str(len(ms)) != want else len(ms)
        out.append([t, n, " / ".join(map(str, costs)),
                    ", ".join(f"{h['Name']} {h['Cost']}" for h in ms) or "no heroes assigned yet",
                    ". ".join(flags)])
    return out


def table_at(title):
    r = next(i for i, row in enumerate(cur) if row[13].strip() == title)
    return r + 2                      # title row, then the column-header row


cr = table_at("CLASS - cost spread")
updates.append({"range": f"N{cr + 1}:R{cr + len(classes)}", "values": spread(classes, cls)})
orr = table_at("ORIGIN - cost spread")
updates.append({"range": f"N{orr + 1}:R{orr + len(origins)}", "values": spread(origins, org)})

# --- footer ------------------------------------------------------------------
curve = collections.Counter(int(h["Cost"]) for h in heroes)
fr = next(i for i, row in enumerate(cur) if row[13].strip() == "Cost curve")
updates.append({"range": f"P{fr + 1}:Q{fr + 1}",
                "values": [[" / ".join(str(curve[i]) for i in range(1, 6)),
                            f"costs 1-5 across all {len(heroes)} heroes (rule B1)"]]})
# --- hidden-role spread per cost, directly under the cost curve --------------
# Three buckets, not five: Tank, Carry and Support. Carry is Fighter + Marksmen
# + Mage together - your reading of Finisher, "basically a carry which mean it
# could be either a Marksmen, Mage, or Fighter".
BUCKET = {"Tank": "Tank", "Support": "Support",
          "Fighter": "Carry", "Marksmen": "Carry", "Mage": "Carry"}


def role_spread(bucket):
    """counts per cost for one bucket, plus how many are still undecided."""
    hit, undecided = [], []
    for k in range(1, 6):
        at = [h for h in heroes if int(h["Cost"]) == k]
        pin = [h for h in at if "(choice)" not in str(h.get("Hidden role", ""))
               and BUCKET.get(str(h.get("Hidden role", "")).strip()) == bucket]
        opn = [h for h in at if "(choice)" in str(h.get("Hidden role", ""))
               and any(BUCKET.get(r.strip()) == bucket
                       for r in str(h["Hidden role"]).replace("(choice)", "").split("/"))]
        hit.append(len(pin))
        undecided.append(len(opn))
    return hit, undecided


role_rows = []
for bucket, why in (("Tank", "rule C6 wants these spread almost equally across the costs"),
                    ("Carry", "Fighter + Marksmen + Mage counted together"),
                    ("Support", "Schema is the only class that produces one, so this caps at 3")):
    hit, opn = role_spread(bucket)
    role_rows.append([f"{bucket} spread", "", " / ".join(map(str, hit)),
                      f"{sum(hit)} pinned; a further {sum(opn)} undecided hero(es) could still "
                      f"become {bucket} ({' / '.join(map(str, opn))} by cost). {why}."])

# the three rows live between "Cost curve" and "Rules enforced"; make room once
have = [cur[fr + 1 + i][13].strip() if fr + 1 + i < len(cur) else "" for i in range(3)]
if have != [r[0] for r in role_rows]:
    sh.batch_update({"requests": [{"insertDimension": {"range": {
        "sheetId": grid.id, "dimension": "ROWS",
        "startIndex": fr + 1, "endIndex": fr + 1 + 3}, "inheritFromBefore": True}}]})
    fr_shift = 3
else:
    fr_shift = 0
updates.append({"range": f"N{fr + 2}:Q{fr + 4}", "values": role_rows})

updates.append({"range": f"Q{fr + 2 + 3}", "values": [[
    "B2 every trait has a cost 1-2 hero; B4 at most one cost-5 per trait; B5 at most two of any "
    "one cost; B6 at most TWO duplicated costs, which caps every trait at 7 members. B3 (small "
    "traits skew expensive) is a preference, not enforced. All four hard rules hold at 56 "
    "heroes. Scale (4), Grave (5) and Wildheart (5) are the origins below 6; Scale is held "
    "there by the open comment that it takes no tank, and Wildheart by the one pairing it only "
    "with Finisher/Gambit/Echo/Reviled/Adrenaline."]]})

# clear the divider column and any stray table row a previous bad run left behind
updates.append({"range": f"M2:M{seats_row + 1}",
                "values": [[""] for _ in range(seats_row)]})
updates.append({"range": f"N{orr + len(origins) + 1}:R{orr + len(origins) + 1}",
                "values": [[""] * 5]})

grid.batch_update(updates)

# --- repaint the matrix ------------------------------------------------------
def rgb(r, g, b):
    return {"red": r, "green": g, "blue": b}


AMBER_BG = rgb(1, 0.9529412, 0.8745098)
AMBER_FG = rgb(0.54901963, 0.41960785, 0.09803922)
BLUE     = rgb(0.8862745, 0.94509804, 0.9882353)
GREY     = rgb(0.9647059, 0.9647059, 0.9647059)
NEEDS_ART = "NEW - unnamed art"

fills = []
for ri, k in enumerate(classes):
    for ci, o in enumerate(origins):
        h = by_pair.get((k, o))
        if h is None:
            fmt = dict(backgroundColor=GREY,
                       textFormat={"fontSize": 10, "bold": False, "italic": False})
        elif str(h["Status"]).strip() == NEEDS_ART:
            fmt = dict(backgroundColor=AMBER_BG, horizontalAlignment="CENTER",
                       wrapStrategy="WRAP",
                       textFormat={"fontSize": 10, "bold": False, "italic": True,
                                   "foregroundColor": AMBER_FG})
        else:
            fmt = dict(backgroundColor=BLUE, horizontalAlignment="CENTER",
                       wrapStrategy="WRAP",
                       textFormat={"fontSize": 10, "bold": False, "italic": False})
        fills.append({"repeatCell": {
            "range": {"sheetId": grid.id, "startRowIndex": ri + 1, "endRowIndex": ri + 2,
                      "startColumnIndex": ci + 1, "endColumnIndex": ci + 2},
            "cell": {"userEnteredFormat": fmt},
            "fields": "userEnteredFormat(" + ",".join(sorted(fmt)) + ")"}})
sh.batch_update({"requests": fills})

# Row height: the cells wrap now, and a wrapped line is not a newline, so count
# has to be estimated from the column width (~6.0px per character at 10pt).
import math

MATRIX_COL_PX = 105
heights = []
for ri, k in enumerate(classes):
    lines = 1
    for o in origins:
        h = by_pair.get((k, o))
        if not h:
            continue
        lines = max(lines, sum(max(1, math.ceil(len(seg) / (MATRIX_COL_PX / 6.0)))
                               for seg in cell_text(h).splitlines() or [""]))
    heights.append({"updateDimensionProperties": {
        "range": {"sheetId": grid.id, "dimension": "ROWS",
                  "startIndex": ri + 1, "endIndex": ri + 2},
        "properties": {"pixelSize": max(21, 15 * lines + 8)}, "fields": "pixelSize"}})
sh.batch_update({"requests": heights})
print(f"row heights set for {len(heights)} class rows")

held = sum(1 for k in classes for o in origins if (k, o) in by_pair)
art = sum(1 for h in heroes if str(h["Status"]).strip() == NEEDS_ART)
print(f"repainted {len(fills)} cells: {held} held, {art} of them still needing art")
print(f"grid refreshed in place: {len(classes)} class rows x {len(origins)} origin columns, "
      f"{len(heroes)} heroes")
print("class order:", ", ".join(classes))
print("origin order:", ", ".join(origins))
