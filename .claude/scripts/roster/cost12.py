# -*- coding: utf-8 -*-
"""The cost 1-2 half of the roster: 19 units, solved against the rules.

C11 fixes the counts: cost 1 is 5 carries + 5 tanks, cost 2 is 4 carries + 5 tanks.
Everything else - which origin, which role, which class - is searched, not chosen by
hand, under A2, B2, B5, B6, C1, C2, C3, C5, C8, C9 and C10.

Origin allocation is fixed up front to level the roster: every origin ends on 4
members except Vagabond on 5 (it has the largest stale target of the ones sitting
low). Every origin gets exactly one cost 1-2 TANK, which satisfies C5 everywhere at
once and C9 for Wildheart, whose tank may only be cheap.
"""
import math
from collections import Counter
import roster25 as R

api, KEY = R.api, R.KEY
ORI_LIST = R.ORIGINS
CLASS_ORDER = R.CLASS_ORDER
CLS = R.CLS
MELEE, RANGED, AD, AP = R.MELEE, R.RANGED, R.AD, R.AP

def dmg_of(cls, role, dmg):
    """A hero's damage type: stated if known, else forced by its role, else by its class."""
    if dmg:
        return dmg
    if role == "Marksmen":
        return "AD"
    if role in ("Hybrid Marksmen", "Mage"):
        return "AP"
    if cls in R.AP:
        return "AP"
    if cls in R.AD:
        return "AD"
    return None


# origin, class, cost, role, damage
TANKY = {"Reviled", "Adrenaline", "Resolute"}


def relabel(role, cls):
    """A melee carry in one of the Tank/Fighter classes reads as a Juggernaut."""
    return "Juggernaut" if role == "Fighter" and cls in TANKY else role


EXIST = [(a[0], a[9], a[1], relabel(a[5], a[9]), dmg_of(a[9], a[5], a[7])) for a in R.ASSIGN]

# C4, extended: inside one origin, no two heroes at ADJACENT costs share a hidden role -
# now across all of 1-5, not just the 3-5 band. The 3-5 half is fixed, so when a cost 1-2
# pick clashes with a cost-3 hero it is the cheap one that moves.
# Counter, not a set: two units of one origin can sit at the same cost with the same
# role, and a set would lose the first one's entry when the second backtracks.
ADJ = {}
for _o, _c, _cost, _role, _dm in EXIST:
    ADJ.setdefault((_o, _cost), Counter())[_role] += 1

# ---- origin allocation: (origin, how many carries, how many tanks) -----------
ALLOC = [("Vagabond", 2, 1), ("Comrade", 1, 1), ("Wildheart", 1, 1), ("Grave", 0, 1),
         ("Warband", 0, 1), ("Arcanum", 3, 1), ("Scale", 1, 1), ("Freeblade", 1, 1),
         ("Bloom", 0, 1), ("Skyreach", 0, 1)]
assert sum(c for _, c, _ in ALLOC) == 9 and sum(t for _, _, t in ALLOC) == 10

CARRY_ROLES = ["Fighter", "Juggernaut", "Marksmen", "Hybrid Marksmen", "Mage"]
ROLE_REQ = {"Marksmen": ("Ranged", "AD"), "Hybrid Marksmen": ("Ranged", "AP"),
            "Mage": (None, "AP"), "Fighter": ("Melee", None),
            "Juggernaut": ("Melee", None), "Tank": (None, None)}

USED_PAIR = {(o, c) for o, c, _, _, _ in EXIST}          # A2
# C10 only bites inside one cost, and costs 1-2 are all new, so only the new units matter.

units = []                                            # (origin, kind)
for o, nc, nt in ALLOC:
    units += [(o, "CARRY")] * nc + [(o, "TANK")] * nt
assert len(units) == 19

CAP_COST = {(1, "CARRY"): 5, (1, "TANK"): 5, (2, "CARRY"): 4, (2, "TANK"): 5}


def legal(origin, role, dmg):
    out = []
    rng, rdmg = ROLE_REQ[role]
    dmg = dmg or rdmg
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
        if origin == "Arcanum" and (c in AD or dmg == "AD"):   # C8, read as "no AD at all"
            continue
        out.append(c)
    return sorted(out, key=CLASS_ORDER.index)


def opt_count(o, kind):
    """How many (cost, role, class) options this unit has before any other unit is
    placed. Assigning the most constrained units first is what makes the search with
    the extended C4 finish at all - without it, the tanks forced to cost 1 get chosen
    last and every earlier choice has to be undone."""
    n = 0
    roles = ["Tank"] if kind == "TANK" else CARRY_ROLES
    for cost in (1, 2):
        for role in roles:
            for dmg in (["AD", "AP"] if role in ("Fighter", "Juggernaut", "Tank") else [None]):
                for c in legal(o, role, dmg):
                    if (o, c) in USED_PAIR:
                        continue
                    eff = relabel(role, c)
                    if (ADJ.get((o, cost - 1), {}).get(eff)
                            or ADJ.get((o, cost + 1), {}).get(eff)):
                        continue
                    n += 1
    return n


units.sort(key=lambda u: opt_count(*u))


def solve(max_class_load):
    n = len(units)
    pick = [None] * n
    load = Counter(c for _, c, _, _, _ in EXIST)
    pairs = set(USED_PAIR)
    percost = Counter()
    cost_role_class = set()                            # C10
    origin_cost = Counter((o, c) for o, _, c, _, _ in EXIST)   # B5

    base_dmg = {}
    for _o, _c, _cost, _role, _dm in EXIST:
        base_dmg.setdefault(_o, set()).add(_dm)

    # C3 is a "must eventually hold" condition, so checking it only at the end makes
    # the search backtrack one unit at a time. Check it the moment an origin runs out
    # of units instead - that is the last point at which it can still be fixed.
    LAST = {}
    for j, (uo, _k) in enumerate(units):
        LAST[uo] = j

    def c3_ok(o):
        """Every origin needs an AD and an AP hero. Arcanum is exempt - C8 bars AD
        from it outright, so C3 is unsatisfiable there. That is a conflict between two
        rules, not something the solver can resolve."""
        if o == "Arcanum":
            return True
        ds = set(base_dmg.get(o, set()))
        for (uo, _k), p in zip(units, pick):
            if p and uo == o:
                ds.add(dmg_of(p[2], p[1], p[3]))
        return "AD" in ds and "AP" in ds

    def rec(i):
        if i == n:
            return (all(load.get(c, 0) >= 1 for c in CLS)
                    and len({c for c in CLS if any(p and p[2] == c for p in pick)}) == len(CLS))
        o, kind = units[i]
        opts = []
        for cost in (1, 2):
            if percost[(cost, kind)] >= CAP_COST[(cost, kind)]:
                continue
            if origin_cost[(o, cost)] >= 2:            # B5
                continue
            roles = ["Tank"] if kind == "TANK" else CARRY_ROLES
            for role in roles:
                # A Fighter and a Tank both need an explicit damage type: their roles
                # do not force one, and their class only forces it when it is not Mixed.
                # Without this a Mixed-class tank has no damage type at all and can
                # never satisfy C3 for its origin.
                for dmg in (["AD", "AP"] if role in ("Fighter", "Juggernaut", "Tank") else [None]):
                    for c in legal(o, role, dmg):
                        if (o, c) in pairs:            # A2
                            continue
                        if (cost, role, c) in cost_role_class:   # C10
                            continue
                        eff = relabel(role, c)
                        if (ADJ.get((o, cost - 1), {}).get(eff)
                                or ADJ.get((o, cost + 1), {}).get(eff)):   # C4, extended
                            continue
                        if load.get(c, 0) >= max_class_load:
                            continue
                        opts.append((cost, role, c, dmg))
        # spend scarce classes first: prefer a class with no cheap member yet, then low load
        opts.sort(key=lambda x: (load.get(x[2], 0) > 0 and
                                 any(p and p[2] == x[2] for p in pick), load.get(x[2], 0)))
        for cost, role, c, dmg in opts:
            pick[i] = (cost, role, c, dmg)
            load[c] += 1; pairs.add((o, c)); percost[(cost, kind)] += 1
            cost_role_class.add((cost, role, c)); origin_cost[(o, cost)] += 1
            ADJ.setdefault((o, cost), Counter())[relabel(role, c)] += 1
            if (i != LAST[o] or c3_ok(o)) and rec(i + 1):
                return True
            pick[i] = None
            load[c] -= 1; pairs.discard((o, c)); percost[(cost, kind)] -= 1
            cost_role_class.discard((cost, role, c)); origin_cost[(o, cost)] -= 1
            ADJ[(o, cost)][relabel(role, c)] -= 1
        return False

    return (pick, load) if rec(0) else (None, None)


for cap in (4, 5, 6):
    pick, load = solve(cap)
    if pick:
        print("solved with max class load %d" % cap)
        break
assert pick, "no cost 1-2 assignment exists under the rules"

NEW = []
for (o, kind), (cost, role, cls, dmg) in zip(units, pick):
    NEW.append((o, cls, cost, role, kind, dmg))

ALL = [(o, c, cost, role, "3-5", dm) for o, c, cost, role, dm in EXIST] + \
      [(o, c, cost, role, "1-2", dmg_of(c, role, d)) for o, c, cost, role, _k, d in NEW]

# ---------------------------------------------------------------- rule re-check
def check():
    errs = []
    seen = set()
    for o, c, cost, role, _b, _d in ALL:
        if (o, c) in seen:
            errs.append("A2: %s/%s twice" % (o, c))
        seen.add((o, c))
    byc = {}
    for o, c, cost, role, _b, _d in ALL:
        byc.setdefault((cost, role), set())
        if c in byc[(cost, role)]:
            errs.append("C10: cost %d %s on %s twice" % (cost, role, c))
        byc[(cost, role)].add(c)
    for o in ORI_LIST:                                  # C5 + B2
        mine = [x for x in ALL if x[0] == o]
        if not any(x[3] == "Tank" for x in mine):
            errs.append("C5: %s has no tank" % o)
        if not any(x[2] <= 2 for x in mine):
            errs.append("B2: %s has no cost 1-2 hero" % o)
        if o == "Wildheart":                            # C9
            for x in mine:
                if x[3] == "Tank" and x[2] > 2:
                    errs.append("C9: Wildheart tank at cost %d" % x[2])
    for c in CLS:                                       # B2 for classes
        mine = [x for x in ALL if x[1] == c]
        if not mine:
            errs.append("class %s has no members" % c)
        elif not any(x[2] <= 2 for x in mine):
            errs.append("B2: class %s has no cost 1-2 hero" % c)
    for t in list(ORI_LIST) + list(CLS):                # B5 / B6
        key = 0 if t in ORI_LIST else 1
        costs = Counter(x[2] for x in ALL if x[key] == t)
        if any(v > 2 for v in costs.values()):
            errs.append("B5: %s has 3+ of one cost" % t)
        if sum(1 for v in costs.values() if v > 1) > 2:
            errs.append("B6: %s has 3+ duplicated costs" % t)
    for o in ORI_LIST:                                  # C3
        ds = {x[5] for x in ALL if x[0] == o}
        if "AD" not in ds:
            errs.append("C3: %s holds no AD hero" % o)
        if "AP" not in ds:
            errs.append("C3: %s holds no AP hero" % o)
    return errs


errs = check()
print("rule check:", "ALL PASS" if not errs else errs)

carry = Counter(c for _, _, c, r, _b, _d in ALL if r != "Tank")
tank = Counter(c for _, _, c, r, _b, _d in ALL if r == "Tank")
print("carries by cost:", dict(sorted(carry.items())), " C11 min", R.SPREAD_CARRY)
print("tanks   by cost:", dict(sorted(tank.items())), " C11 min", R.SPREAD_TANK)
for cst in (1, 2, 3, 4, 5):
    assert carry.get(cst, 0) >= R.SPREAD_CARRY[cst], "carry short at %d" % cst
    assert tank.get(cst, 0) >= R.SPREAD_TANK[cst], "tank short at %d" % cst
print("C11 met at every cost.  roster =", len(ALL), "units")
print("origin seats:", dict(sorted(Counter(x[0] for x in ALL).items())))
print("class seats :", dict(sorted(Counter(x[1] for x in ALL).items())))

if __name__ == "__main__":
    print("\nthe 19 new cost 1-2 units:")
    for o, cls, cost, role, kind, dmg in sorted(NEW, key=lambda x: (x[2], x[0])):
        print("  c%d  %-10s %-6s %-16s %-11s %s" % (cost, o, kind, role, cls, dmg or ""))
