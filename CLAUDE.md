# Magic School 3

Unity auto-chess game: heroes are bought, benched, and placed on a hex board, then auto-battle. Heroes on opposing teams move toward each other and fight; the side with all heroes dead loses. UI is built with **UI Toolkit** (not uGUI).

## Architecture (`Assets/Scripts/`)

- **`Hex.cs`** — one per hex tile. Computes its own neighbors by distance rather than axial math (flat-top hex grid: same-column neighbors are ~1.0 apart, diagonal neighbors ~1.118 apart; a 1.15x threshold on the nearest distance catches exactly the real neighbors, including at board edges with fewer than 6). Tracks `Occupant` (a `Hero` or null) so only one hero can stand on a hex at a time. Needs `SetBoard()` called by `BattleBoard` before use — do not call `GetNeighbors()` before that's happened.
- **`BattleBoard.cs`** — discovers all child `Hex`es in `Awake()` and keys them in `Dictionary<HexPlacement, Hex>` by `(Team side, int column, int row)`, derived from position rather than hardcoded, so it stays correct if the board layout changes. Tracks all spawned heroes in `HeroesOnBoard`. `SpawnHero(prefab, hex, team)` is the entry point for placing a hero — it also wires up `hex.SetBoard()`/`hero.SetBoard()`.
- **`Hero.cs`** — walks via `Vector3.Lerp` + an `AnimationCurve` (ease-in/ease-out) over `1 / moveSpeed` seconds per hex — not `CharacterController` (the board is 2D sprites with no colliders; a hero prefab never had one, so that path throws `MissingComponentException`). `MoveTowardEnemy()` greedily steps toward whichever enemy hero is nearest, stopping once already adjacent (distance == 1 hex). Reserves its destination hex the moment it *commits* to a step, not on arrival — reserving only on arrival would let two heroes both target the same free hex in the same frame.
- **`HexPlacement.cs`** — defines `enum Team { Blue, Red }` and `struct HexPlacement { Team team; int column; int row; }`. Used as `BattleBoard`'s dictionary key and for the Inspector-assigned starting positions in `BattleBoard._heroPlacement`. Must stay a `struct` (value-based equality is required for dictionary lookups to work by value; a `class` here would silently break lookups via reference equality).

**UI Toolkit panel pattern** (`Assets/UI Toolkit/`): one shared `UIDocument` on `MainScreen.uxml`, which has empty named containers as slots (`#BenchPanel`, and eventually `#TraitPanel`/`#HeroPanel`/`#ShopPanel`). Each actual panel is its own `.uxml` file plus a small controller script (see `BenchPanelController.cs`) that clones its UXML into the matching-named slot at runtime. The convention: name a panel's root element in its own `.uxml` the same as the slot container waiting for it in `MainScreen.uxml` — the controller looks up the slot by reading its own root element's name, so no manual string field has to be kept in sync.

## What's built vs. pending

- Movement, pathfinding (greedy step-toward-nearest-enemy), and hex occupancy are working.
- **Not built yet:** combat (attack speed, mana — 10 per attack, skill on full mana per the original design), and every UI panel except Bench (Shop, Trait Panel, Hero Panel are just empty slots).
- Pathfinding is greedy-step only, not real BFS — it skips a fully-occupied best-neighbor but doesn't route around obstacles otherwise. Acceptable for now; revisit if occupancy contention becomes a visible problem with more heroes on board.
- `BattleBoard._heroPlacement` (Inspector list of starting `HexPlacement`s) has been reset to defaults multiple times across past type changes — double-check its values before relying on it.

## Working in this project

- **Coplay MCP** (`mcp__coplay-mcp__*` tools) is connected to this project's live Unity Editor: `play_game`/`stop_game`, `execute_script` (runs arbitrary C# in the Editor), `get_game_object_info`, `get_unity_logs`, `check_compile_errors`, prefab editing via `add_component`/`set_property`.
- **Always verify non-trivial script/prefab changes live** rather than trusting compilation alone: enter Play mode and run a throwaway diagnostic `execute_script` (write it to the scratchpad dir) that checks actual runtime values or reproduces the reported bug, then `stop_game`. Unity execution-order/lifecycle bugs (e.g. `Awake()` ordering races between objects) don't show up as compile errors.
- **`mcp__coplay-mcp__save_scene` gotcha:** passing a bare scene name (e.g. `"Board"`) does a "Save As" into the wrong path (`Assets/Board.unity` instead of `Assets/Scenes/Board.unity`) and silently switches the Editor's active scene to that new wrong file. Always pass the full relative path without extension, e.g. `"Scenes/Board"`.
- The user leaves inline code-review questions via CodeTour `.tours/*.tour` JSON files (edited in VS Code). When asked to "read .tour" (or similar), `Glob` `.tours/*.tour` (the filename itself has changed before, e.g. `review.tour` → `revview.tour`) and read all steps — but respect explicit "Don't answer" notes on individual steps, those are self-notes rather than questions.
- VS Code debugging uses the `visualstudiotoolsforunity.vstuc` extension (the current official one, not the deprecated "Debugger for Unity"), with an "Attach to Unity" launch config already in `.vscode/launch.json`.
- GitHub remote: https://github.com/SwiftkeyX/Magic-School-3.git, branch `main`.
