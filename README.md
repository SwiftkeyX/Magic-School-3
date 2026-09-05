# Magic School 3

A Unity auto-chess game. Buy heroes, place them on a hex board, then watch them fight on
their own.

![A fight in progress](docs/battle.gif)

Built with **Unity 6000.4** (URP, 2D) and **UI Toolkit**.

> Work in progress. Combat, movement and skills run; the economy around them does not yet —
> see [What works today](#what-works-today). Heroes are placeholder shapes: the systems came
> first, the art has not.

## Installing

Two things have to be in place before the project will open:

| Needed | Why |
| --- | --- |
| **Unity 6000.4.9f1** | Install it through [Unity Hub](https://unity.com/download). The version is pinned in `ProjectSettings/ProjectVersion.txt`; a newer 6000.4.x will open the project and offer to upgrade it. |
| **Git on your `PATH`** | Not only to clone. One package is pulled straight from a git URL, and Unity's Package Manager shells out to `git` to fetch it — without it the project opens with a missing dependency. |

Then clone and open it:

```bash
git clone https://github.com/SwiftkeyX/Magic-School-3.git
```

In Unity Hub, **Add → Add project from disk**, and pick the folder you just cloned.

Nothing needs installing by hand after that. Every other dependency comes from the Unity
registry, and `Packages/packages-lock.json` pins all of them — the git one by commit hash — so
you get the versions this was built against rather than whatever is current.

**The first open is slow, and that is expected.** Unity builds its `Library/` folder from
nothing: importing every sprite, compiling every assembly, generating shader variants.
Minutes, not seconds. It is cached afterwards, and `Library/` is gitignored, so you pay it
once per clone.

Then load `Assets/Scenes/Board.unity` and press Play.

### If a package fails to resolve

One dependency does not come from the Unity registry:

```
"com.coplaydev.coplay": "https://github.com/CoplayDev/unity-plugin.git#beta"
```

It is an editor-side assistant plugin, and the game does not use it — nothing under
`Assets/Scripts/` references it and no `.asmdef` lists it. If Package Manager cannot fetch it
(no git on the `PATH`, no network, or the `beta` branch has moved), delete that line from
`Packages/manifest.json` and reopen the project. Unity will rewrite the lock file, and the
game builds and plays without it.

## Running it

| Input | Does |
| --- | --- |
| Left-click | Pick item/hero up |
| Right-click | Open the inspector on it |
| `Space` | Start the fight, and continue to the next stage |
| `R` | Quick restart the current stage |
| `1` `2` `3` | Run the game at x1, x2 or x3 speed |

## What works today

**Running:**

- **Movement and pathfinding.** hero move on a hex toward a nearest enemy using A\* algorithm on the hex grid, with hex reservation so two heroes
  cannot commit to the same tile in the same frame.
- **Combat.** Auto-attack on an attack-speed cooldown, passive skill when condition is true, and a skill cast when mana caps.
- **Damage, healing, modifiers and statuses** they can be apply to a hero.
- **Skills.** A skill is a list of steps, each playing a template action — projectile, AoE or
  hitbox — picked by condition. **24 heroes exist; 17 of them have skills built.**
- **Item.** item wear to a hero, grant hero modifiers.

**Not built yet:**

- **Gold and economy.** The Shop resolves buy-versus-cancel when you release a drag, but it
  cannot charge for the purchase.
- **Trait panel.** Still an empty slot in the main screen layout.

## How the code is laid out

The code is split into 12 modules along `.asmdef` boundaries, so the dependency direction is
enforced by the compiler — see **[ARCHITECTURE.md](ARCHITECTURE.md)**
for the dependency graph and what each module does.

## License

No license file yet, which means all rights are reserved by default.
