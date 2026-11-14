# -*- coding: utf-8 -*-
"""The SpiderWeb, built to your framework.

One origin at a time, split by breakpoint tier. A board is 9 units. A section for
"Comrade 2" holds boards where Comrade is active at tier 1 exactly - at least its
tier-1 breakpoint and below its tier-2 one - so the sections never overlap.

SCORE: every active trait, origin or class, pays by how many UNITS its live
breakpoint eats - C(b,2), one point per PAIR of members. b = 2..7 pays 1/3/6/10/15/21.

WHY NOT A TIER-INDEXED CURVE (1/3/5/7 and every steepening of it): a tier index is
not what a board spends. Tier 3 costs 5 units in Freeblade and 7 in Bloom, so one
number cannot price both - and under 1/3/5/7 that showed up exactly there, as
Freeblade 5 being the only deep tier in the game that paid for itself. Every board
here trades at roughly 2-3 points per unit, so what a breakpoint must be priced
against is its SIZE. C(b,2) does that and is convex, so each extra member is worth
more than the last, which is the thing "going deeper" is supposed to mean.

METHOD, stated plainly: C(60,9) is 14.6 billion boards, so this is NOT exhaustive.
For each section it enumerates every way to pick the required members of the focus
origin, fills the rest by beam search, then hill-climbs by swapping units until no
single swap improves the score. Results are the best FOUND, not proven optimal.
"""
import itertools
from collections import Counter, defaultdict
import stub_simple as S
import roster25 as R

UNITS = S.ALL
ORI_BP, CLA_BP = S.ORI_BP, S.CLA_BP
BP = dict(ORI_BP); BP.update(CLA_BP)
# b =         0  1  2  3  4   5   6   7   8   9
PAIRS   =    [0, 0, 1, 3, 6, 10, 15, 21, 28, 36]      # C(b,2)
BOARD = 9
BEAM = 400

by_origin = defaultdict(list)
for i, u in enumerate(UNITS):
    by_origin[u["o"]].append(i)


def tier_of(trait, n):
    t = 0
    for i, b in enumerate(BP.get(trait, [])):
        if n >= b:
            t = i + 1
    return t


def score(idx):
    c = Counter()
    for i in idx:
        c[UNITS[i]["o"]] += 1
        c[UNITS[i]["cls"]] += 1
    s, live = 0, []
    for trait, n in c.items():
        t = tier_of(trait, n)
        if t:
            s += PAIRS[BP[trait][t - 1]]          # priced by the breakpoint's SIZE
            live.append((trait, n, t))
    return s, live


def section(origin, tier_i):
    """Boards where `origin` sits at exactly tier_i (1-based)."""
    bps = ORI_BP[origin]
    lo = bps[tier_i - 1]
    hi = bps[tier_i] if tier_i < len(bps) else 99
    members = by_origin[origin]
    if lo > len(members):
        return None, lo                      # unreachable: not enough members exist
    pool = [i for i in range(len(UNITS)) if UNITS[i]["o"] != origin]
    best = []
    for core in itertools.combinations(members, lo):
        # beam search over the remaining slots
        beams = [tuple(core)]
        for _ in range(BOARD - lo):
            nxt = {}
            for b in beams:
                for j in pool:
                    if j in b:
                        continue
                    cand = tuple(sorted(b + (j,)))
                    if cand not in nxt:
                        nxt[cand] = score(cand)[0]
            beams = [k for k, _ in sorted(nxt.items(), key=lambda kv: -kv[1])[:BEAM]]
        for b in beams:
            n_o = sum(1 for i in b if UNITS[i]["o"] == origin)
            if lo <= n_o < hi:
                best.append((score(b)[0], b))
    # hill-climb the top handful
    best.sort(key=lambda x: -x[0])
    polished = {}
    for sc, b in best[:40]:
        cur, curscore = list(b), sc
        improved = True
        while improved:
            improved = False
            for pos in range(len(cur)):
                for j in range(len(UNITS)):
                    if j in cur:
                        continue
                    trial = cur[:pos] + [j] + cur[pos + 1:]
                    n_o = sum(1 for i in trial if UNITS[i]["o"] == origin)
                    if not (lo <= n_o < hi):
                        continue
                    s2 = score(tuple(trial))[0]
                    if s2 > curscore:
                        cur, curscore, improved = trial, s2, True
                        break
                if improved:
                    break
        polished[tuple(sorted(cur))] = curscore
    # two boards with the same live traits are the same comp for our purposes;
    # keep only the first of each signature so "3 best" means 3 DIFFERENT comps
    seen, out = set(), []
    for b, sc in sorted(polished.items(), key=lambda kv: -kv[1]):
        sig = tuple(sorted((x[0], x[2]) for x in fmt(b)))
        if sig in seen:
            continue
        seen.add(sig)
        out.append((b, sc))
    return out, None


def units_of(idx):
    return ", ".join(sorted("%s/%s c%d%s" % (UNITS[i]["o"], UNITS[i]["cls"], UNITS[i]["cost"],
                                             "*" if UNITS[i].get("stub") else "")
                            for i in idx))


def fmt(idx):
    c = Counter()
    for i in idx:
        c[UNITS[i]["o"]] += 1
        c[UNITS[i]["cls"]] += 1
    live = sorted(((t, n, tier_of(t, n)) for t, n in c.items() if tier_of(t, n)),
                  key=lambda x: (-x[2], x[0]))
    return live


if __name__ == "__main__":
    import json
    DUMP = {"roster": len(UNITS), "board": BOARD, "sections": []}
    print("roster %d units, board %d\n" % (len(UNITS), BOARD))
    for o in R.ORIGINS:
        print("=" * 78)
        print("%s   (%d members, breakpoints %s)"
              % (o, len(by_origin[o]), "/".join(map(str, ORI_BP[o]))))
        for t in range(1, len(ORI_BP[o]) + 1):
            res, dead = section(o, t)
            head = "  %s %d  (tier %d, %d pt)" % (o, ORI_BP[o][t - 1], t,
                                     PAIRS[ORI_BP[o][t - 1]])
            if dead:
                print(head + "   UNREACHABLE - needs %d members, origin has %d"
                      % (dead, len(by_origin[o])))
                DUMP["sections"].append(dict(origin=o, tier=t, bp=ORI_BP[o][t - 1],
                                             pts=PAIRS[ORI_BP[o][t - 1]], dead=dead,
                                             members=len(by_origin[o]), comps=[]))
                continue
            print(head)
            comps = []
            for rank, (b, sc) in enumerate(res[:3], 1):
                live = fmt(b)
                print("     %d. %d pts  |  %s" % (
                    rank, sc, ", ".join("%s %d(t%d)" % (t_, n, ti) for t_, n, ti in live)))
                comps.append(dict(score=sc, live=live, units=units_of(b)))
            DUMP["sections"].append(dict(origin=o, tier=t, bp=ORI_BP[o][t - 1],
                                         pts=PAIRS[ORI_BP[o][t - 1]], dead=None,
                                         members=len(by_origin[o]), comps=comps))

    json.dump(DUMP, open("web.json", "w"), indent=1)
    print("wrote web.json")
