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
Blackboard.GetAtk() -> HeroDataRuntime.Stat.Atk -> StatModifier.ModifiedAtk() -> Stat.BaseAtk

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

**DONE** — 2026-08-06. Verified in Play mode against all 5 seeded heroes: every stat matches
its SO, current HP/mana seed correctly, modifiers still apply and expire, and the
DamageReduction path is unchanged. No compile errors, no runtime errors.

- [x] **Delete a layer.** `HeroDataRuntime` holds `Stat` + placement + `isDummy`. Its stat
  forwards earn nothing — expose `Stat` directly and let callers read `runtimeData.Stat.Atk`.
  Same for the blackboard's stat block.
  → `HeroDataRuntime`'s 15 stat forwards deleted, replaced by `public Stat Stat => _stat;`.
  The blackboard keeps its `GetX()` methods (states talk to the blackboard, never past it —
  per the `review-comments.md` decision) but they now read `Stat` in one hop.

- [x] **Make stat lookup data-driven.** `StatModifier` has nine near-identical methods, eight
  of which are literally `return _stat.BaseX;`. Replace with:

  ```csharp
  public float Get(StatType type) => _base[type] + FlatBonus(type);
  ```

  Then adding a stat = one enum member + one Inspector field. This also kills the current
  **circular dependency**: `Stat` owns `StatModifier`, and `StatModifier` holds a back-reference
  to `Stat`. That cycle means neither class can be understood on its own.
  → New `StatType` enum; `Stat` keys base values in one dictionary; the nine `ModifiedX()`
  methods collapse into `StatModifier.Apply(StatType, baseValue)`. The cycle is gone —
  `StatModifier` no longer holds a `Stat`, it takes the base value as an argument.
  Which modifier affects which stat is now one `FlatBonusTarget` dictionary, so "why does my
  buff do nothing?" has exactly one place to look.

- [x] `GetAtk()` and `GetAttackDamage()` are duplicates returning the same value.
  → `GetAtk()` removed (it had no callers); `GetAttackDamage()` kept.

- [ ] Getter style is inconsistent inside one layer — `GetRange()` (method) next to `Team`
  (property). Pick properties; they read cheaper.
  → Left alone for now: converting the `GetX()` methods to properties churns every state file
  for a purely cosmetic gain. Worth doing as its own pass, not bundled into this one.

### Cost after the change

Adding a new stat is now: one `StatType` member + one line in `Stat`'s constructor + one
`HeroDataSO` field, plus a one-line typed property if call sites want `int` instead of `float`.
**Down from 9 edits across 5 files to 3 edits across 3 files**, and none of them require
touching `StatModifier` or `HeroDataRuntime` at all.

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

**DONE** — 2026-08-06. `.Blackboard.` call sites: **59 across 15 files → 47 across 12**.
`SkillEffect.cs` and both UI bars are now entirely off that list. The states still go through
the blackboard on purpose (per the `review-comments.md` decision) — they're the one consumer
it was actually designed for.

Verified with a live combat run: auto-attacks landed, heroes walked and attacked
(so `MovementConfig` threads correctly), and Jhin's mana went 120 → capped → 20 with a Dummy
taking 130 damage, which is the `SkillEffect` → `IDamageable` path proving itself end to end.
No errors.

- [x] Introduce narrow interfaces, implemented by `Hero`:

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
  → Both live in `Hero/HeroInterface.cs`; `Hero` implements them. `SkillEffect.cs` no longer
  mentions `Blackboard`, `Hero`, or `HeroStateType` anywhere.

  Two details worth remembering:
  - `ApplyEffect` takes `IReadOnlyList<IDamageable>`, not `List<IDamageable>`. That interface is
    **covariant**, so every existing caller keeps passing the `List<Hero>` it already had —
    the whole switch cost zero changes in `LegacyAction`, `CircleAOE`, `ZoneAOE`,
    `HomingProjectile`, and `Cast`.
  - `IDamageable` exposes `IsAlive` rather than letting callers write `recipient == null`.
    Once a recipient is typed as an interface, `==` is plain reference equality and Unity's
    fake-null no longer applies, so a `== null` check would silently stop detecting destroyed
    objects. This is the easy way to get a subtle bug out of an otherwise safe refactor.

- [x] Move `_moveSpeed` / `_walkCurve` / `_attackCurve` off the blackboard. They live there only
  so `HeroWalk`/`HeroAttack` can reach them — that's animation config, not shared state. A
  `MovementConfig` struct passed to those two states removes three members and one whole concern.
  → New `Hero/MovementConfig.cs` (readonly struct), built in `Hero.Init` and handed to
  `HeroStateMachine`, which passes it to only the three states that move — `HeroIdle`,
  `HeroWalk`, `HeroAttack`. `HeroDead` and `HeroStunned` never see it. The blackboard lost three
  members and three constructor parameters.

- [x] **Remove `Hero.Blackboard` entirely.** The getter was public, so anything *could* reach
  through it — the finish line for this section is that nothing can.
  → Done 2026-08-06. `.Blackboard.` now appears **zero** times in the codebase (was 59).

  Two halves, per the decisions taken at the time:
  - **States hold the blackboard directly.** `HeroState` takes it in its constructor
    (your own idea from `review-comments.md`). States keep `_me` too, for `transform`,
    `StateMachine.ChangeState` and `Team` — the Unity-facing bits that aren't the blackboard's
    job. `_me.Blackboard.X` became `_blackboard.X`, ~35 sites.
  - **Everyone else goes through interfaces.** Two new ones alongside `IDamageable`:
    `IPlaceable` (CurrentHex, ReservedHex, CurrentPlacement, IsInCombat, SetReservedHex,
    SetCurrentPlacement) and `ITargeter` (FindNearestEnemy, FindFurthestEnemy). `SetBoard`/
    `SetTeam` stayed plain public methods — they're lifecycle wiring, not a role.

  Cross-hero reads were the interesting part: `nearestEnemy.Blackboard.GetCurrentHex()` is now
  `nearestEnemy.CurrentHex`, so one hero asking about another goes through the same narrow
  contract as any outside system.

- [x] **Then delete `HeroStateMachineBlackBoard` outright.** Once the getter was gone, the
  blackboard and `Hero` were visibly two classes doing the same job — both pure glue, both
  forwarding, with no way to answer "how are these different?".

  Its stated reason (in its own docstring) was "we don't want to pass Hero to the states,
  since Hero contains additional unrelated data". The interfaces do that now, and better —
  so the blackboard was a second mechanism for a solved problem, costing one extra forwarding
  hop on every single access.

  Folded into `Hero`; states hold `_me` only. The duplicate accessor pairs collapsed in the
  process — `GetCurrentHP()`/`CurrentHP` and `GetMaxHP()`/`MaxHP` were the same value under
  two names, and are now one each.

  What it reads like at the call site:
  ```csharp
  // before
  _nearestEnemy.Blackboard.TakeDamage(_me.Blackboard.GetAttackDamage());
  // after
  _nearestEnemy.TakeDamage(_me.AttackDamage);
  ```

  **Known trade-off, accepted deliberately:** the blackboard was a plain C# class, `Hero` is a
  MonoBehaviour, so hero runtime state now lives inside a Unity component and can't be
  constructed in a test without a GameObject. `Stat`, `StatModifier`, `HeroDataRuntime`,
  `FindEnemy`, `CombatMath`, `HexPathfinder` and the state machine are all still plain classes,
  so the testable core is largely intact — but `Hero` itself no longer is. Revisit if unit
  tests ever arrive.

- [x] `FindEnemy` reaching back through `_me.Blackboard.Board` for the object that built it.
  → It now holds `BattleBoard` directly, set via `FindEnemy.SetBoard()` from the blackboard's
  own `SetBoard()`. It arrives by setter rather than constructor because the board doesn't
  exist until `Preparation` wires it up, after `Hero.Init()` has already run.

---

## 3. `BlackboardTemp` — the split is easy

Already self-flagged as messy. It holds two unrelated things:

Note: its name is now stale too — the "Blackboard" it was named after no longer exists.
Renaming it is free to do as part of the split below.

**DONE** — 2026-08-06. `BlackboardTemp.cs` deleted. Verified live: compile clean, combat runs,
`PlaySkillCastEffect` spawns its FloatingText (0 → 1), and dead heroes land at
`spriteAlpha = 0.30`, so both halves are exercised from their real call paths. No errors.

- [x] **Sprite alpha + floating text** -> presentation. Wants to be a `HeroVisuals`
  MonoBehaviour on the prefab, where the VFX prefab can be wired in the Inspector instead of
  `Resources.Load` (legacy API, and the `static` cache is shared across all heroes).
  → Split into `HeroVisuals`, initially kept as a **plain class, not a MonoBehaviour** — see the
  prefab note below. **Superseded 2026-08-07:** once the prefabs became variants it became a
  MonoBehaviour after all, and the `static` cache went away with `Resources.Load`.
- [x] **`SkillTrigger`** -> *not* temporary. It's the hero's per-cast skill progress, permanent
  runtime state. Belongs in a `HeroSkillRuntime` alongside the `SkillSO`.
  → `HeroSkillRuntime` owns the `SkillSO` too, which removed an oddity: callers used to hand a
  hero its own skill back to ask it to cast. `TriggerSkill(skill, step, capped)` is now
  `TriggerSkill(step, capped)`.

`Hero` forwards `SetDeadVisual()` / `PlaySkillCastEffect()` / `TriggerSkill()` flat rather than
exposing `_visuals` / `_skillRuntime`, per the `review-comments.md` dislike of
`_me.SubObject.Method()`. Knock-on: `HeroState._skill` became dead (only `HeroAttack` read it)
and was removed.

### Why `HeroVisuals` was not a MonoBehaviour — and now is

The blocker was that **every hero prefab was standalone** — none was a variant of `BaseHero`. So
the Inspector-wired version meant adding and wiring the component once per hero, where a missed
one only surfaces as a null ref the first time that hero dies.

**DONE** — 2026-08-07. Both halves, in order: the prefabs became variants, then the component
moved onto `BaseHero` once.

- [x] **The real fix is upstream: make the hero prefabs variants of `BaseHero`.** Then this
  change — and every future shared-component change — is one edit instead of one per hero.
  → Done for **all 24** (the review said 19; the actual count including `Have Skill/` and
  `Dummy` is 24). Each prefab went from ~938 lines of duplicated YAML to a ~67-line
  `PrefabInstance` carrying only its real overrides.

  How it was done, since it is not a supported Editor operation: instantiate `BaseHero`, apply
  the hero's overrides, `PrefabUtility.SaveAsPrefabAsset` **over the existing path**.
  Overwriting the path keeps the `.meta`, so the **asset GUID is preserved** — no `.meta` file
  changed. Three things are worth knowing if this is ever repeated:

  - **Only three values actually differed per hero**: root `m_Name`, `SpriteRenderer.m_Color`,
    and root `m_LocalPosition`. Collider, rigidbody, canvas, both bars, both curves, sprite,
    material and scale were byte-identical across all 24 — which is what made the conversion
    safe. A structural gate (component set + full child hierarchy must match `BaseHero`) ran
    per prefab before touching it; all 24 passed.
  - **The root object's local fileID does change**, even though the GUID doesn't. Every
    `HeroDataSO._prefab` pointed at the old root and had to be re-pointed. That was the whole
    external reference surface — mapped up front, it was exactly one `.asset` per prefab.
  - **Re-pointing inside `AssetDatabase.StartAssetEditing()` silently writes null.** The first
    pass reported success and left all 24 `_prefab` fields empty. The reference has to be
    assigned in a second pass, after `StopAssetEditing`/`Refresh`, loading the variant back
    off disk. Verify by force-reimporting and re-resolving, not by reading the objects you
    just assigned — the in-session cache will happily hide a broken on-disk reference.

  Verified live: all 25 `HeroDataSO`s resolve their prefab after a forced reimport, and a seeded
  combat run had every hero spawn with correct stats, colour and hex, walk, attack, take damage
  and gain mana, with no errors.

- [x] **Then `HeroVisuals` became a MonoBehaviour**, added and wired **once** on `BaseHero` —
  and all 24 variants inherited both the component and the wiring, confirmed by loading each
  back off disk. That is the payoff, demonstrated on the first change that needed it.
  - `_skillCastTextPrefab` is typed `FloatingText`, not `GameObject`, so a prefab without the
    component can't be wired into it and the `GetComponent` at spawn time is gone.
  - `Resources.Load` is gone, and with it the `static` prefab cache. Since that was the last
    `Resources.` call in the project, `SkillCastText.prefab` moved to `Assets/Prefabs/VFX/`
    (GUID preserved via `AssetDatabase.MoveAsset`) and the empty `Assets/Resources/` tree was
    deleted — a `Resources` folder force-includes its contents in every build.
  - Verified live: `PlaySkillCastEffect` spawns a `FloatingText` reading "Skill Activated!" in
    the blue-team colour, `SetDeadVisual` drops sprite alpha 1 → 0.30, and in real combat Jhin's
    mana went 140 → capped → 30 with the target taking skill damage.

---

- [x] **`Hero.StateMachine` was the last member breaking the flat-forward pattern.** Everything
  else on `Hero` forwards a method; this one handed out the whole object, so states wrote
  `_me.StateMachine.ChangeState(...)` — the `_me.SubObject.Method()` shape removed everywhere
  else. Replaced with `Hero.ChangeState(HeroStateType)` across 12 call sites, and the getter is
  gone: nothing outside the states ever used the machine, so `_stateMachine` is now fully private.

### Follow-up: which interfaces actually earn their keep

Counted 2026-08-06, after all of the above. Usages **outside** `Hero`'s own declaration:

| Interface | Usages | Verdict |
|---|---|---|
| `IDamageable` | 11 | Real. `SkillEffect` takes `IReadOnlyList<IDamageable>` and never mentions `Hero`. |
| `IPlaceable` | 9 | Real, as of this pass — see below. |
| `IHeroStats` | 0 | Compile-checked grouping only. |
| `ITargeter` | 0 | Compile-checked grouping only. |

The pattern is clear: **an interface pays off when the consumer is a genuinely separate
subsystem.** `SkillEffect` operates on anything damageable; `Hex`/`BenchSlot` only ever move a
thing onto themselves. Neither needs to know what a Hero is.

`IHeroStats`/`ITargeter` have no such consumer — the states are the hero's *own behaviour*, not
an external system, and they need `transform`, `StateMachine` and `IsStunned` which no
interface covers. Restricting a class's view of the object it is part of isn't the same problem.

- [x] **Make `IPlaceable` real.** `Placement.OnHeroPlaced/OnHeroUnplaced` and
  `PlacementExtensions.EnterPlacementExtension` now take `IPlaceable`. **`Hex.cs`,
  `BenchSlot.cs` and `Placement.cs` contain zero references to `Hero`.**
  `IPlaceable` gained `Transform transform { get; }`, satisfied implicitly by MonoBehaviour's
  inherited member — the same trick `Placement` already used.

  The one blocker was `Hex` calling `_board.TrackThisHero(hero)`, which needs a real `Hero`.
  That call was misplaced anyway — a hex shouldn't own the board's roster — so it moved to
  `Preparation`, which already knows it's moving a Hero. It keys off `hero.IsInCombat`
  ("my placement is a Hex"), so no type test is needed.

- [ ] **Don't add another interface** unless there's a second implementer or a genuinely
  separate subsystem. That's the test `IDamageable` and `IPlaceable` passed and the other two
  didn't.

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

- [x] **`Hero/States/HeroAttack.cs:60-64` — dead code, skill cast VFX never plays.**
  ```csharp
  bool success = false;
  if (_currentStep != null) _me.Blackboard.Temp.TriggerSkill(...);  // return value dropped
  if (success) _me.Blackboard.Temp.PlaySkillCastEffect(...);        // always false
  ```
  Should be `success = _me.Blackboard.Temp.TriggerSkill(...)`.
  → Fixed 2026-08-06 as part of the section 3 split, since these were the exact lines being
  rewritten and carrying dead code into a fresh file would have been worse. **This is a visible
  gameplay change:** the floating "Skill Activated!" text now actually appears on cast. Verified
  spawning live. The `_currentStep != null` guard moved inside `HeroSkillRuntime.TriggerSkill`.

- [x] **Max-HP modifiers are unreachable.** `Stat.SetCurrentHP` clamps to `BaseHP`, but
  `Blackboard.GetMaxHP()` returns the *modified* HP (`ModifiedHP()` = base + Heal modifiers).
  With a bonus-HP modifier active, current HP can never reach max, and the health bar can never
  show full.
  → Fixed while rewriting `Stat` — `SetCurrentHP` now clamps to the modified `HP`.

  **Still open, and needs a design call:** the only `ModifierEnum` that raises max HP is
  `Heal`, which is almost certainly wrong — healing shouldn't raise the ceiling, and `BonusHP`
  (which should) was a silent no-op. The refactor preserved this exactly rather than changing
  gameplay behaviour on its own; see `FlatBonusTarget` in `StatModifier.cs`. Swapping `Heal`
  for `BonusHP` there is a one-line change once you decide.

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

- [x] **`Hero.Start()`/`Hero.Update()` throw on a scene-placed Hero that was never spawned.**
  Found 2026-08-06 when a `BaseHero` object in the scene started spamming
  `NullReferenceException` every frame once combat began — `_runtimeData`/`_stateMachine` are
  only set by `Init()`, which `Preparation` calls for spawned heroes only.
  Pre-existing, not introduced by any refactor: the old code did `_blackboard.IsInCombat()`,
  which threw identically. It only surfaced once a `Hero` was left sitting in the scene.
  → Both now open with `if (!IsInitialized) return;`, matching what `Healthbar`/`Manabar`
  already did.

- [ ] **Stale comment:** `HeroDead.cs:1` says "Entered from `Blackboard.TakeDamage()`" — it's
  actually polled by each state's `CheckSwitchState`.

- [ ] **Stale docs in `CLAUDE.md`:** it says `Hex` tracks `Occupant` (it doesn't anymore) and
  references `BattleBoard._heroPlacement` (moved to `BattlePlacementSO`).

---

## Suggested order

Ranked by payoff ÷ risk. 1–4 are mechanical and safe. 5–6 change the actual shape of the
codebase — worth doing **before** combat and traits add more callers, because every new system
built against the current `Blackboard` makes it more expensive to split later.

1. [x] Fix the `success` bug (2 min — it's a live feature that silently doesn't work)
   — done alongside section 3
2. [ ] Hoist Dead/Stunned interrupts into `HeroStateMachine` (removes 4x duplication)
3. [ ] Rename `LegacyAction` -> `SkillAction`, flatten folders (pure readability)
4. [x] Split `BlackboardTemp` into `HeroVisuals` + `HeroSkillRuntime`
   — **done third**, see section 3
5. [x] Introduce `IDamageable`, cut the skill system loose from `Blackboard`
   — **done second**, see section 2
6. [x] Collapse `Stat` / `StatModifier` / `HeroDataRuntime` into one data-driven stat block
   — **done first**, see section 1
7. [ ] Move the reservation index onto `BattleBoard`
8. [ ] Namespaces + asmdefs
