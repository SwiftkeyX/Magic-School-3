# Architecture

Scripts live in `Assets/Scripts/`, split into assemblies so the dependency direction is
enforced by the compiler rather than by convention. **One `.asmdef` is one module**, and the
module is the unit of reuse: everything inside one is deeply coupled on purpose, so lifting a
piece out means taking the whole assembly with it.

Namespaces subdivide further inside a module, but they are naming, not boundaries. The
assembly is the line the compiler actually holds.

## Who depends on whom

Arrows point at what a module is allowed to reference. Nothing here is inherited or implied —
this is what the `.asmdef` files say.

```
MagicSchool.Contracts    -> (nothing)
MagicSchool.Engine       -> (nothing)
MagicSchool.VFX          -> Unity.TextMeshPro
MagicSchool.StatScaling  -> Contracts
MagicSchool.Modifiers    -> Contracts
MagicSchool.Skills       -> Contracts, Engine, StatScaling, Modifiers
MagicSchool.Combat       -> Contracts, Engine, Skills, VFX, Modifiers
MagicSchool.UI           -> Contracts, Combat, UnityEngine.UI
MagicSchool.Core         -> Contracts, Engine, Skills, Combat
MagicSchool.Player       -> Contracts, Combat, Core, Unity.InputSystem
MagicSchool.Editor       -> Core                       Editor-only, sits in Core/Editor/
MagicSchool.Combat.Tests -> Combat, Contracts, Skills  EditMode tests, in Assets/Tests/
```

Each module also carries a `*Module.md` next to its `.asmdef` stating its own boundary.

## The modules

**Contracts** — the interfaces and enums every other module talks through. The most
significant are:

- `ICombatant` — a unit that can fight on a board.
- `IEffectable` — a unit that can be affected by an attack or a skill.
- `IPlaceable` — a unit that can be placed on an `IPlacement`.

It references nothing, and has to stay that way. Anything it reaches for gets dragged
underneath every other module.

**Engine** — services that decouple the rest of the game from the Unity engine.

**VFX** — floating combat text.

**StatScaling** — how stat ratios and scaling are computed.

**Modifiers** — buffs, debuffs and statuses as data, plus the resolver that turns them into a
final stat and expires them on time. Both halves live here so the module stands alone:
anything that grants "+20 armour" needs only this module, not the combat system as well.

**Skills** — a hero's skill: a list of steps, each step playing a template action (projectile,
AoE, hitbox). Effects apply through `IEffectable`, so this never knows the `Hero` type.

**Combat** — two smaller pieces that are deeply coupled, Hero and Placement, in one assembly.
A hero needs its hex and a hex tracks its occupant, so the coupling is real and the module
admits it rather than pretending otherwise. Holds the hex grid, A\* pathfinding, hex
reservation, and the hero state machine.

**UI** — overlay panels (inspector, shop) in UI Toolkit, and world-space UI (health bar, mana
bar, skill bar) in uGUI.

**Core** — the composition root. Owns the game state machine — preparation, combat, result —
and the stage progression between them, and wires the other modules together. It is the only
module that knows about all of them.

**Player** — processes player input: dragging heroes, right-click to inspect, and the keys
that start and restart a fight.

**Editor** — inspector tooling for Core.

**Combat.Tests** — EditMode tests for Combat, Contracts and Skills.
