# AI.md — Notes for AI coding agents

This file gives AI coding agents the practical, non-obvious knowledge needed
to work in this repository. Read it before changing code or content.

---

## What this project is

Galactik Wars is a 3D multiplayer space combat game originally written in
2008 for **XNA Game Studio 2.0** (Windows + Xbox 360, Xbox LIVE networking).
The repository has been fully ported to **MonoGame 3.8.6.1** on **.NET 10**
(DesktopGL backend). Original XNA-era code is preserved in git history
(commit `cde75b1`); the working tree is the ported version.

## Build and run

```sh
dotnet tool restore                    # MGCB local tool (.config/dotnet-tools.json)
dotnet build src/GalactikWars.slnx     # builds code + content pipeline
dotnet run --project src/GalactikWars  # launches the game (opens a window)
```

- Always build from the repository root.
- Content pipeline errors appear in the same build as C# errors; check
  `Content.mgcb` paths when a build fails with a content message.
- A successful build compiles all content; a clean `Build succeeded` means
  every asset in `Content.mgcb` is valid.

## Repo conventions

- **Central package management**: package versions live only in
  `src/Directory.Packages.props`. `PackageReference`s in `.csproj` files must
  be version-less. Never add a `Version` attribute.
- **Package sources**: `nuget.config` clears inherited feeds and maps
  everything to nuget.org. This is intentional (the developer machine has
  private feeds that break NU1507).
- **Solution format**: `src/GalactikWars.slnx` is XML and **case-sensitive**
  in `Project Path` attributes. There is no `.sln`.
- **Local tools**: MGCB is a local tool pinned in `.config/dotnet-tools.json`
  (version `3.8.6.1`, no floating versions). After changing it, run
  `dotnet tool restore`.
- **Namespaces match folders**: every file's namespace is its folder path
  under the project root (`GalactikWars.Screens` for `src/GalactikWars/Screens/`,
  etc.). Root-level files (`Game.cs`, `GameVariables.cs`, `Resources.resx`)
  are `GalactikWars`. When creating a folder, its files must use the matching
  namespace.
- **Namespace/type name collisions**: two names appear both as a namespace
  and as a type — this forces special handling when referencing them from
  *other* namespaces (C# binds the enclosing namespace's member first):
  - `ScreenManager` (namespace `GalactikWars.ScreenManager`, class
    `ScreenManager`): files *inside* that namespace need nothing; files in
    sibling namespaces use a **using-alias placed inside the namespace
    declaration**: `using ScreenManager = GalactikWars.ScreenManager.ScreenManager;`
    (a top-of-file alias does NOT work there; an alias named `ScreenManager`
    inside namespace `GalactikWars` itself is illegal too, so `Game.cs`
    fully qualifies instead). Instance code on `GameScreen` subclasses
    usually doesn't need the alias because the inherited
    `GameScreen.ScreenManager` property wins.
  - `ScreenState` (namespace-level enum in `GalactikWars.ScreenManager` vs
    the inherited `GameScreen.ScreenState` property): comparisons in
    subclasses use the alias
    `using ScreenStateEnum = GalactikWars.ScreenManager.ScreenState;`.
  - The particle engine class is `ParticleSystem` (file
    `Particles/ParticleSystem.cs`) — deliberately not named `Particles` to
    avoid the same collision with namespace `GalactikWars.Particles`.

## Language / framework gotchas

These are the differences between the old XNA code and MonoGame that will
bite you if you copy code from git history:

1. **`SpriteBatch.Begin` argument order** is `(SpriteSortMode, BlendState)`.
   XNA's `Begin(SpriteBlendMode…)` overload does not exist. Every `Begin` in
   the codebase takes sort mode first.
2. **`Color` constructor ambiguity**: MonoGame has both `(int, int, int, int)`
   and `(byte, byte, byte, byte)` overloads. XNA-era calls that mix int
   literals with byte variables (e.g. `new Color(255, 255, 255, TransitionAlpha)`
   where `TransitionAlpha` is `byte`) are ambiguous — **cast the int literals
   to `(byte)`**.
3. **`GameTime`**: `TotalRealTime` and the 4-argument constructor do not
   exist. Use `TotalGameTime`/`ElapsedGameTime` and the
   `(total, elapsed, isRunningSlowly)` constructor.
4. **No XNA Live**: `Microsoft.Xna.Framework.Net`, `GamerServices`,
   `Storage`, `GamerPrivilegeException` and `Guide` must not reappear.
   The replacement is the in-repo library **`GalactikWars.Net`**
   (`src/GalactikWars/Net/`), which mirrors the XNA `NetworkSession` API over
   UDP LAN. `NetworkSessionComponent` in `src/GalactikWars/Networking/` is
   the ported game component (ours, not XNA's) — it is fine to reference.
5. **Audio**: XACT is gone. All sound goes through
   `src/GalactikWars/Handlers/SoundHandler.cs` (`SoundEffect` + a `Cue`
   wrapper). There is **one shared instance** on `ScreenManager`
   (`ScreenManager.SoundHandler`), loaded and updated by the `ScreenManager`
   itself — screens access it via `soundHandler => ScreenManager.SoundHandler`.
   Music is organized as **named tracks** (`SoundHandler.MenuTrack`,
   `LoadingTrack`, `BattleTrack`, `LobbyTrack`, `GameOverTrack`,
   `CreditsTrack`), switched with `PlayTrack(...)`; screens select their
   track in `LoadContent` (or `Update` for the main menu). Effect cue names
   (`engine_1`, `engine_2`, effect names like `explosion1`) are stable
   identifiers — keep them when adding sounds.

## Gameplay behavior (current)

How the game plays today — keep these in sync when touching gameplay code:

- **Controls**: `W`/`S` move, `A`/`D` rotate, `Space` fires bullets, `R`
  fires rockets, `B` triggers the atom bomb (single-player only). `E`/`Q`
  zoom. Menus are navigable with `W`/`S` as well as the arrow keys
  (`InputState.MenuUp/MenuDown`; select = `Space`/`Enter`, cancel =
  `Escape`). Mouse buttons map to the same weapons (left = bullets, right =
  rockets, middle = bomb). Movement is **keyboard-only** — the old
  mouse-steering (`VariationX/Y`, `MouseMoved`) was removed from
  `HandleInput`; `GameVariables.MouseVisible/MouseActive` are `true` for the
  weapon buttons.
- **Sound cue mapping** (the reports' intent, keep it when touching audio):
  engine loops are `engine_1` (own ship) / `engine_2` (others) via
  `PlayMyEngineSound`; a large asteroid breaking into smaller ones is
  `explosion4`; a small asteroid being destroyed is `explosion1`; a ship and
  a ship/meteor being destroyed together is `explosion2`; the atom bomb is
  `explosion3`; a meteor hitting a shield is `phase_collide_ship1`; a ship
  shield hitting another ship shield (multiplayer) is `shield_collide_ship1`.
- **Engine exhaust / thrusters**: while thrusting (`S`), the single-player
  screen emits a green **jet-engine plume** behind the ship. The tail is
  `-RotationMatrix.Backward` — `RotationMatrix.Backward` is `(sin r, −cos r,
  0)` and that end is the ship's **nose/front** (it is where bullets and
  rockets exit). Forward thrust (`W`) instead fires **twin thrusters at the
  nose** (`EmitForwardThrusters`): two nozzles flanking the centerline
  (±0.35 head distance on the `(cos r, sin r)` lateral axis), flames
  venting **forward** — the reaction pushes the ship along its W travel
  direction. The tail distance / head distance is the ship model's own
  bounding-sphere radius (the 6 ship hulls are different sizes); the plume
  stretches about one hull-length (scaled by zoom) with a lateral jitter,
  and each particle's `lifeSpan` grades from 8 at the nozzle (bright green)
  to 1 at the tip (dark green) — bright core, fading plume.
  Particles are type 1 (`CreateTrailParticles`, 4/frame), drawn at their
  `xyorg` only. Reaction-control jets (`A`/`D`) are also implemented
  (`EmitRcsJet`): the nozzle sits just off the head, on the lateral side
  `(cos r, sin r) * side` (`side` = −1 for `A`, +1 for `D`), and the flame
  points outward from that side — the jet pushes the nose into the turn.
  Same shared `shipTrail` pool, same heat-graded plume style, shorter puff
  (0.8× the head screen distance). Do not
  reuse the explosion emitter-count logic for trails — the ship's emitter
  origin changes every frame.
- **Particle emitters**: every explosion/burst emitter position in the
  single-player screen goes through the same `WorldToScreen` helper —
  meteor explosions (`CollisionsHandler.explosionPosition`, a `Vector2`
  lifted to `Vector3(X, Y, 0)`), ship death, and the atom bomb (world
  origin = screen centre). The old 800×600-era hand math put bursts at the
  wrong X — never re-introduce those formulas.
- **Zoom limits**: `E`/`Q` change the camera height (±500, accumulated by
  `CCamera.SetTranslation`) and the playfield size (±200). Both are clamped
  — camera height within `GameVariables.MinZoomHeight/MaxZoomHeight`
  (10000–200000, 2D uses `camera.Position.Z`, 3D the negative-Y height),
  playfield within `MinPlayfieldSize/MaxPlayfieldSize` (10000–100000) — so
  fully zoomed in/out views can no longer collapse the camera onto the
  playfield plane or the playfield to zero/negative.
- **Weapon spawn points**: bullets and rockets exit from the ship's **nose**:
  `RotationMatrix.Backward`, which evaluates to `(sin r, −cos r, 0)` — the
  front end. The tail is `-RotationMatrix.Backward` (= `.Forward`), where
  the engine exhaust burns.
- **Rockets**: limited ammo (`GameVariables.RocketAmmo`, 10 per life),
  tracked by `Player.rockets` and shown on the single-player HUD. Before the
  player updates, `GameplayScreenSinglePlayer.HomeRockets` steers every
  active rocket toward the nearest active asteroid via `Rocket.SteerToward`
  — a turn-rate-limited missile model (thruster correction, arcs instead of
  snapping). `Rocket.Update` re-derives `rotation` from `direction` every
  frame (`CreateRotationX(PiOver2) * CreateRotationZ(atan2(-dir.X, dir.Y))`)
  so the drawn nose keeps pointing along the flight path while homing; the
  rocket model shares the ship's nose convention (nose = matrix-backward).
  Multiplayer rockets home the same way toward the opposing ship via
  `GameplayScreenMultiplayer.HomeRockets` (deterministic per synced ship
  positions).
- **Asteroid splitting**: destroying a large asteroid spawns
  `GameVariables.AsteroidSplitCount` (2) small ones via
  `SpawnSplitAsteroids`, using the extra slots at the end of `asteroid2List`
  (`GameVariables.NumAsteroid2Slots` = `NumAsteroids2 + 2 * NumAsteroids1`).
  Loops over `asteroid2List` use its full `Length`; only the first
  `NumAsteroids2` slots are populated by `ResetAsteroids`.
  `CollisionsHandler.CheckForRocketAsteroidCollision` is single-target and
  returns 0 (no hit), 1 (small destroyed) or 2 (large destroyed — caller
  splits). All of this is single-player only: multiplayer is ship-vs-ship
  with no asteroids.
- **Enemy/ally ships (phase 3, single-player only)**: `EnemyShip`
  (Entities/EnemyShip.cs) covers both roles via `IsAlly` — red enemies
  (`p1_pencil` model, red ambient tint) hunt the player: they steer the
  nose (`RotationMatrix.Backward`, same convention as the player ship)
  toward the player at `GameVariables.EnemyTurnRate`, strafe toward them
  (thrust independent of facing) until `EnemyStandoff`, and fire along the
  nose when aligned (< 0.35 rad) and in range, on a ~0.9–1.9 s cooldown.
  Destroyed enemies respawn after `EnemySpawnSeconds` while the count is
  below `NumEnemyShips`. The blue ally (`p1_saucer`, blue tint) escorts:
  it flies toward the player until `AllyStandoff` (emitting the same twin
  frontal bursts while thrusting), aims at the nearest active enemy and
  fires; it respawns 3 s after falling (only while the player lives). Hits:
  bullets −10 shield, rockets destroy outright (`CheckForBulletEnemyShip`,
  `CheckForRocketEnemyShip`, `CheckForAllyBulletEnemyShip` — ally kills
  credit the player's score —, `CheckForEnemyBulletShip`,
  `CheckForEnemyBulletAlly`); kills score `EnemyShipBonus` (500) and the
  atom bomb also wipes the squadron. Enemy/ally ships ignore asteroids
  (future polish) and multiplayer has none of this.
- **Death flow (single-player)**: no manual respawn key. When the ship dies
  with lives left, a 1 s `respawnTimer` calls `WarpToCenter()` with the
  `hyperspace_activate` cue. With zero lives, a 4 s `gameOverTimer` returns
  to the main menu via `LoadingScreen.Load` (once, guarded by
  `returnedToMenu`).
- **Background (single-player)**: the level texture is drawn stretched over
  the whole viewport with a ±5% parallax margin following the ship;
  multiplayer stretches its background to the viewport as well.
- **Lighting**: `SetupEffectDefaults` and `DrawModel` in both gameplay
  screens set `effect.AmbientLightColor` to `(0.55, 0.55, 0.55)` right after
  `EnableDefaultLighting()` — it is a `Vector3`, not a `Color`.

## Content project rules (`assets/Content.mgcb`)

- The content project lives at the repository root in **`assets/`**, together
  with all raw assets (`audio/`, `fonts/`, `models/`, `textures/`, and UI
  textures under `ui/` — nothing sits loose at the `assets/` root except the
  mgcb itself). Source code stays under `src/` — do not
  move assets back into it.
- **Never quote `/build:` paths.** The MGCB parser in 3.8.6.1 does not strip
  quotes; `"audio/Music/battle.WAV"` fails because the quotes become
  part of the path. This is why the font file
  `Lucida Console.spritefont` was renamed to `LucidaConsole.spritefont` and
  code loads `fonts/LucidaConsole`.
- Asset paths in `/build:` are relative to the `assets` folder and become
  the runtime load name (`models/p1_pencil`, `fonts/LucidaConsole`,
  `audio/Effects/explosion1`).
- Models use `XImporter` (DirectX `.x` files). `.fbx` files exist on disk but
  are **not** referenced by the mgcb — do not add them back; MonoGame cannot
  import FBX 6.0 (see the converter below).
- Textures referenced by models are looked up relative to the model file
  (`..\textures\foo.tga` from `assets/models/`), matching case-insensitively
  on Windows.

## Model conversion tool

`tools/FbxToXConverter/` converts the 2008 ASCII FBX 6.0 ship models into
DirectX `.x` text meshes the pipeline can import. If a model ever changes:

```sh
dotnet run --project tools/FbxToXConverter -- assets/models/<name>.fbx
```

It writes `<name>.x` next to the source, bakes the node transform into a
`FrameTransformMatrix`, unrolls polygon-vertex attributes and fan-triangulates
polygons. The FBX parser only understands the FBX 6.0 *inline array* syntax
(`Vertices: 1,2,…` terminated by a blank line) and name-based `Connect:` links
— not FBX 6.1+ `*N { a: … }` arrays or numeric-ID connections.

## Networking library invariants

`src/GalactikWars/Net/` (UDP, LAN only):

- Host binds UDP port **26900**.
- Constants: find window 2 s, `GamerTimeout`/`HostTimeout` 5 s, roster
  broadcast 500 ms, discovery beacon 1 s, `MaxGamers` 6.
- Star topology: clients talk only to the host; the host relays `Data`
  packets between gamers.
- Packets are length-prefixed binary (`Packet.cs`); adding fields means
  bumping handling on both host and client sides.
- Host migration is accepted by the API but not orchestrated by the protocol —
  don't promise it in messages shown to users.

## Style

- The codebase keeps its original XNA-sample formatting (4-space indent,
  `#region` blocks, XML doc comments). Match the surrounding style of each
  file rather than reformatting.
- Namespaces: game code is `GalactikWars`, the networking library is
  `GalactikWars.Net`.
- Do not re-enable `EnableDefaultCompileItems` changes; the game project
  relies on default item globbing.

## Verification checklist

Before finishing a change:

1. `dotnet build src/GalactikWars.slnx` succeeds with 0 errors.
2. `dotnet run --project src/GalactikWars` launches the game window without
   console errors (content loads, menus render).
3. `git grep -n -E "Microsoft\.Xna\.Framework\.(Net|GamerServices|Storage)"` is
   empty (ignoring this file and docs).
4. If you touched networking, both host and client paths still compile
   against `NetworkSession`'s public surface.
