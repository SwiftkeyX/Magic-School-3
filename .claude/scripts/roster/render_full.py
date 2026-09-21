# -*- coding: utf-8 -*-
"""Render the whole 41-unit roster: costs 1-5, both grids."""
import math
from collections import Counter
import cost12 as C
import roster25 as R

api, KEY = R.api, R.KEY
ORIGINS, CLASS_ORDER = R.ORIGINS, R.CLASS_ORDER
COSTS = [1, 2, 3, 4, 5]
TAB_COST, TAB_OX = "Grid - Cost Archetype", "Grid - Origin x Class"

# unified unit record: origin, cost, kind, role text, role, class, source
UNITS = []
for a in R.ASSIGN:                       # cost 3-5
    UNITS.append(dict(o=a[0], cost=a[1], kind=a[2], text=a[4], role=a[5], cls=a[9],
                      src=("added" if a[8] == R.ADDED else "text")))
for o, cls, cost, role, kind, dmg in C.NEW:      # cost 1-2
    txt = role if not dmg else "%s %s" % (dmg, role)
    UNITS.append(dict(o=o, cost=cost, kind=kind, text=txt, role=role, cls=cls, src="new12"))
assert len(UNITS) == 41

# A hero holds exactly ONE hidden role, so every melee carry needs a call between
# Fighter (fragile, massive damage) and Juggernaut (tankier, still hits hard). All six
# Fighter-capable classes may hold either, so this is a per-hero choice, not a class
# rule. Default applied: a melee carry sitting in one of the three Tank/Fighter classes
# reads as a Juggernaut; the ones in Ambush, Finisher and Gambit stay Fighters.
TANKY = {"Reviled", "Adrenaline", "Resolute"}
JUGGER = 0
for u in UNITS:
    if u["role"] == "Fighter" and u["cls"] in TANKY:
        u["role"] = "Juggernaut"
        u["text"] = u["text"].replace("Fighter", "Juggernaut").replace("fighter", "Juggernaut")
        u["jug"] = True
        JUGGER += 1
print("relabelled %d melee carries as Juggernaut" % JUGGER)

# relabelling can only relax C10 (same cost + same role + same class), never break it
seen = set()
for u in UNITS:
    k = (u["cost"], u["role"], u["cls"])
    assert k not in seen, "C10 broken after relabel: %s" % (k,)
    seen.add(k)
print("C10 still holds after the relabel")

BLUE, AMBER, GREEN, EMPTY, DARK = "#E2F1FC", "#FFF3DF", "#E6F4EA", "#F6F6F6", "#37464F"
AMB_FG, NOTE_BG, NOTE_FG, NOTE_BD = "#8C6B19", "#FFFBE0", "#725402", "#D8B759"
TRAIT_FG, FLAG_FG = "#0C46A0", "#B73F0C"
FILL = {"text": BLUE, "added": AMBER, "new12": GREEN}
SRC_TEXT = {"text": "your Cost Archetype text",
            "added": "ADDED for C11 - not in your text",
            "new12": "NEW cost 1-2, solved from the rules"}


def rgb(h):
    h = h.lstrip("#")
    return {"red": int(h[0:2], 16) / 255.0, "green": int(h[2:4], 16) / 255.0,
            "blue": int(h[4:6], 16) / 255.0}


def mk(sid):
    def rng(r0, r1, c0, c1):
        return {"sheetId": sid, "startRowIndex": r0 - 1, "endRowIndex": r1,
                "startColumnIndex": c0, "endColumnIndex": c1 + 1}

    def fmt(r0, r1, c0, c1, bg=None, fg=None, bold=None, italic=None, size=10,
            halign=None, valign="TOP"):
        c, f = {"wrapStrategy": "WRAP", "verticalAlignment": valign}, [
            "userEnteredFormat.wrapStrategy", "userEnteredFormat.verticalAlignment"]
        if bg:
            c["backgroundColor"] = rgb(bg); f.append("userEnteredFormat.backgroundColor")
        tf = {}
        if fg:
            tf["foregroundColor"] = rgb(fg); f.append("userEnteredFormat.textFormat.foregroundColor")
        if bold is not None:
            tf["bold"] = bold; f.append("userEnteredFormat.textFormat.bold")
        if italic is not None:
            tf["italic"] = italic; f.append("userEnteredFormat.textFormat.italic")
        if size:
            tf["fontSize"] = size; f.append("userEnteredFormat.textFormat.fontSize")
        if tf:
            c["textFormat"] = tf
        if halign:
            c["horizontalAlignment"] = halign; f.append("userEnteredFormat.horizontalAlignment")
        return {"repeatCell": {"range": rng(r0, r1, c0, c1), "cell": {"userEnteredFormat": c},
                               "fields": ",".join(sorted(set(f)))}}
    return rng, fmt


def tab_id(title, ncols, nrows):
    meta = api.get("https://sheets.googleapis.com/v4/spreadsheets/" + KEY,
                   params={"fields": "sheets(properties(title,sheetId))"}).json()
    ex = {s["properties"]["title"]: s["properties"]["sheetId"] for s in meta["sheets"]}
    if title in ex:
        return ex[title]
    r = api.post("https://sheets.googleapis.com/v4/spreadsheets/%s:batchUpdate" % KEY,
                 json={"requests": [{"addSheet": {"properties": {"title": title, "gridProperties":
                       {"rowCount": nrows, "columnCount": ncols}}}}]})
    return r.json()["replies"][0]["addSheet"]["properties"]["sheetId"]


def push(title, sid, rows, ncols, check):
    api.post("https://sheets.googleapis.com/v4/spreadsheets/%s:batchUpdate" % KEY,
             json={"requests": [{"unmergeCells": {"range": {"sheetId": sid}}},
                                {"updateCells": {"range": {"sheetId": sid},
                                                 "fields": "userEnteredValue"}}]})
    api.put("https://sheets.googleapis.com/v4/spreadsheets/%s/values/'%s'!A1" % (KEY, title),
            params={"valueInputOption": "RAW"}, json={"values": rows})
    col = chr(64 + ncols) if ncols <= 26 else "A" + chr(64 + ncols - 26)
    got = api.get("https://sheets.googleapis.com/v4/spreadsheets/%s/values/'%s'!A1:%s%d"
                  % (KEY, title, col, len(rows))).json().get("values", [])
    for r_, c_ in check:
        v = got[r_ - 1][c_] if r_ - 1 < len(got) and c_ < len(got[r_ - 1]) else ""
        assert str(v).strip(), "%s: r%d c%d did not land" % (title, r_, c_)
    print("%-24s %d rows, %d unit cells verified" % (title, len(rows), len(check)))


def heights(req, sid, rows, W, wide, merged=None):
    def wrapped(t, px):
        cpl = max(4, int(px / 6.4))
        return sum(max(1, int(math.ceil(len(s) / float(cpl)))) for s in str(t).split("\n"))
    for i, row in enumerate(rows, 1):
        if i in wide:
            n = wrapped(row[1], sum(W[1:]))
        elif merged and i in merged:
            first, last, cols = merged[i]
            span = last - first + 1
            n = max([1] + [int(math.ceil(wrapped(row[c], W[c]) / float(span))) if c in cols
                           else wrapped(row[c], W[c]) for c in range(len(row)) if row[c]])
        else:
            n = max([1] + [wrapped(v, W[c]) for c, v in enumerate(row) if v])
        req.append({"updateDimensionProperties": {
            "range": {"sheetId": sid, "dimension": "ROWS", "startIndex": i - 1, "endIndex": i},
            "properties": {"pixelSize": max(21, 18 * n + 6)}, "fields": "pixelSize"}})
    for c, w in enumerate(W):
        req.append({"updateDimensionProperties": {
            "range": {"sheetId": sid, "dimension": "COLUMNS", "startIndex": c, "endIndex": c + 1},
            "properties": {"pixelSize": w}, "fields": "pixelSize"}})


# =====================================================================
def build_cost_grid():
    HEAD = ["Origin", "Cost Archetype (your words)"] + ["cost %d" % c for c in COSTS] + \
           ["Units", "Where each unit came from"]
    NC = len(HEAD)
    W = [100, 190, 135, 135, 135, 135, 135, 48, 250]
    rows, blocks, cells = [HEAD], [], []
    for o in ORIGINS:
        mine = sorted([u for u in UNITS if u["o"] == o], key=lambda u: (u["cost"], u["kind"]))
        first = len(rows) + 1
        for i, u in enumerate(mine):
            row = [o if i == 0 else "",
                   (R.ARCHETYPE[o] or "(nothing written)") if i == 0 else "",
                   "", "", "", "", "", str(len(mine)) if i == 0 else "",
                   "c%d %s - %s%s" % (u["cost"], u["cls"], SRC_TEXT[u["src"]],
                    "  |  read as JUGGERNAUT, not Fighter" if u.get("jug") else "")]
            col = 2 + COSTS.index(u["cost"])
            row[col] = "%s\n%s\n\nclass: %s\nname: ___" % (u["kind"], u["text"], u["cls"])
            cells.append((len(rows) + 1, col, u["src"]))
            rows.append(row)
        blocks.append((o, first, len(rows)))
    last = blocks[-1][2]

    r_tot = len(rows) + 1
    rows.append(["Units", ""] + [str(sum(1 for u in UNITS if u["cost"] == c)) for c in COSTS] +
                [str(len(UNITS)), ""])
    r_sp = len(rows) + 1
    for lbl, roles, tgt in (("Carries vs C11", R.CARRY_ROLES, R.SPREAD_CARRY),
                            ("Tanks vs C11", {"Tank"}, R.SPREAD_TANK)):
        got = [sum(1 for u in UNITS if u["cost"] == c and u["role"] in roles) for c in COSTS]
        rows.append([lbl, "min " + " / ".join(str(tgt[c]) for c in COSTS)] +
                    ["%d (min %d)" % (g, tgt[c]) for g, c in zip(got, COSTS)] +
                    [str(sum(got)), ""])
    rows.append([""] * NC)
    r_leg = len(rows) + 1
    load = Counter(u["cls"] for u in UNITS)
    rows.append(["", "THE WHOLE ROSTER, COSTS 1-5. One hero per row and per cell.  "
                     "BLUE = named in the Origin tab's Cost Archetype column. AMBER = added at cost "
                     "3-5 to meet C11's minimum spread, not in your text. GREEN = the cost 1-2 half, "
                     "which your text says nothing about - origin, role and class were all solved "
                     "from the rules.  "
                     "Every origin got exactly one cost 1-2 TANK, which satisfies C5 everywhere at "
                     "once and C9 for Wildheart, whose tank may only be cheap.  "
                     "RULES CHECKED after solving, not assumed: A2, B2, B5, B6, C1, C2, C5, C8, C9, "
                     "C10 all pass. C3 fails for ARCANUM ONLY and cannot be fixed - C8 bars AD from "
                     "Arcanum while C3 demands every origin hold an AD hero. That is a conflict "
                     "between two rules, awaiting your ruling.  "
                     "Class seats: " + "  ".join("%s %d" % (c, load[c]) for c in CLASS_ORDER)] +
                [""] * (NC - 2))

    sid = tab_id(TAB_COST, NC, len(rows) + 4)
    push(TAB_COST, sid, rows, NC, [(r, c) for r, c, _ in cells])
    rng, fmt = mk(sid)
    req = [{"updateSheetProperties": {"properties": {"sheetId": sid, "gridProperties": {
        "rowCount": len(rows) + 3, "columnCount": NC, "frozenRowCount": 1,
        "frozenColumnCount": 1}},
        "fields": "gridProperties(rowCount,columnCount,frozenRowCount,frozenColumnCount)"}}]
    req.append(fmt(1, len(rows), 0, NC - 1, bg="#FFFFFF", size=10))
    req.append(fmt(2, last, 2, 6, bg=EMPTY, halign="CENTER"))
    req.append(fmt(1, 1, 0, NC - 1, bg=DARK, fg="#FFFFFF", bold=True, halign="CENTER",
                   valign="MIDDLE"))
    req.append(fmt(2, last, 0, 0, bg="#FFFFFF", fg=TRAIT_FG, bold=True, valign="MIDDLE"))
    req.append(fmt(2, last, 7, 7, halign="CENTER", valign="MIDDLE"))
    req.append(fmt(2, last, 8, 8, fg=FLAG_FG, size=9))
    for r_, c_, src in cells:
        req.append(fmt(r_, r_, c_, c_, bg=FILL[src], halign="CENTER",
                       fg=AMB_FG if src == "added" else None,
                       italic=True if src == "added" else None))
    merged = {}
    for o, first, lastr in blocks:
        if lastr > first:
            for col in (0, 1, 7):
                req.append({"mergeCells": {"range": rng(first, lastr, col, col),
                                           "mergeType": "MERGE_ALL"}})
            for rr in range(first, lastr + 1):
                merged[rr] = (first, lastr, (0, 1, 7))
        req.append({"updateBorders": {"range": rng(lastr, lastr, 0, NC - 1),
                                      "bottom": {"style": "SOLID", "color": rgb("#B0BEC5")}}})
    req.append(fmt(r_tot, r_tot, 0, NC - 1, bg=DARK, fg="#FFFFFF", bold=True, halign="CENTER",
                   valign="MIDDLE"))
    req.append(fmt(r_sp, r_sp + 1, 0, 0, bg=DARK, fg="#FFFFFF", bold=True, valign="MIDDLE"))
    req.append(fmt(r_sp, r_sp + 1, 1, NC - 1, bg=NOTE_BG, fg=NOTE_FG, size=9, halign="CENTER"))
    req.append({"mergeCells": {"range": rng(r_leg, r_leg, 1, NC - 1), "mergeType": "MERGE_ALL"}})
    req.append(fmt(r_leg, r_leg, 1, NC - 1, bg=NOTE_BG, fg=NOTE_FG, size=9))
    req.append({"updateBorders": {"range": rng(r_leg, r_leg, 1, NC - 1),
                                  "top": {"style": "SOLID", "color": rgb(NOTE_BD)},
                                  "bottom": {"style": "SOLID", "color": rgb(NOTE_BD)},
                                  "left": {"style": "SOLID", "color": rgb(NOTE_BD)},
                                  "right": {"style": "SOLID", "color": rgb(NOTE_BD)}}})
    heights(req, sid, rows, W, {r_leg}, merged)
    r = api.post("https://sheets.googleapis.com/v4/spreadsheets/%s:batchUpdate" % KEY,
                 json={"requests": req})
    print("%-24s format %s" % (TAB_COST, r.status_code))


def build_ox():
    NC = 1 + len(CLASS_ORDER) + 1
    W = [100] + [92] * len(CLASS_ORDER) + [50]
    rows = [["Origin \\ Class"] + CLASS_ORDER + ["Seats"]]
    cells = []
    for o in ORIGINS:
        row = [o] + [""] * len(CLASS_ORDER)
        n = 0
        for u in sorted([x for x in UNITS if x["o"] == o], key=lambda x: x["cost"]):
            ci = 1 + CLASS_ORDER.index(u["cls"])
            row[ci] = "c%d %s\n%s" % (u["cost"], u["kind"], u["role"])
            cells.append((len(rows) + 1, ci, u["src"]))
            n += 1
        rows.append(row + [str(n)])
    r_seats = len(rows) + 1
    rows.append(["Seats"] + [str(sum(1 for u in UNITS if u["cls"] == c)) for c in CLASS_ORDER] +
                [str(len(UNITS))])
    rows.append(["Carries"] + [str(sum(1 for u in UNITS if u["cls"] == c
                                       and u["role"] in R.CARRY_ROLES)) for c in CLASS_ORDER] +
                [str(sum(1 for u in UNITS if u["role"] in R.CARRY_ROLES))])
    rows.append(["Tanks"] + [str(sum(1 for u in UNITS if u["cls"] == c and u["role"] == "Tank"))
                             for c in CLASS_ORDER] +
                [str(sum(1 for u in UNITS if u["role"] == "Tank"))])
    rows.append([""] * NC)
    r_leg = len(rows) + 1
    rows.append(["", "THE WHOLE ROSTER AS ORIGIN x CLASS, costs 1-5, 41 units. Each cell is one "
                     "hero: its cost, whether it is the CARRY or the TANK, and its hidden role. "
                     "BLUE = from your Cost Archetype text, AMBER = added at cost 3-5 for C11, "
                     "GREEN = the solved cost 1-2 half. Every cell obeys A2, so no origin repeats a "
                     "class - which is why the grid is sparse: 41 of the 120 cells are used."] +
                [""] * (NC - 2))

    sid = tab_id(TAB_OX, NC, len(rows) + 4)
    push(TAB_OX, sid, rows, NC, [(r, c) for r, c, _ in cells])
    rng, fmt = mk(sid)
    req = [{"updateSheetProperties": {"properties": {"sheetId": sid, "gridProperties": {
        "rowCount": len(rows) + 3, "columnCount": NC, "frozenRowCount": 1,
        "frozenColumnCount": 1}},
        "fields": "gridProperties(rowCount,columnCount,frozenRowCount,frozenColumnCount)"}}]
    req.append(fmt(1, len(rows), 0, NC - 1, bg="#FFFFFF", size=9))
    req.append(fmt(2, 1 + len(ORIGINS), 1, len(CLASS_ORDER), bg=EMPTY, halign="CENTER"))
    req.append(fmt(1, 1, 0, NC - 1, bg=DARK, fg="#FFFFFF", bold=True, halign="CENTER",
                   valign="MIDDLE"))
    req.append(fmt(2, 1 + len(ORIGINS), 0, 0, bg=DARK, fg="#FFFFFF", bold=True, valign="MIDDLE"))
    req.append(fmt(2, 1 + len(ORIGINS), NC - 1, NC - 1, bg=DARK, fg="#FFFFFF", bold=True,
                   halign="CENTER", valign="MIDDLE"))
    for r_, c_, src in cells:
        req.append(fmt(r_, r_, c_, c_, bg=FILL[src], halign="CENTER", size=9,
                       fg=AMB_FG if src == "added" else None,
                       italic=True if src == "added" else None))
    req.append(fmt(r_seats, r_seats + 2, 0, NC - 1, bg=DARK, fg="#FFFFFF", bold=True,
                   halign="CENTER", valign="MIDDLE"))
    req.append({"mergeCells": {"range": rng(r_leg, r_leg, 1, NC - 1), "mergeType": "MERGE_ALL"}})
    req.append(fmt(r_leg, r_leg, 1, NC - 1, bg=NOTE_BG, fg=NOTE_FG, size=9))
    heights(req, sid, rows, W, {r_leg})
    r = api.post("https://sheets.googleapis.com/v4/spreadsheets/%s:batchUpdate" % KEY,
                 json={"requests": req})
    print("%-24s format %s" % (TAB_OX, r.status_code))


if __name__ == "__main__":
    build_cost_grid()
    build_ox()
