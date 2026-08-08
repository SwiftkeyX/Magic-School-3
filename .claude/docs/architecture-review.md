# Architecture Review — `Assets/Scripts/`

Full read-through of all 52 scripts on 2026-08-05, focused on readability / editability /
coupling. **All sections were completed 2026-08-06 → 08-07** (see git history for the diffs).
This file has been trimmed to what's still open, plus the traps worth re-reading before
touching the same ground again.

---

## What the pass changed

Deleted a layer (`HeroStateMachineBlackBoard`), cut the skill system loose from `Hero`
internals via `IDamageable`/`IPlaceable`, collapsed the `Stat`/`StatModifier`/`HeroDataRuntime`
forwarding into one data-driven block (`StatType`), hoisted the Dead/Stunned interrupts into
`HeroStateMachine.Tick`, renamed `LegacyAction` → `TemplateAction` and flattened five folder
levels to one, moved hex reservation into an index on `BattleBoard`, made every hero prefab a
variant of `BaseHero`, and put everything in `namespace MagicSchool`.

Three real bugs died with it: the skill-cast VFX that never played (dropped return value),
unreachable max-HP modifiers, and a dead hero getting one free attack after `ChangeState(Dead)`.

`Hero` stopped handing out its collaborators — `Blackboard`, `StateMachine` and `Board` were all
public and are all private or gone.

**What it didn't touch:** the centre of gravity. At class level `Hero` still has 28 classes
depending on it and 12 it depends on, with eleven of its own parts holding a back-pointer to it
(`Hero.Init` builds each collaborator and hands it `this`). Every refactor narrowed the *edges*;
none changed the *shape*. That's a design decision, not a cleanup, which is why it isn't a
checklist item.

---

## Still open

- [ ] **Mana is consumed before the cast is validated.** `Stat.AddMana` zeroes mana the instant
  it caps, but `TemplateAction.Spawn` can bail with no valid target and spawn nothing — mana
  spent, no skill. Tagged `FIXLATER` at `Skill/SkillActionGroup/TemplateAction.cs:31`.

- [ ] **`Board/HexPathfinder.cs:51` can throw.** `while (cameFrom[step] != startHex)` assumes
  `startHex` isn't itself a goal hex — but the mover's own hex is never blocked to itself, so it
  can qualify as a goal → `KeyNotFoundException`. Currently masked because `HeroIdle` checks
  attack range first. Fragile; wants a one-line guard.

- [ ] **Max-HP bonus is wired to the wrong modifier.** The only `ModifierEnum` that raises max HP
  is `Heal`, which is almost certainly wrong — healing shouldn't raise the ceiling, and `BonusHP`
  (which should) is a silent no-op. Preserved as-is by the refactor rather than changing gameplay
  unasked. One line in `FlatBonusTarget` (`StatModifier.cs`) once you decide.

- [ ] **`HitboxSize` / `HitboxShape` are dead.** Serialized into every `SkillActionGroup` but read
  by no action — size actually comes from the prefab's collider. Removing them is a data change
  across the five `SkillSO` assets, not just a code deletion.

- [ ] **`Assets/Prefabs/Skill/PiercingProjectile.prefab` carries a `HomingProjectile` component**
  (it references that script's GUID). `PiercingProjectile.cs` is a **0-byte file** referenced by
  nothing, and `Jhin.asset` points at the prefab. Implementing piercing for real means fixing the
  prefab's component, not just filling in the file.

- [ ] **Stale `CLAUDE.md`**, five things across three lines:
  - `Hex` "tracks `Occupant`" — it doesn't; reservations live in `BattleBoard._reservedBy`.
  - `Hex` "needs `SetBoard()` called" — that method is `Init(board, hexNumber)`.
  - `HexPlacement.cs` doesn't exist; `Team`/`GamePhase` are in `Core/Enum.cs`, the structs in
    `Core/Struct.cs`.
  - `HexPlacement` is now `HexNumber` — it's a coordinate, not a placement.
  - `BattleBoard._heroPlacement` is `BattlePlacementSO.HeroesPlacement` (mentioned twice).

  Missing rather than wrong: every script is now in `namespace MagicSchool`.

- [ ] **`Preparation` is named after a phase but has no phase logic** — it's a spawner and a mover
  bundled under a phase's name, with different callers for each. Tagged `ASKING` in
  `Preparation.cs:8`.

- [ ] **asmdefs — blocked, and not on effort.** Measured from compiled IL,
  `{ <root>, Board, Core, Hero, Player, Skill }` form one strongly connected component, and Unity
  hard-errors on circular assembly references. No ordering of asmdefs works; breaking the cycle
  first is the `Hero` design question above. Payoff check before anyone tries: at ~56 files the
  compile-time win is negligible, so what's left is *enforced* dependencies — the reward for
  breaking the cycles, not a way to break them.

  Two corrections to how this was first analysed: **folder ≠ module** (that partitioning came from
  the review item, not the code), and **grep can't measure dependencies** — it counts types named
  in comments and enum members that shadow class names (`GamePhase.Preparation` read as the
  `Preparation` class), which gave three different cycle counts in three passes.

---

## Guidelines that came out of it

- **Don't add another interface** unless there's a second implementer or a genuinely separate
  subsystem. `IDamageable` (11 external usages) and `IPlaceable` (9) passed that test;
  `IHeroStats` and `ITargeter` have **zero** and are compile-checked grouping only. The states are
  the hero's *own* behaviour, not an external system — restricting a class's view of the object it
  is part of isn't the same problem.
- **No `_me.SubObject.Method()`.** `Hero` forwards flat (`SetDeadVisual`, `TriggerSkill`,
  `ChangeState`, `WhoReservedThisHex`) rather than exposing `_visuals`, `_skillRuntime`,
  `_stateMachine` or `_board`.
- **`CheckSwitchState()` is a state's OWN transitions only.** Machine-level interrupts (Dead,
  Stunned) resolve in `HeroStateMachine.Tick` before `OnUpdate` runs. Dead is terminal; already-
  `Stunned` is excluded from the stun check so `HeroStunned.OnUpdate` can notice it expiring.
- **Accepted trade-off:** folding the blackboard into `Hero` put hero runtime state inside a
  MonoBehaviour, so it can't be constructed in a test without a GameObject. `Stat`,
  `StatModifier`, `HeroDataRuntime`, `FindEnemy`, `CombatMath`, `HexPathfinder` and the state
  machine are all still plain classes. Revisit if unit tests ever arrive.

---

## Traps worth remembering

Each of these was hit for real during the refactors, and each fails **silently**.

- **`[SerializeReference]` stores the type's namespace *and* assembly** as part of its identity
  (`type: {class: AttackSkillEffect, ns: , asm: Assembly-CSharp}`). Changing either without
  migrating the records resolves all of them to **null with no compile error and no warning** —
  skills keep firing and do nothing. Applies to asmdefs too, since they change the assembly name.
- **Renaming a serialized field blanks the data.** Add `[FormerlySerializedAs]`, re-save every
  asset so the new key is written to disk, verify, *then* remove the attribute.
- **Moving a `.cs` without its `.cs.meta`** changes the script GUID and turns every prefab
  referencing it into "missing script". `git mv` both together.
- **Prefab-variant conversion:** overwrite the existing path with `SaveAsPrefabAsset` so the
  `.meta` and asset GUID survive — but the **root object's local fileID still changes**, so every
  `HeroDataSO._prefab` had to be re-pointed. **Re-pointing inside
  `AssetDatabase.StartAssetEditing()` silently writes null**; assign in a second pass after
  `StopAssetEditing`/`Refresh`, and verify by force-reimporting rather than by reading the objects
  you just assigned.
- **`== null` stops working behind an interface.** Once a recipient is typed as `IDamageable`,
  `==` is plain reference equality and Unity's fake-null no longer applies, so a destroyed object
  reads as alive. Hence `IDamageable.IsAlive`. Conversely, in `BattleBoard`'s reservation index
  `hero == null` **must** be checked before `hero.StateType` — that's the fake-null catching a
  destroyed GameObject still sitting in the dictionary.
- **A `Resources/` folder force-includes its contents in every build.** The last `Resources.Load`
  is gone; keep it that way (`SkillCastText.prefab` lives in `Assets/Prefabs/VFX/`).
- **Dead heroes are filtered on READ, not cleared on death.** `WhoReservedThisHex` returns null
  for a corpse, so nothing has to hook the `Dead` transition — one fewer place to keep in sync.
- **Unity 6000.4.9f1 is C# 9** — file-scoped namespaces need C# 10 (CS8773).
