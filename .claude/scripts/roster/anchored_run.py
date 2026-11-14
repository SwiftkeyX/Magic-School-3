# -*- coding: utf-8 -*-
"""Solve every declared anchor and write anchored.json."""
import json
import anchored as A

DUMP = {"board": A.BOARD, "roster": A.N, "sections": []}
for kind, anchors in (("origin", A.ORIGIN_ANCHORS), ("class", A.CLASS_ANCHORS)):
    print("=" * 78); print(kind.upper(), "ANCHORS")
    for trait, bp in anchors:
        is_o = kind == "origin"
        core = [A.TRAITS[t] for t in sorted({A.UC[i] for i in (A.core_of(trait) if is_o else [])})]
        res, bad = A.solve(trait, bp, is_o)
        rec = dict(kind=kind, trait=trait, bp=bp, core=core, free=A.BOARD - bp, comps=[], bad=bad)
        head = "%s %d" % (trait, bp)
        if bad:
            print("  %-14s IMPOSSIBLE (%s)" % (head, bad))
        else:
            floor = A.FLOORS.get(bp) if is_o else None
            best = res[0][1] if res else 0
            mark = "" if floor is None else ("ok" if best >= floor else "MISS %d<%d" % (best, floor))
            rec["floor"] = floor
            print("  %-14s core %-24s %s" % (head, "+".join("%s 2" % c for c in core) or "(none)", mark))
            for i, (bd, sc) in enumerate(res[:3], 1):
                lv = sorted(A.live(bd), key=lambda x: -x[1])
                print("     %d. %2d pts  %s" % (i, sc, "  ".join("%s %d" % t for t in lv)))
                rec["comps"].append(dict(score=sc, live=lv, units=A.units_of(bd)))
        DUMP["sections"].append(rec)
json.dump(DUMP, open("anchored.json", "w"), indent=1)
print("\nwrote anchored.json")
