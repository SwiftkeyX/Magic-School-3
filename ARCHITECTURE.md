# Architecture

Scripts live in `Assets/Scripts/`, split into assemblies so the dependency direction is
enforced by the compiler rather than by convention. **One `.asmdef` is one module**, and the
module is the unit of reuse: everything inside one is deeply coupled on purpose, so reusing
something means taking the entire module.

Namespaces subdivide further inside a module, but they are naming, not boundaries.

## Who depends on whom

Arrows point at what a module is allowed to reference.

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
MagicSchool.Editor       -> Core
MagicSchool.Combat.Tests -> Combat, Contracts, Skills
```

Each module also carries a `*Module.md` next to its `.asmdef` stating its own boundary.

Two of them do not get a top-level folder of their own: `MagicSchool.Editor` is Editor-only and
lives in `Core/Editor/`, and `MagicSchool.Combat.Tests` holds the EditMode tests and lives in
`Assets/Tests/`.

## The modules

**Contracts** — the interfaces and enums every other module talks through. The most
significant are:

- `ICombatant` — a unit that can fight on a board.
- `IEffectable` — a unit that can be affected by an attack or a skill.
- `IPlaceable` — a unit that can be placed on an `IPlacement`.

It's a leaf module, and must never reference another module, because that would easily lead to
a cyclic dependency.

**Core**

- controls the game state machine, e.g. preparation, combat, result.
- controls stage progression between them, and wires the other modules together.

**Player** — processes player input: dragging heroes, right-click to inspect, and start or restart a fight.

**Combat**

- two smaller modules that are deeply coupled, Hero and Placement, Hero need to ask Placement for other Hero whereabout,
and Placement need to know which Hero standing on it.
- controls hero behaviour through a state machine.
- constructs a graph-like hex grid that heroes fight on, using A\* to path across it.

**Skills**

- a hero's skill: small steps combined into one big skill.
- built from a list of steps played in order, each step playing a template action
  (projectile, AoE, Cast, etc.).
- apply the effect to hero; an effect could be damage, status, buff or debuff.
- read here for more detail: [Modular Skill System](https://docs.google.com/spreadsheets/d/1PSSGZAq2gkkOxTENDWpChI_2OpXvTmQIfPxZebuuDsc/edit?usp=sharing)

**Modifiers**

- resolves modifiers, e.g. buffs, debuffs and statuses.
- counts down modifier timers.
- computes the final stat after combining modifiers.
- returns whether a status is active when asked.

**StatScaling** — how stat ratios and scaling are computed.

**VFX** — floating combat text.

**UI** — overlay panels (inspector, shop), and world-space UI (health bar, mana
bar, skill bar).

**Engine** — services that decouple the rest of the game from the Unity engine.

**Editor** — inspector tooling for Core.

**Combat.Tests** — EditMode tests for Combat, Contracts and Skills.
