# UI Toolkit

Why `MainScreen.uxml` holds a copy of every panel that nothing ever uses at runtime.

## The short version

Each panel is mounted **twice**, by two different mechanisms, for two different reasons:

1. **`<ui:Instance>` inside `MainScreen.uxml`** - hand written, exists only so the panel is
   visible in the UI Builder and in the Inspector's UXML preview. Nothing reads it at runtime.
2. **`PanelMounter.MountInMainPanel`** - clones the panel's own `.uxml` fresh, at `OnEnable`,
   and that clone is the one you actually see when you press Play.

The mount **throws the first one away**:

```csharp
slot.Clear();      // <- the hand-written <ui:Instance> dies here
slot.Add(panel);   // <- the runtime clone takes its place
```

So the previews are scaffolding for edit mode. Delete them all and the game looks identical;
you just get an empty grey rectangle in the Builder instead of a panel.

## How a panel finds its slot

`MainScreen.uxml` is a skeleton of empty named `VisualElement`s. `PanelMounter` looks the slot
up **by the panel's own root element name**:

```csharp
VisualElement slot = mainPanel.Q<VisualElement>(panel.name);
```

So `Scoreboard.uxml`'s root element is named `ScoreboardPanel`, because that is the slot waiting
for it in `MainScreen.uxml`. No controller carries a slot-name string that could drift out of
sync - the names *are* the wiring.

Rename either side and the panel silently does not appear. There is no exception, only a
`Debug.LogWarning` from `PanelMounter`.

## Slot map

One `UIDocument` GameObject in `Board.unity` carries the single `UIDocument` (`MainScreen.uxml`)
plus every `PanelController`, each with its own `_panelAsset`.

| slot in MainScreen | edit-mode preview | mounted at runtime by |
|---|---|---|
| `#ShopPanel`          | yes             | `ShopPanelController` - `Shop.uxml` |
| `#HeroPanel`          | yes             | `InspectorController` - `HeroPanel.uxml` |
| `#HeroCountPanel`     | yes             | `HeroCountController` - `HeroCountPanel.uxml` |
| `#PhaseHintPanel`     | yes             | `PhaseHintController` - `PhaseHintPanel.uxml` |
| `#ResultBannerPanel`  | yes, `display: none` | `ResultBannerController` - `ResultBannerPanel.uxml` |
| `#ScoreboardPanel`    | yes, `display: none` | `ScoreboardController` - `Scoreboard.uxml` |
| `#RewardPanel`        | yes, `display: none` | `RewardPanelController` - `RewardPanel.uxml` |
| `#TraitPanel`         | -               | nothing yet |
| `#BenchPanel`         | -               | nothing, deliberately - the bench is world-space `Hero` objects, not `VisualElement`s |
| `#BoardArea`          | -               | nothing - the hex board shows through from world space |

The three `display: none` previews are full-screen overlays. Left visible they would cover the
whole Builder canvas and you could not edit anything underneath.

## Traps

**Styling the `<ui:Instance>` does nothing at runtime.** This is the one that costs an hour.
Layout you set on the `<ui:Instance>` element inside `MainScreen.uxml` looks right in the
Builder and is gone in Play mode, because that element is what `slot.Clear()` deletes. Only two
places survive:

- the **slot** `VisualElement` itself (its width, padding, `position: absolute`, etc.)
- the panel's **own** `.uxml` / `.uss`

If a panel is the right size in the Builder and the wrong size in Play, this is why.

**`display: none` in the preview is not how a panel hides at runtime.** Runtime hiding is
`PanelController.SetShown`, which toggles the `is-hidden` class - each panel's `.uss` defines
it. Two unrelated mechanisms that happen to look the same.

**After mounting, two nested elements share one name** - the slot, and the panel root inside it.
`MainPanel.Q<VisualElement>("ScoreboardPanel")` returns the *slot*, not the panel. Query from
the `panel` argument `OnMounted` hands you, never by name from the root.

**`<ui:Template src=...>` carries a GUID.** Moving or renaming a panel `.uxml` is safe as long
as the `.meta` travels with it. Deleting and recreating one is not - the Instance then points at
a GUID that no longer exists.

**UI Builder deletes every comment in a file it saves.** That is why this is a README and not
comments in `MainScreen.uxml`. After any Builder session, `git diff` the `.uxml`/`.uss` and put
back the prose it dropped. It has already emptied `MatchStatus.uss` and `HeroPanel.uxml` once.

## Adding a panel

1. New `MyPanel.uxml`, root element named `MyPanel`.
2. An empty `<ui:VisualElement name="MyPanel"/>` slot in `MainScreen.uxml`, positioned and sized
   there - remember the slot is the half that survives.
3. A `MyPanelController : PanelController`, override `OnMounted(panel)`, query children off
   `panel`.
4. Add the component to the `UIDocument` GameObject and point `_panelAsset` at `MyPanel.uxml`.
5. Optional: a `<ui:Template>` + `<ui:Instance>` in the slot so you can see it in the Builder.
   Runtime does not care either way.

## Odds and ends

- `Ghost.uxml` is not a panel. It is the drag ghost the shop spawns, cloned by
  `ShopPanelController` via `_ghostAsset`.
- `MatchStatus.uss` is a shared stylesheet - `HeroCountPanel`, `PhaseHintPanel` and
  `ResultBannerPanel` all pull it in. `MatchStatus.uxml` next to it is referenced by nothing:
  no slot, no controller, no scene reference.
