# -*- coding: utf-8 -*-
"""The ANCHORED spiderweb - comps that SHOULD be strong, checked against the roster.

This is not a search for the best 9 units; web.py does that and its answer is a board
with no carry on it (Warband 7 + Resolute 4 scores highest and every active class on
it is a tank). A comp is instead DECLARED - one trait held at one breakpoint, plus the
carry and tank its Cost Archetype names - and the job here is to check each declared
comp is buildable and report what the leftover slots reach.

So the score stops CHOOSING the comp and only DESCRIBES it: the anchor and the core are
constraints, and C(b,2) ranks the spare slots alone.

CORE: read off the Origin tab's Cost Archetype column - a "standard" origin contributes
one carry and one tank, a "duo carry" origin two carries - and resolved to CLASSES
through the roster, since a breakpoint is a class and "Juggernaut" is only a role.
"""
import random
from collections import Counter
import stub_simple as S
import roster25 as R

UNITS = S.ALL
BP = dict(S.ORI_BP); BP.update(S.CLA_BP)
PAIRS = [0, 0, 1, 3, 6, 10, 15, 21, 28, 36]          # C(b,2)
BOARD = 9
N = len(UNITS)

TRAITS = sorted(BP)
TI = {t: i for i, t in enumerate(TRAITS)}
NT = len(TRAITS)
UO = [TI[u["o"]] for u in UNITS]
UC = [TI[u["cls"]] for u in UNITS]

TIER = [[0] * (BOARD + 1) for _ in range(NT)]
VAL = [[0] * (BOARD + 1) for _ in range(NT)]
BPS = [[0] * (BOARD + 1) for _ in range(NT)]      # the BREAKPOINT a count has reached
for _t, _bps in BP.items():
    for _n in range(BOARD + 1):
        _k = 0
        for _i, _b in enumerate(_bps):
            if _n >= _b:
                _k = _i + 1
        TIER[TI[_t]][_n] = _k
        BPS[TI[_t]][_n] = _bps[_k - 1] if _k else 0
        VAL[TI[_t]][_n] = PAIRS[_bps[_k - 1]] if _k else 0

# --- the declared list ------------------------------------------------------
ORIGIN_ANCHORS = [("Vagabond", 6), ("Comrade", 6), ("Warband", 5), ("Warband", 7),
                  ("Freeblade", 5), ("Freeblade", 7), ("Bloom", 7),
                  ("Skyreach", 5), ("Skyreach", 7),
                  ("Wildheart", 4), ("Grave", 4), ("Arcanum", 4), ("Scale", 4)]
CLASS_ANCHORS = [("Ambush", 4), ("Momentum", 4), ("Echo", 4),
                 ("Cadence", 4), ("Finisher", 4), ("Gambit", 4)]

# The least an origin anchor's best comp must score, by its breakpoint. Locked by the
# user 2026-09-27. Class anchors have no floor - they answer "which origin", not "how strong".
FLOORS = {7: 24, 6: 21, 5: 18, 4: 17}

# Cost Archetype, transcribed by hand from the Origin tab - (kind, cost) per core slot.
# Like roster25.SLOTS this must be kept in step with that column by hand.
ARCHETYPE = {
    "Vagabond":  [("CARRY", 4), ("TANK", 4)],          # 4-cost flex standard
    "Comrade":   [("CARRY", 5), ("TANK", 4)],          # 4-5-cost flex standard
    "Wildheart": [("CARRY", 3), ("CARRY", 3)],         # 3-cost duo carry
    "Grave":     [("CARRY", 4), ("CARRY", 4)],         # 4-cost flex duo carry
    "Warband":   [("CARRY", 4), ("TANK", 4)],          # 4-cost flex standard
    "Arcanum":   [],                                   # Cost Archetype cell is BLANK
    "Scale":     [("CARRY", 3), ("TANK", 4)],          # special, 1/2/3/4
    "Freeblade": [("CARRY", 3), ("TANK", 3)],          # 3-cost standard
    "Bloom":     [("CARRY", 4), ("CARRY", 5)],         # 4-5-cost flex duo carry
    "Skyreach":  [("CARRY", 4), ("CARRY", 5)],         # 4-5-cost flex duo carry
}


def core_of(origin):
    """The archetype's (kind, cost) slots resolved to this origin's actual units.

    A duo-carry origin names two carries at ONE cost (Wildheart c3, Grave c4), so slots
    are consumed without replacement rather than required to be unique - ordered by
    class name so the same two units come back every run.
    """
    out = []
    for kind, cost in ARCHETYPE.get(origin, []):
        hits = sorted((i for i in range(N) if UNITS[i]["o"] == origin
                       and UNITS[i]["kind"] == kind and UNITS[i]["cost"] == cost
                       and i not in out), key=lambda i: UNITS[i]["cls"])
        assert hits, "%s has no %s at cost %d" % (origin, kind, cost)
        out.append(hits[0])
    return out


def counts_of(bd):
    c = [0] * NT
    for i in bd:
        c[UO[i]] += 1
        c[UC[i]] += 1
    return c


def score_of(c):
    return sum(VAL[t][c[t]] for t in range(NT) if c[t])


def window(trait, bp):
    bps = BP[trait]
    i = bps.index(bp)
    return bp, (bps[i + 1] if i + 1 < len(bps) else 99)


def ok(c, ti, lo, hi, need, cap):
    """cap: no active trait may have reached a breakpoint ABOVE the anchor's own.

    Measured on the breakpoint REACHED, not the raw member count - an anchor at 4
    alongside 5 Resolutes is still Resolute 4, which is a tie and allowed."""
    if not (lo <= c[ti] < hi):
        return False
    if any(c[t] < 2 for t in need):
        return False
    if cap:
        for t in range(NT):
            if c[t] and BPS[t][c[t]] > cap:
                return False
    return True


def repair(bd, ti, lo, hi, need, cap, rng, tries=60):
    """Swap out units that push a trait past the cap until the board is legal."""
    bd = list(bd)
    for _ in range(tries):
        c = counts_of(bd)
        if ok(c, ti, lo, hi, need, cap):
            return bd
        over = [t for t in range(NT) if c[t] and cap and BPS[t][c[t]] > cap]
        if not over:
            return None
        t = rng.choice(over)
        movable = [i for i in bd if (UO[i] == t or UC[i] == t) and UO[i] != ti
                   and UC[i] not in need]
        if not movable:
            return None
        drop = rng.choice(movable)
        cands = [j for j in range(N) if j not in bd and UO[j] != ti and UO[j] != t and UC[j] != t]
        if not cands:
            return None
        bd[bd.index(drop)] = rng.choice(cands)
    return None


def climb(bd, ti, lo, hi, need, cap):
    cur = list(bd)
    c = counts_of(cur)
    sc = score_of(c)
    inb = [False] * N
    for i in cur:
        inb[i] = True
    while True:
        best = None
        for p in range(BOARD):
            i = cur[p]
            c[UO[i]] -= 1; c[UC[i]] -= 1
            for j in range(N):
                if inb[j]:
                    continue
                c[UO[j]] += 1; c[UC[j]] += 1
                if ok(c, ti, lo, hi, need, cap):
                    d = score_of(c) - sc
                    if d > 0 and (best is None or d > best[0]):
                        best = (d, p, j)
                c[UO[j]] -= 1; c[UC[j]] -= 1
            c[UO[i]] += 1; c[UC[i]] += 1
        if best is None:
            return tuple(sorted(cur)), sc
        d, p, j = best
        i = cur[p]
        inb[i] = False; inb[j] = True
        c[UO[i]] -= 1; c[UC[i]] -= 1; c[UO[j]] += 1; c[UC[j]] += 1
        cur[p] = j
        sc += d


def solve(trait, bp, is_origin, restarts=900, seed=5):
    """Boards holding `trait` at exactly `bp`'s tier, with the archetype core live."""
    ti = TI[trait]
    lo, hi = window(trait, bp)
    pool = [i for i in range(N) if UO[i] == ti or UC[i] == ti]
    core = core_of(trait) if is_origin else []
    need = sorted({UC[i] for i in core})
    cap = bp if is_origin else 0          # rule 4 is ORIGIN anchors only, by your call
    if lo > len(pool):
        return None, dict(reason="members", needs=lo, has=len(pool))
    rng = random.Random(seed)
    found, feasible = {}, False
    for _ in range(restarts):
        k = rng.randint(lo, min(hi - 1, len(pool)))
        base = list(core)
        rest = [i for i in pool if i not in base]
        if len(base) + len(rest) < k:
            continue
        bd = base + rng.sample(rest, k - len(base))
        # one partner from outside for each core class, then random fill
        for t in need:
            if sum(1 for i in bd if UC[i] == t) >= 2:
                continue
            cands = [i for i in range(N) if UC[i] == t and i not in bd and UO[i] != ti]
            if cands and len(bd) < BOARD:
                bd.append(rng.choice(cands))
        outside = [i for i in range(N) if i not in bd and UO[i] != ti]
        if len(bd) > BOARD or len(outside) < BOARD - len(bd):
            continue
        bd += rng.sample(outside, BOARD - len(bd))
        bd = repair(bd, ti, lo, hi, need, cap, rng)
        if bd is None:
            continue
        feasible = True
        b2, sc = climb(bd, ti, lo, hi, need, cap)
        found[b2] = sc
    if not feasible:
        return None, dict(reason="core", core=[TRAITS[t] for t in need])
    out, seen = [], set()
    for bd, sc in sorted(found.items(), key=lambda kv: -kv[1]):
        sig = tuple(sorted(live(bd)))
        if sig in seen:
            continue
        seen.add(sig)
        out.append((bd, sc))
    return out, None


def live(bd):
    c = counts_of(bd)
    return [(TRAITS[t], c[t]) for t in range(NT) if c[t] and TIER[t][c[t]]]


def units_of(bd):
    return ", ".join(sorted("%s/%s c%d%s" % (UNITS[i]["o"], UNITS[i]["cls"], UNITS[i]["cost"],
                                             "*" if UNITS[i].get("stub") else "")
                            for i in bd))
