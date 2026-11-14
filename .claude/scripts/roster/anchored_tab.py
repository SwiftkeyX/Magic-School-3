# -*- coding: utf-8 -*-
"""Render the ANCHORED spiderweb to the SpiderWeb tab - declared comps, not a search.

Reads anchored.json. Each row is a comp that SHOULD be strong: one trait held at one
breakpoint, plus the carry and tank its Cost Archetype names, both forced. The score
only ranks the spare slots - it no longer picks the comp, which is the whole point of
the rewrite (web.py's free search puts an all-tank board on top).
"""
import json
import math
from google.oauth2.service_account import Credentials
from google.auth.transport.requests import AuthorizedSession

KEY = "1SufJzQ-7ccyCI_RbgRD5n8Siy2Ipd2epC1VA8TUBATI"
TAB = "SpiderWeb"
D = json.load(open("anchored.json"))

DARK, OCC, EMPTY = "#37464F", "#E2F1FC", "#F6F6F6"
AMB_BG, AMB_FG = "#FFF3DF", "#8C6B19"
NOTE_BG, NOTE_FG, NOTE_BD = "#FFFBE0", "#725402", "#D8B759"
TRAIT_FG = "#0C46A0"
NC = 6
W = [120, 66, 156, 300, 56, 442]

rows, bars, subs, notes, amber, best = [], [], [], [], [], []


def add(cells):
    rows.append(list(cells) + [""] * (NC - len(cells)))
    return len(rows)


bars.append(add(["SPIDERWEB - the comps that should be strong, and whether the roster can build them"]))
notes.append(add(["", "Board size %d, roster %d. Each section is a DECLARED comp, not a search "
                      "result: one trait held at exactly one breakpoint, plus the carry and tank "
                      "its Cost Archetype names - all forced. The %d origin anchors and %d class "
                      "anchors are the design's own list of what should be playable.  "
                      "RULE 4, origin anchors only: no other active trait may have reached a "
                      "breakpoint ABOVE the anchor's own, so a Grave 4 board is a GRAVE board and "
                      "not a Freeblade 5 board with four Graves in it. Measured on the breakpoint "
                      "REACHED, not the raw count - Grave 4 beside five Resolutes is still "
                      "Resolute 4, a tie, and allowed. Class anchors run uncapped by your call, so "
                      "they still answer 'which origin does this class want'.  "
                      "SCORE: C(b,2), one point per PAIR of members, so a trait at b = 2..7 pays "
                      "1/3/6/10/15/21. It ranks the SPARE slots only; it does not choose the comp. "
                      "That is deliberate - the free search in web.py maximises this score and its "
                      "top board (Warband 7 + Resolute 4, 27 pts) has no carry class on it at all, "
                      "because the score cannot tell a carry from a tank.  "
                      "A '*' marks a stub hero."
                  % (D["board"], D["roster"],
                     sum(1 for s in D["sections"] if s["kind"] == "origin"),
                     sum(1 for s in D["sections"] if s["kind"] == "class"))]))
add([])

cur = None
for s in D["sections"]:
    if s["kind"] != cur:
        cur = s["kind"]
        bars.append(add(["%s ANCHORS" % cur.upper()]))
    head_ = "%s %d" % (s["trait"], s["bp"])
    core = " + ".join("%s 2" % c for c in s["core"]) or "(no Cost Archetype)"
    if s["bad"]:
        why = ("only %d members exist, needs %d" % (s["bad"]["has"], s["bad"]["needs"])
               if s["bad"]["reason"] == "members"
               else "core %s cannot be reached alongside it" % "+".join(s["bad"].get("core", [])))
        amber.append(add([head_, s["kind"], core, "IMPOSSIBLE - " + why, "", ""]))
        continue
    r = add([head_, s["kind"], core, "Traits the board turns on", "Score", "The 9 units"])
    subs.append(r)
    for i, c in enumerate(s["comps"]):
        lv = "  ".join("%s %d" % (t, n) for t, n in sorted(c["live"], key=lambda x: -x[1]))
        r = add(["", "comp %d" % (i + 1), "%d spare" % (s["free"] - 2 * (len(s["core"]) > 0)),
                 lv, "%d pts" % c["score"], c["units"]])
        if i == 0:
            best.append(r)
add([])

bars.append(add(["WHAT THE WEB SAYS"]))
imp = [s for s in D["sections"] if s["bad"]]
tight = [s for s in D["sections"] if not s["bad"] and len(s["comps"]) == 1]
notes.append(add(["", "IMPOSSIBLE COMPS: " +
                  ("; ".join("%s %d" % (s["trait"], s["bp"]) for s in imp) or "none - every "
                   "declared comp can be built") +
                  ".  ONLY ONE WAY TO BUILD IT: " +
                  ("; ".join("%s %d" % (s["trait"], s["bp"]) for s in tight) or "none") +
                  " - a 7-wide anchor plus a two-class core spends all 9 slots, so there is no "
                  "variation left and the comp is fully determined.  "
                  "WHY THE CORE IS FORCED: rule A2 gives an origin's members all different "
                  "classes, so every class an origin brings sits at 1 and needs a partner from "
                  "outside. The leftover slots are the entire budget, and without forcing the "
                  "core the optimiser spends them on whatever scores most - which is a second "
                  "tank class, every time."]))

s = AuthorizedSession(Credentials.from_service_account_file(
    "google-service-credential.json",
    scopes=["https://www.googleapis.com/auth/spreadsheets"]))
meta = s.get("https://sheets.googleapis.com/v4/spreadsheets/" + KEY,
             params={"fields": "sheets(properties(title,sheetId))"}).json()
ex = {x["properties"]["title"]: x["properties"]["sheetId"] for x in meta["sheets"]}
if TAB in ex:
    sid = ex[TAB]
else:
    sid = s.post("https://sheets.googleapis.com/v4/spreadsheets/%s:batchUpdate" % KEY,
                 json={"requests": [{"addSheet": {"properties": {"title": TAB}}}]}
                 ).json()["replies"][0]["addSheet"]["properties"]["sheetId"]
s.post("https://sheets.googleapis.com/v4/spreadsheets/%s:batchUpdate" % KEY,
       json={"requests": [{"unmergeCells": {"range": {"sheetId": sid}}},
                          {"updateCells": {"range": {"sheetId": sid},
                                           "fields": "userEnteredValue"}}]})
r = s.put("https://sheets.googleapis.com/v4/spreadsheets/%s/values/'%s'!A1" % (KEY, TAB),
          params={"valueInputOption": "RAW"}, json={"values": rows})
print("values", r.status_code, r.json().get("updatedRange"))


def rgb(h):
    h = h.lstrip("#")
    return {"red": int(h[0:2], 16) / 255.0, "green": int(h[2:4], 16) / 255.0,
            "blue": int(h[4:6], 16) / 255.0}


def rng(r0, r1, c0, c1):
    return {"sheetId": sid, "startRowIndex": r0 - 1, "endRowIndex": r1,
            "startColumnIndex": c0, "endColumnIndex": c1 + 1}


def fmt(r0, r1, c0, c1, bg=None, fg=None, bold=None, italic=None, size=10, halign=None):
    c, f = {"wrapStrategy": "WRAP", "verticalAlignment": "TOP"}, [
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


req = [{"updateSheetProperties": {"properties": {"sheetId": sid, "gridProperties": {
    "rowCount": len(rows) + 3, "columnCount": NC, "frozenRowCount": 1}},
    "fields": "gridProperties(rowCount,columnCount,frozenRowCount)"}}]
req.append(fmt(1, len(rows), 0, NC - 1, bg="#FFFFFF", size=9))
req.append(fmt(1, len(rows), 0, 0, fg=TRAIT_FG, bold=True, size=10))
for r_ in bars:
    req.append(fmt(r_, r_, 0, NC - 1, bg=DARK, fg="#FFFFFF", bold=True, size=10))
    req.append({"mergeCells": {"range": rng(r_, r_, 0, NC - 1), "mergeType": "MERGE_ALL"}})
for r_ in subs:
    req.append(fmt(r_, r_, 0, NC - 1, bg=OCC, bold=True, size=9))
for r_ in best:
    req.append(fmt(r_, r_, 3, 4, bg=OCC, bold=True))
for r_ in amber:
    req.append(fmt(r_, r_, 0, NC - 1, bg=AMB_BG, fg=AMB_FG, italic=True))
for r_ in notes:
    req.append({"mergeCells": {"range": rng(r_, r_, 1, NC - 1), "mergeType": "MERGE_ALL"}})
    req.append(fmt(r_, r_, 1, NC - 1, bg=NOTE_BG, fg=NOTE_FG, size=9))
    req.append({"updateBorders": {"range": rng(r_, r_, 1, NC - 1),
                                  "top": {"style": "SOLID", "color": rgb(NOTE_BD)},
                                  "bottom": {"style": "SOLID", "color": rgb(NOTE_BD)},
                                  "left": {"style": "SOLID", "color": rgb(NOTE_BD)},
                                  "right": {"style": "SOLID", "color": rgb(NOTE_BD)}}})
for c, w in enumerate(W):
    req.append({"updateDimensionProperties": {
        "range": {"sheetId": sid, "dimension": "COLUMNS", "startIndex": c, "endIndex": c + 1},
        "properties": {"pixelSize": w}, "fields": "pixelSize"}})
for i, row in enumerate(rows, 1):
    if i in notes:
        n = max(1, int(math.ceil(len(row[1]) / float(sum(W[1:]) / 5.8))))
    elif i in bars:
        n = 1
    else:
        n = max([1] + [int(math.ceil(len(str(v)) / float(W[c] / 5.8)))
                       for c, v in enumerate(row) if v])
    req.append({"updateDimensionProperties": {
        "range": {"sheetId": sid, "dimension": "ROWS", "startIndex": i - 1, "endIndex": i},
        "properties": {"pixelSize": max(19, 15 * n + 5)}, "fields": "pixelSize"}})
r = s.post("https://sheets.googleapis.com/v4/spreadsheets/%s:batchUpdate" % KEY,
           json={"requests": req})
print("format", r.status_code, "| rows", len(rows))
