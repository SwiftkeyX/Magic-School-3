# -*- coding: utf-8 -*-
"""Render the SpiderWeb to its own tab, one origin at a time, split by tier."""
import json
import math
from google.oauth2.service_account import Credentials
from google.auth.transport.requests import AuthorizedSession

KEY = "1SufJzQ-7ccyCI_RbgRD5n8Siy2Ipd2epC1VA8TUBATI"
TAB = "SpiderWeb"
D = json.load(open("web.json"))

DARK, OCC, EMPTY = "#37464F", "#E2F1FC", "#F6F6F6"
AMB_BG, AMB_FG = "#FFF3DF", "#8C6B19"
NOTE_BG, NOTE_FG, NOTE_BD = "#FFFBE0", "#725402", "#D8B759"
TRAIT_FG = "#0C46A0"
NC = 6
W = [130, 95, 60, 330, 95, 430]

rows, bars, subs, notes, amber, best = [], [], [], [], [], []


def add(cells):
    rows.append(list(cells) + [""] * (NC - len(cells)))
    return len(rows)


bars.append(add(["SPIDERWEB - best boards per origin, per breakpoint tier"]))
notes.append(add(["", "Board size %d, roster %d. One origin at a time; a section holds the boards "
                      "where that origin sits at EXACTLY that tier, so sections never overlap. "
                      "SCORE: every active trait pays by how many UNITS its live breakpoint eats - "
                      "C(b,2), one point per PAIR of members, so b = 2..7 pays 1/3/6/10/15/21 - "
                      "counting classes as well as origins. It is priced by SIZE and not by tier "
                      "index because tier 3 costs 5 units in Freeblade and 7 in Bloom, and one "
                      "number cannot pay for both.  "
                      "HOW IT WAS COMPUTED, honestly: C(60,9) is 14.6 billion boards, so this is NOT "
                      "exhaustive. Each section enumerates every way to pick the focus origin's "
                      "required members, fills the rest by beam search (width 400), then hill-climbs "
                      "by unit swaps until nothing improves. These are the best boards FOUND, not "
                      "proven optimal.  A '*' marks a stub hero."
                  % (D["board"], D["roster"])]))
add([])

cur = None
for s in D["sections"]:
    if s["origin"] != cur:
        cur = s["origin"]
        bars.append(add(["%s   -   %d members" % (cur, s["members"])]))
    head = "%s %d" % (s["origin"], s["bp"])
    if s["dead"]:
        r = add([head, "tier %d" % s["tier"], "%d pt" % s["pts"],
                 "UNREACHABLE - needs %d members, the origin has %d" % (s["dead"], s["members"]),
                 "", "This tier can never be played."])
        amber.append(r)
        continue
    r = add([head, "tier %d" % s["tier"], "%d pt" % s["pts"], "Traits the board turns on",
             "Score", "The 9 units"])
    subs.append(r)
    for i, c in enumerate(s["comps"]):
        live = "  ".join("%s %d (t%d)" % (t, n, ti) for t, n, ti in c["live"])
        r = add(["", "comp %d" % (i + 1), "", live, "%d pts" % c["score"], c["units"]])
        if i == 0:
            best.append(r)
add([])

# which tier of which origin pays best, and which are dead
live = [s for s in D["sections"] if not s["dead"] and s["comps"]]
top = sorted(live, key=lambda s: -s["comps"][0]["score"])[:5]
dead = [s for s in D["sections"] if s["dead"]]
bars.append(add(["WHAT THE WEB SAYS"]))
notes.append(add(["", "HIGHEST-SCORING SECTIONS: " +
                  "; ".join("%s %d = %d pts" % (s["origin"], s["bp"], s["comps"][0]["score"])
                            for s in top) +
                  ".  DEAD TIERS: " +
                  ("; ".join("%s %d (needs %d, has %d)"
                             % (s["origin"], s["bp"], s["dead"], s["members"]) for s in dead)
                   or "none") +
                  ".  THE TRADE THIS EXPOSES: spending 5-7 of 9 slots on one origin leaves little "
                  "room to light anything else, and rule A2 sharpens that - no two members of an "
                  "origin share a class, so taking a whole origin lights NO class at all (Warband, "
                  "with its A3 Resolute double, is the one exception). A deep board is therefore paid "
                  "once per unit where a wide board is paid twice. C(b,2) is the curve that pays for "
                  "that handicap; under the old flat 1/3/5/7 the 7-wide tiers were the weakest boards "
                  "in the game."]))

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
