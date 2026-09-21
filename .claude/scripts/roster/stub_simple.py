# -*- coding: utf-8 -*-
"""Stub heroes, generated simply and on purpose.

Stubs are disposable: their only job is to make the trait MEMBER COUNTS right so the
spiderweb has something real to chew on. So this enforces only what affects those
counts - the origin quota, the class quota, and A2 (no repeated origin/class pair) -
plus role legality so nothing is nonsense. It deliberately does NOT enforce C4, C10,
B5, B6 or the one-class-per-cost preference; those shape finished heroes, and if the
spiderweb looks wrong these get thrown away anyway.

Every stub is marked so it can never be mistaken for a designed hero.
"""
from collections import Counter
import render_full as RF
import roster25 as R

CLS, CLASS_ORDER, ORIGINS = R.CLS, R.CLASS_ORDER, R.ORIGINS
MELEE, RANGED, AD, AP = R.MELEE, R.RANGED, R.AD, R.AP
TANKY = {"Reviled", "Adrenaline", "Resolute"}

ORIGIN_TARGET = {"Vagabond": 7, "Comrade": 7, "Freeblade": 7, "Warband": 7,
                 "Skyreach": 7, "Bloom": 7, "Wildheart": 5, "Grave": 5,
                 "Arcanum": 4, "Scale": 4}
ROSTER = sum(ORIGIN_TARGET.values())


def bp(tab, n):
    out = {}
    for row in R.get("'%s'!A2:D%d" % (tab, n + 1)):
        row = row + [""] * (4 - len(row))
        if row[0]:
            out[row[0]] = [int(x.strip()) for x in row[3].split("/") if x.strip().isdigit()]
    return out


ORI_BP, CLA_BP = bp("Origin", 11), bp("Class", 13)
TOP_C = {c: b[-1] for c, b in CLA_BP.items()}

UNITS = [dict(u) for u in RF.UNITS]
for u in UNITS:
    u["stub"] = False
have_o, have_c = Counter(u["o"] for u in UNITS), Counter(u["cls"] for u in UNITS)
# A3: Protector, Resolute and Adrenaline may appear TWICE in one origin. Every other
# class stays one-per-origin under A2.
DOUBLE = {"Protector", "Resolute", "Adrenaline"}


def cap_of(cls):
    return 2 if cls in DOUBLE else 1


used = Counter((u["o"], u["cls"]) for u in UNITS)

need_o = {o: ORIGIN_TARGET[o] - have_o[o] for o in ORIGINS}
assert all(v >= 0 for v in need_o.values()), need_o
TOTAL = sum(need_o.values())

# classes must reach their top breakpoint; the leftover seats go to the classes with
# the deepest breakpoints, since those are the hardest to field.
need_c = {c: max(0, TOP_C[c] - have_c[c]) for c in CLS}
spare = TOTAL - sum(need_c.values())
for c in sorted(CLS, key=lambda c: -TOP_C[c]):
    while spare > 0 and need_c[c] < TOP_C[c] - have_c[c] + 2:
        need_c[c] += 1
        spare -= 1
        break
for c in sorted(CLS, key=lambda c: -TOP_C[c]):
    if spare <= 0:
        break
    need_c[c] += 1
    spare -= 1
assert sum(need_c.values()) == TOTAL, (sum(need_c.values()), TOTAL)

ROLE_REQ = {"Marksmen": ("Ranged", "AD"), "Hybrid Marksmen": ("Ranged", "AP"),
            "Mage": (None, "AP"), "Fighter": ("Melee", None),
            "Juggernaut": ("Melee", None), "Tank": (None, None)}


def roles_for(origin, cls):
    """Roles this class could hold in this origin, under C1, C2 and C8 only."""
    out = []
    for role in CLS.get(cls, ()):
        rng, dmg = ROLE_REQ[role]
        if rng == "Melee" and cls in RANGED:
            continue
        if rng == "Ranged" and cls in MELEE:
            continue
        if dmg == "AD" and cls in AP:
            continue
        if dmg == "AP" and cls in AD:
            continue
        if origin == "Arcanum" and (cls in AD or dmg == "AD"):
            continue
        out.append(role)
    return out


# --- assign (origin, class) pairs: a plain bipartite fill -------------------
pairs = []
oq, cq = dict(need_o), dict(need_c)


def rec():
    if all(v == 0 for v in oq.values()):
        return all(v == 0 for v in cq.values())
    o = max(oq, key=lambda x: oq[x])
    if oq[o] == 0:
        return False
    # the scarcest class that can legally sit in this origin
    cands = [c for c in CLS if cq.get(c, 0) > 0
             and used[(o, c)] < cap_of(c) and roles_for(o, c)]
    cands.sort(key=lambda c: (-cq[c], CLASS_ORDER.index(c)))
    for c in cands:
        oq[o] -= 1; cq[c] -= 1; used[(o, c)] += 1; pairs.append((o, c))
        if rec():
            return True
        pairs.pop(); used[(o, c)] -= 1; oq[o] += 1; cq[c] += 1
    return False


assert rec(), "no origin/class pairing exists even at this loose level"
print("paired %d stubs" % len(pairs))

# --- give each stub a role and a cost, loosely ------------------------------
stubs = []
cost_cycle = {}
for o, c in pairs:
    roles = roles_for(o, c)
    taken = {u["role"] for u in UNITS + stubs if u["o"] == o}
    role = next((r for r in roles if r not in taken), roles[0])
    if role == "Fighter" and c in TANKY:
        role = "Juggernaut"
    used_costs = Counter(u["cost"] for u in UNITS + stubs if u["o"] == o)
    cost = min((1, 2, 3, 4, 5), key=lambda k: (used_costs[k], k))
    stubs.append(dict(o=o, cls=c, cost=cost, role=role,
                      kind=("TANK" if role == "Tank" else "CARRY"),
                      text="STUB " + role, src="stub", stub=True, dmg=None))

ALL = UNITS + stubs
fo, fc = Counter(u["o"] for u in ALL), Counter(u["cls"] for u in ALL)
print("roster:", len(ALL))
print("\norigin seats (target):")
for o in ORIGINS:
    print("   %-10s %d / %d %s" % (o, fo[o], ORIGIN_TARGET[o],
                                   "OK" if fo[o] == ORIGIN_TARGET[o] else "MISMATCH"))
print("\nclass seats (top breakpoint):")
for c in CLASS_ORDER:
    print("   %-11s %d / %d %s" % (c, fc[c], TOP_C[c], "OK" if fc[c] >= TOP_C[c] else "SHORT"))
pc = Counter((u["o"], u["cls"]) for u in ALL)
bad = {k: v for k, v in pc.items() if v > cap_of(k[1])}
assert not bad, "A2/A3 broken: %s" % bad
dbl = {k: v for k, v in pc.items() if v > 1}
print("\nA2 holds. A3 doubles in use:", dbl or "none")
print("\nthe %d stubs:" % len(stubs))
for u in sorted(stubs, key=lambda u: (u["o"], u["cost"])):
    print("   %-10s c%d  %-6s %-16s %s" % (u["o"], u["cost"], u["kind"], u["role"], u["cls"]))
print("\ncosts:", dict(sorted(Counter(u["cost"] for u in ALL).items())))
print("roles:", dict(sorted(Counter(u["role"] for u in ALL).items())))
