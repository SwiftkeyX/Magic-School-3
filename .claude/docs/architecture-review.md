# Architecture Review — `Assets/Scripts/`

Full read-through of all 52 scripts (~3,300 lines) on 2026-08-05, focused on
**readability / editability / coupling** rather than bug-hunting (though bugs found along the
way are listed at the bottom).

Line numbers are a snapshot at review time — use file + quoted content to relocate after
refactors move things. Check items off as they're addressed.

---

## Verdict

The bones are good. The state machine, `HexPathfinder`, `Hitbox`, `Placement`, and the
SO-driven skill data are all sound designs, and the inline comments are genuinely excellent.

The problem isn't the design, it's that **one pattern — "delegate everything through a glue
layer" — got applied uniformly instead of selectively**. That pattern is correct in a couple
of places and pure cost everywhere else, and it's now the dominant tax in the codebase.

---

## 1. The pass-through chain — biggest editability tax

Reading a hero's attack stat today:

```
Blackboard.GetAtk() -> HeroDataRuntime.Atk -> Stat.Atk -> StatModifier.ModifiedAtk() -> Stat.BaseAtk
```

Five hops, four files, and **three of them add zero logic**. `HeroDataRuntime`'s stat section
is 13 one-line forwards to `Stat`. `HeroStateMachineBlackBoard`'s stat section is 10 more
forwards to `HeroDataRuntime`.

Cost of adding ONE new stat (e.g. crit chance):

| File | Edits |
|---|---|
| `HeroDataSO` | field + getter |
| `Stat` | field + `BaseCrit` + `Crit` + ctor line |
| `StatModifier` | `ModifiedCrit()` |
| `HeroDataRuntime` | getter |
| `HeroStateMachineBlackBoard` | `GetCrit()` |

**5 files, 9 edits, for one number.** That is the editability problem stated as a number.

### Fixes

- [ ] **Delete a layer.** `HeroDataRuntime` holds `Stat` + placement + `isDummy`. Its stat
  forwards earn nothing — expose `Stat` directly and let callers read `runtimeData.Stat.Atk`.
  Same for the blackboard's stat block.

- [ ] **Make stat lookup data-driven.** `StatModifier` has nine near-identical methods, eight
  of which are literally `return _stat.BaseX;`. Replace with:

  ```csharp
  public float Get(StatType type) => _base[type] + FlatBonus(type);
  ```

  Then adding a stat = one enum member + one Inspector field. This also kills the current
  **circular dependency**: `Stat` owns `StatModifier`, and `StatModifier` holds a back-reference
  to `Stat`. That cycle means neither class can be understood on its own.

- [ ] `GetAtk()` and `GetAttackDamage()` are duplicates returning the same value.

- [ ] Getter style is inconsistent inside one layer — `GetRange()` (method) next to `Team`
  (property). Pick properties; they read cheaper.

---

## 2. `Blackboard` isn't a blackboard, it's the Hero API

The docstring says it exists so states don't need `Hero`. But `Hero.Blackboard` is public, so
**59 call sites across 15 files** go through it — states, skill effects, UI bars, `Hex`,
`Preparation`, `FindEnemy`, `BattleBoard`.

The coupling wasn't removed, it was *centralized*. One class now exposes board access, team,
movement curves, animation curves, placement, all stats, damage, healing, mana, modifiers, and
a grab-bag — so any change to it can break the UI, the skill system, and pathfinding at once.

Two tells that it's leaking:

- `FindEnemy` is constructed by the blackboard but calls `_me.Blackboard.Board` — reaching back
  out through `Hero` to get to the object that built it. Pass `BattleBoard` in directly.
- `SkillEffect.ApplyEffect` calls `recipient.Blackboard.TakeDamage(...)`. The skill system needs
  *four* methods but sees all forty.

### Fix — split by consumer, not by "who needs to share"

- [ ] Introduce narrow interfaces, implemented by `Hero`:

  ```csharp
  public interface IDamageable       // what SkillEffect needs
  {
      void TakeDamage(int amount);
      void Heal(float amount);
      void AddModifier(Modifier m);
      bool IsDead { get; }
  }

  public interface IStatReadout      // what Healthbar / Manabar need
  {
      int CurrentHP { get; } int MaxHP { get; }
      int CurrentMana { get; } int MaxMana { get; }
  }
  ```

  `SkillEffect` then takes `List<IDamageable>` and stops knowing that heroes have blackboards,
  state machines, or hexes at all. This one change decouples the entire skill system from hero
  internals.

- [ ] Move `_moveSpeed` / `_walkCurve` / `_attackCurve` off the blackboard. They live there only
  so `HeroWalk`/`HeroAttack` can reach them — that's animation config, not shared state. A
  `MovementConfig` struct passed to those two states removes three members and one whole concern.

---

## 3. `BlackboardTemp` — the split is easy

Already self-flagged as messy. It holds two unrelated things:

- [ ] **Sprite alpha + floating text** -> presentation. Wants to be a `HeroVisuals`
  MonoBehaviour on the prefab, where the VFX prefab can be wired in the Inspector instead of
  `Resources.Load` (legacy API, and the `static` cache is shared across all heroes).
- [ ] **`SkillTrigger`** -> *not* temporary. It's the hero's per-cast skill progress, permanent
  runtime state. Belongs in a `HeroSkillRuntime` alongside the `SkillSO`.

Splitting these is ~20 minutes and deletes the file currently labeled "code architecture begin
to be really messy."

---

## 4. `CheckSwitchState` — the same 10 lines, four times

`HeroIdle`, `HeroWalk`, `HeroAttack`, `HeroStunned` each open with an identical HP-check ->
`Dead` and stun-check -> `Stunned`. Add one global interrupt (airborne, silence, knockup) and
every state needs editing, forever.

These are **machine-level interrupts, not state transitions**.

- [ ] Hoist them into `HeroStateMachine.Tick()`:

  ```csharp
  public void Tick()
  {
      if (_interrupts.TryResolve(out HeroStateType forced)) { ChangeState(forced); return; }
      Current?.OnUpdate();
  }
  ```

  Each state then contains only its own logic — `HeroWalk` drops to "lerp, then go idle when
  done," which is all it actually is.

---

## 5. `LegacyAction` — rename it and flatten the folders

- [ ] `Skill/SkillActionGroup/LegacyAction/LegacyActionCategory/LegacyActionChild/` is **five
  folder levels for seven files**. Flatten to `Skill/Actions/`.

- [ ] "Legacy" universally reads as *deprecated* to any C# reader (including future you). It
  isn't — it's the spawned physical thing a skill produces. Rename to `SkillAction`.

- [ ] `LegacyAction.TriggerSkill` is an instance method called *on the prefab*, which then
  instantiates a copy of itself. The object is simultaneously factory and product — which is why
  it needed a four-line comment to explain. A static `SkillActionSpawner.Spawn(prefab, ...)`
  makes the dual role disappear.

- [ ] `ApplyEffectToRecipients` — the `SameToAimTarget` and `EnemiesInArea` branches have
  identical bodies.

---

## 6. Occupancy is scanned, not indexed

`HeroIdle.ReservedHexes()` allocates a fresh `HashSet` by scanning **every hero on the board,
every frame, per hero** — O(n²) allocations per frame. `WorthWaitingForBlocker` does a second
full scan inside a neighbor loop.

Beyond GC cost this is a coupling problem: hex occupancy is stored on heroes but queried by
heroes about *other* heroes, so `HeroIdle` must know the whole roster to answer "is this hex
free?"

- [ ] Give `BattleBoard` a `Dictionary<Hex, Hero>` reservation index, updated on
  `SetReservedHex`. Then it's `board.IsReserved(hex)` and `HeroIdle` stops iterating the roster
  entirely.

---

## 7. No namespaces, no asmdefs

Zero of both, project-wide.

At 52 files this is fine; at 150 it isn't. Every class name is globally reserved — and there are
already generic ones like `Stat`, `Hitbox`, `Cast`, `Circle`, `Box`, `Placement` that will
collide with any asset-store package imported later.

- [ ] Add `namespace MagicSchool.Hero { }` etc. Doing it now is a find-and-replace; later it's a
  merge conflict.
- [ ] Add `.asmdef` per top-level folder — also cuts iteration compile time noticeably.

---

## Bugs found while reading

Not the point of the review, but real:

- [ ] **`Hero/States/HeroAttack.cs:60-64` — dead code, skill cast VFX never plays.**
  ```csharp
  bool success = false;
  if (_currentStep != null) _me.Blackboard.Temp.TriggerSkill(...);  // return value dropped
  if (success) _me.Blackboard.Temp.PlaySkillCastEffect(...);        // always false
  ```
  Should be `success = _me.Blackboard.Temp.TriggerSkill(...)`.

- [ ] **Max-HP modifiers are unreachable.** `Stat.SetCurrentHP` clamps to `BaseHP`, but
  `Blackboard.GetMaxHP()` returns the *modified* HP (`ModifiedHP()` = base + Heal modifiers).
  With a bonus-HP modifier active, current HP can never reach max, and the health bar can never
  show full.

- [ ] **Mana is consumed before the cast is validated.** `Stat.AddMana` zeroes mana the instant
  it caps, but `LegacyAction.TriggerSkill` can bail out with no valid target and spawn nothing —
  mana spent, no skill. Already TODO'd at `LegacyAction.cs:32-35`.

- [ ] **`Board/HexPathfinder.cs:55` can throw.** `while (cameFrom[step] != startHex)` assumes
  `startHex` is not itself a goal hex, but `ReservedHexes()` excludes the mover, so its own hex
  is unreserved and can qualify as a goal -> `KeyNotFoundException`. Currently masked because
  `HeroIdle` checks attack range first. Fragile; wants a one-line guard.

- [ ] **Dead code:** `LegacyActionEnum` is declared and never referenced anywhere.
  `HitboxSize` / `HitboxShape` are serialized into every `SkillActionGroup` but never read by any
  action (size actually comes from the prefab's collider).

- [ ] **Stale comment:** `HeroDead.cs:1` says "Entered from `Blackboard.TakeDamage()`" — it's
  actually polled by each state's `CheckSwitchState`.

- [ ] **Stale docs in `CLAUDE.md`:** it says `Hex` tracks `Occupant` (it doesn't anymore) and
  references `BattleBoard._heroPlacement` (moved to `BattlePlacementSO`).

---

## Suggested order

Ranked by payoff ÷ risk. 1–4 are mechanical and safe. 5–6 change the actual shape of the
codebase — worth doing **before** combat and traits add more callers, because every new system
built against the current `Blackboard` makes it more expensive to split later.

1. [ ] Fix the `success` bug (2 min — it's a live feature that silently doesn't work)
2. [ ] Hoist Dead/Stunned interrupts into `HeroStateMachine` (removes 4x duplication)
3. [ ] Rename `LegacyAction` -> `SkillAction`, flatten folders (pure readability)
4. [ ] Split `BlackboardTemp` into `HeroVisuals` + `HeroSkillRuntime`
5. [ ] Introduce `IDamageable`, cut the skill system loose from `Blackboard`
6. [ ] Collapse `Stat` / `StatModifier` / `HeroDataRuntime` into one data-driven stat block
7. [ ] Move the reservation index onto `BattleBoard`
8. [ ] Namespaces + asmdefs
