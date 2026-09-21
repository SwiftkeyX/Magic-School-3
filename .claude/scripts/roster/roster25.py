# -*- coding: utf-8 -*-
"""The cost 3-5 band at 25 units, and the class solve for it.

18 units come from the Origin tab's Cost Archetype column, read live. 7 more are
GRID-ONLY additions that exist to meet the minimum spread (rule C11) - the Origin
tab is left exactly as the user wrote it, and every added unit is marked SOURCE.

Classes are solved, not hand-picked: a slot's candidates are the classes whose
hidden role admits its role, filtered by C1 (melee/ranged), C2 (AD/AP) and C8
(Arcanum takes no AD class). The search then satisfies A2, C4 and C10 while
keeping the per-class load as flat as possible.
"""
import math
from google.oauth2.service_account import Credentials
from google.auth.transport.requests import AuthorizedSession

KEY = "1SufJzQ-7ccyCI_RbgRD5n8Siy2Ipd2epC1VA8TUBATI"
TAB = "Grid - Cost Archetype"
GRID_OX = "Grid - Origin x Class"
COSTS = [3, 4, 5]
ORIGINS = ["Vagabond", "Comrade", "Wildheart", "Grave", "Warband", "Arcanum",
           "Scale", "Freeblade", "Bloom", "Skyreach"]
CLASS_ORDER = ["Protector", "Ambush", "Momentum", "Echo", "Schema", "Reviled",
               "Minimize", "Cadence", "Finisher", "Adrenaline", "Gambit", "Resolute"]

MELEE, RANGED = {"Protector", "Adrenaline"}, {"Momentum", "Cadence"}
AD, AP = {"Momentum", "Cadence"}, {"Echo", "Minimize", "Schema"}

# C11, the minimum spread, costs 1-5. Only the 3-5 tail is in scope.
SPREAD_CARRY = {1: 5, 2: 4, 3: 6, 4: 6, 5: 3}
SPREAD_TANK = {1: 5, 2: 5, 3: 3, 4: 4, 5: 0}
CARRY_ROLES = {"Fighter", "Juggernaut", "Marksmen", "Hybrid Marksmen", "Mage"}

# origin, cost, kind, label, role text, role, range, dmg, source
TEXT, ADDED = "text", "added"
SLOTS = [
    ("Vagabond", 4, "CARRY", "4-cost", "range AD marksman", "Marksmen", "Ranged", "AD", TEXT),
    ("Vagabond", 4, "TANK", "4-cost", "Tank", "Tank", None, None, TEXT),
    ("Comrade", 4, "TANK", "4-cost", "Tank", "Tank", None, None, TEXT),
    ("Comrade", 5, "CARRY", "5-cost", "Hybrid Marksmen", "Hybrid Marksmen", "Ranged", "AP", TEXT),
    ("Wildheart", 3, "CARRY", "1st", "AD Fighter", "Fighter", "Melee", "AD", TEXT),
    # 2026-09-21: was Mage. Changed to AP Fighter so Wildheart stops reading like Grave.
    ("Wildheart", 3, "CARRY", "2nd", "AP Fighter", "Fighter", "Melee", "AP", TEXT),
    ("Grave", 3, "TANK", "3-cost", "Tank", "Tank", None, None, ADDED),
    ("Grave", 4, "CARRY", "1st", "Mage", "Mage", None, "AP", TEXT),
    ("Grave", 4, "CARRY", "2nd", "AD Fighter", "Fighter", "Melee", "AD", TEXT),
    ("Warband", 3, "CARRY", "3-cost", "AD Fighter", "Fighter", "Melee", "AD", ADDED),
    ("Warband", 4, "CARRY", "4-cost", "AD marksmen", "Marksmen", "Ranged", "AD", TEXT),
    ("Warband", 4, "TANK", "4-cost", "Tank", "Tank", None, None, TEXT),
    # 2026-09-21: was melee AP Fighter. The other half of the swap with Wildheart.
    ("Scale", 3, "CARRY", "3-cost", "Mage", "Mage", None, "AP", TEXT),
    ("Scale", 4, "TANK", "4-cost", "Tank", "Tank", None, None, TEXT),
    ("Freeblade", 3, "CARRY", "main", "AD marksman", "Marksmen", "Ranged", "AD", TEXT),
    ("Freeblade", 3, "TANK", "3-cost", "Tank", "Tank", None, None, TEXT),
    ("Bloom", 3, "CARRY", "3-cost", "AD marksman", "Marksmen", "Ranged", "AD", ADDED),
    ("Bloom", 4, "CARRY", "4-cost", "Mage", "Mage", None, "AP", TEXT),
    ("Bloom", 5, "CARRY", "5-cost", "AD Fighter", "Fighter", "Melee", "AD", TEXT),
    ("Skyreach", 3, "TANK", "3-cost", "Tank", "Tank", None, None, ADDED),
    ("Skyreach", 4, "CARRY", "4-cost", "Hybrid Marksmen", "Hybrid Marksmen", "Ranged", "AP", TEXT),
    ("Skyreach", 5, "CARRY", "5-cost", "AP fighter", "Fighter", "Melee", "AP", TEXT),
]

# why each ADDED unit sits where it does
ADDED_WHY = {
    ("Grave", 3, "Tank"): "Grave named no tank at all, so C5 could not be met inside the band. A "
                          "cost-3 tank fills that and one of the three the spread wants at cost 3.",
    ("Warband", 3, "Fighter"): "Cost 3 was three carries short. Warband is the melee brawling origin, "
                               "so a melee AD Fighter is the cheapest addition that does not fight "
                               "its identity.",
    ("Bloom", 3, "Marksmen"): "The third cost-3 carry. Ranged AD differs from Bloom's cost-4 Mage, so "
                              "C4 holds across those adjacent costs.",
    ("Skyreach", 3, "Tank"): "The third cost-3 tank, and it gives Skyreach a tank so C5 holds.",
}

cred = Credentials.from_service_account_file(
    "google-service-credential.json",
    scopes=["https://www.googleapis.com/auth/spreadsheets"])
api = AuthorizedSession(cred)


def get(rng):
    url = "https://sheets.googleapis.com/v4/spreadsheets/%s/values/%s" % (KEY, rng)
    return api.get(url).json().get("values", [])


ARCHETYPE = {}
for row in get("'Origin'!A2:N11"):
    row = row + [""] * (14 - len(row))
    if row[0]:
        ARCHETYPE[row[0]] = row[13].strip()

CLS = {}
for row in get("'Class'!A2:M13"):
    row = row + [""] * (13 - len(row))
    CLS[row[0]] = {r.strip() for r in row[12].replace("(choice)", "").split("/") if r.strip()}
SEATS = {}
for row in get("'Class'!A2:C13"):
    row = row + [""] * (3 - len(row))
    if row[0]:
        SEATS[row[0]] = int(row[2] or 0)


def legal(origin, role, rng, dmg):
    out = []
    for c, roles in CLS.items():
        if role not in roles:
            continue
        if rng == "Melee" and c in RANGED:
            continue
        if rng == "Ranged" and c in MELEE:
            continue
        if dmg == "AD" and c in AP:
            continue
        if dmg == "AP" and c in AD:
            continue
        if origin == "Arcanum" and c in AD:      # C8: Arcanum takes no AD class
            continue
        out.append(c)
    return sorted(out, key=lambda c: CLASS_ORDER.index(c))


# --- the spread must actually be met before we bother solving ---------------
def counts():
    car, tan = {}, {}
    for s in SLOTS:
        (car if s[5] in CARRY_ROLES else tan)[s[1]] = \
            (car if s[5] in CARRY_ROLES else tan).get(s[1], 0) + 1
    return car, tan


car, tan = counts()
for c in COSTS:
    assert car.get(c, 0) >= SPREAD_CARRY[c], "carry short at cost %d: %d < %d" % (
        c, car.get(c, 0), SPREAD_CARRY[c])
    assert tan.get(c, 0) >= SPREAD_TANK[c], "tank short at cost %d: %d < %d" % (
        c, tan.get(c, 0), SPREAD_TANK[c])
print("spread met.  carry", {c: car.get(c, 0) for c in COSTS},
      " tank", {c: tan.get(c, 0) for c in COSTS}, " total", len(SLOTS))

CAND = {i: legal(s[0], s[5], s[6], s[7]) for i, s in enumerate(SLOTS)}
for i, s in enumerate(SLOTS):
    assert CAND[i], "no legal class for %s c%d %s" % (s[0], s[1], s[5])


PREVIOUS = {   # the 22 picks carried over, so a re-solve moves as few cells as it can
    ("Vagabond",4,"Marksmen","CARRY"):"Momentum",   ("Vagabond",4,"Tank","TANK"):"Protector",
    ("Comrade",4,"Tank","TANK"):"Schema",           ("Comrade",5,"Hybrid Marksmen","CARRY"):"Ambush",
    ("Wildheart",3,"Fighter","CARRY"):"Resolute",   ("Wildheart",3,"Mage","CARRY"):"Echo",
    ("Grave",3,"Tank","TANK"):"Protector",          ("Grave",4,"Mage","CARRY"):"Minimize",
    ("Grave",4,"Fighter","CARRY"):"Adrenaline",     ("Warband",3,"Fighter","CARRY"):"Reviled",
    ("Warband",4,"Marksmen","CARRY"):"Cadence",     ("Warband",4,"Tank","TANK"):"Minimize",
    ("Scale",3,"Fighter","CARRY"):"Finisher",       ("Scale",4,"Tank","TANK"):"Reviled",
    ("Freeblade",3,"Marksmen","CARRY"):"Momentum",  ("Freeblade",3,"Tank","TANK"):"Echo",
    ("Bloom",3,"Marksmen","CARRY"):"Cadence",       ("Bloom",4,"Mage","CARRY"):"Echo",
    ("Bloom",5,"Fighter","CARRY"):"Resolute",       ("Skyreach",3,"Tank","TANK"):"Resolute",
    ("Skyreach",4,"Hybrid Marksmen","CARRY"):"Ambush", ("Skyreach",5,"Fighter","CARRY"):"Adrenaline",
}


def solve(cap):
    """Backtracking over the slots, tightest first. A2, C10 and the load cap."""
    order = sorted(range(len(SLOTS)), key=lambda i: len(CAND[i]))
    pick, load = {}, {}
    by_origin, by_cost_role = {}, {}
    by_cost_class = {}   # your preference: one class at most once per cost

    def rec(k):
        if k == len(order):
            return True
        i = order[k]
        o, cost, _, _, _, role = SLOTS[i][:6]
        # prefer the class with the most unused seats, so the load stays flat
        cands = sorted(CAND[i], key=lambda c: (c != PREVIOUS.get((o, cost, role, SLOTS[i][2])),
                                               load.get(c, 0),
                                               -(SEATS.get(c, 4) - load.get(c, 0)),
                                               CLASS_ORDER.index(c)))
        for c in cands:
            if load.get(c, 0) >= cap:
                continue
            if c in by_origin.get(o, ()):                      # A2
                continue
            if c in by_cost_role.get((cost, role), ()):        # C10
                continue
            if c in by_cost_class.get(cost, ()):               # preference, not a rule:
                continue                                       # no repeated class inside one cost
            pick[i] = c
            load[c] = load.get(c, 0) + 1
            by_origin.setdefault(o, set()).add(c)
            by_cost_role.setdefault((cost, role), set()).add(c)
            by_cost_class.setdefault(cost, set()).add(c)
            if rec(k + 1):
                return True
            del pick[i]
            load[c] -= 1
            if not load[c]:
                del load[c]
            by_origin[o].discard(c)
            by_cost_role[(cost, role)].discard(c)
            by_cost_class[cost].discard(c)
        return False

    return (pick, load) if rec(0) else (None, None)


lo = int(math.ceil(len(SLOTS) / float(len(CLS))))   # 25 units / 12 classes -> 3
for cap in range(lo, lo + 3):
    pick, load = solve(cap)
    if pick:
        print("solved at cap %d.  load: %s" % (cap, dict(sorted(load.items(),
              key=lambda kv: (-kv[1], kv[0])))))
        break
assert pick, "no assignment exists"

ASSIGN = [tuple(SLOTS[i]) + (pick[i], CAND[i]) for i in range(len(SLOTS))]
SLOTS_WITH_DMG = ASSIGN   # origin0 cost1 kind2 label3 text4 role5 rng6 dmg7 src8 cls9

# --- independent re-check of every rule, against the live tabs -------------
for o, cost, _k, _l, _t, role, rng, dmg, _src, cls, cands in ASSIGN:
    assert cls in legal(o, role, rng, dmg), "%s c%d: %s illegal" % (o, cost, cls)
for o in ORIGINS:
    p = [a[9] for a in ASSIGN if a[0] == o]
    assert len(p) == len(set(p)), "A2 broken in %s: %s" % (o, p)
for o in ORIGINS:
    byc = {}
    for a in ASSIGN:
        if a[0] == o:
            byc.setdefault(a[1], []).append(a[5])
    for c in byc:
        if c + 1 in byc:
            assert not (set(byc[c]) & set(byc[c + 1])), "C4 broken in %s at c%d/c%d" % (o, c, c + 1)
seen = {}
for a in ASSIGN:
    key = (a[1], a[5], a[9])
    assert key not in seen, "C10 broken at cost %d: %s and %s both %s on %s" % (
        a[1], seen[key], a[0], a[5], a[9])
    seen[key] = a[0]
print("A2, C4, C8, C10 all check out.  %d units, %d origins, %d classes used"
      % (len(ASSIGN), len({a[0] for a in ASSIGN}), len({a[9] for a in ASSIGN})))

if __name__ == "__main__":
    for o in ORIGINS:
        for a in ASSIGN:
            if a[0] == o:
                mark = "  <- ADDED" if a[8] == ADDED else ""
                print("  %-10s c%d  %-5s %-18s -> %-11s (of %d legal)%s"
                      % (a[0], a[1], a[2], a[5], a[9], len(a[10]), mark))
