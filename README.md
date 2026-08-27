# Magic School 3

A Unity auto-chess game. Buy heroes, place them on a hex board, then watch them fight on
their own — heroes on opposing teams walk toward each other and trade blows until one side
has nobody left standing.

Built with **Unity 6000.4** (URP, 2D) and **UI Toolkit**.

> Work in progress. Combat, movement and skills run; the economy around them does not yet —
> see [What works today](#what-works-today).

## Running it

Open the project in Unity 6000.4.9f1 or newer, load `Assets/Scenes/Board.unity` and press
Play. All dependencies come from the Unity registry and one public git package, so there is
nothing to install by hand.

| Input | Does |
| --- | --- |
| Left-drag a hero | Pick it up and drop it on a hex or a bench slot |
| Right-click a hero | Open the inspector on it; right-click empty space to close |
| `Space` | Start the fight — and, once it's over, continue to the next stage |
| `R` | Restart the current stage |

There is no Start Battle button yet, which is why `Space` does that job.

## What works today

**Running:**

- Movement and **A\*** pathfinding over the hex grid, with hex reservation so two heroes
  never commit to the same tile in one frame.
- Auto-attack on an attack-speed cooldown, mana (10 per attack), and a skill cast when mana
  caps.
- Damage, healing, modifiers and statuses.
- Skills for 8 of the heroes: Vharn, Sithra, Bulwark, Roland, Quatre, Solace, Vesper, Pip.
- Stage progression — clear a stage and the next one seeds; the team you may field grows by
  one hero per win.

**Not built yet:**

- **Gold and economy.** The Shop resolves buy-versus-cancel when you release a drag, but it
  cannot charge for the purchase.
- **Trait panel and hero panel.** Both are empty slots in the main screen layout.
- **A Start Battle button.** Combat is keyboard-triggered, as above.

## How the code is laid out

Scripts live in `Assets/Scripts/`, split into assemblies (one `.asmdef` per top-level
folder) so the dependency direction is enforced by the compiler rather than by convention.
Arrows point at what a module may reference:

```
Contracts   -> (nothing)        interfaces + enums everyone shares
Engine      -> (nothing)        DebugTool, SceneHelper, CurveMotion
VFX         -> (nothing)        FloatingText
StatScaling -> Contracts        StatRatio + Scaling, the amount math
Modifiers   -> Contracts
Skills      -> Contracts, Engine, StatScaling, Modifiers
Combat      -> Contracts, Engine, Skills, VFX
UI          -> Contracts, Combat
Core        -> Contracts, Engine, Skills, Combat
Player      -> Contracts, Combat, Core, Unity.InputSystem
Editor      -> Core                                        (Editor-only)
```

`Contracts` sits at the bottom and references nothing — anything it reached for would be
dragged underneath every other module. Each module carries a `*Module.md` next to its
asmdef stating its own boundary, and `CLAUDE.md` records the reasoning behind the
boundaries that aren't visible from any single file.

The pieces worth knowing about:

- **`Combat/Placements/`** — `Hex` works out its own neighbours by distance rather than
  axial math, so it survives board layout changes; `BattleBoard` owns hero tracking and hex
  reservations; `HexPathfinder` is A\*, returning one step at a time.
- **`Combat/Heroes/`** — `Hero` is deliberately glue with no logic of its own. The behaviour
  is a state machine over Idle / Walk / Attack / Cast / Stunned / Dead.
- **`Skills/`** — a skill is a list of steps, each picking a template action (projectile,
  AoE, hitbox) by condition. Effects apply through an interface, so nothing here knows the
  `Hero` type.
- **`Core/`** — the composition root: game phase, winner, and the wiring between the rest.

UI is **UI Toolkit**, not uGUI: one shared document (`Assets/UI Toolkit/MainScreen.uxml`)
with named containers as slots, and each panel cloning its own `.uxml` into the matching
slot at runtime. The bench is the deliberate exception — it needs real animated hero
objects standing on it, which a `VisualElement` cannot host, so it is world-space.

Every hero prefab is a variant of `Assets/Prefabs/Hero/BaseHero.prefab`, overriding only
name, sprite colour and position. Shared components belong on `BaseHero`, never on an
individual hero.

## License

No license file yet, which means all rights are reserved by default.
