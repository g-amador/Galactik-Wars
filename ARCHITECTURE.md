# Architecture

This document describes how Galactik Wars is structured and what was involved
in porting it from XNA Game Studio 2.0 (2008) to MonoGame 3.8.6.1 on .NET 10.

---

## 1. High-level overview

```
┌────────────────────────────────────────────────────────────────┐
│                        GalactikWars (game)                      │
│                                                                │
│  Game.cs (loop) ── ScreenManager ── GameScreens                │
│      │              │                 │                        │
│      │              │                 ├─ Screens/   (menus,     │
│      │              │                 │              gameplay)  │
│      │              │                 └─ Networking/ (session  │
│      │              │                                flow)      │
│      │              │                                                          │
│      │              ├── InputState (keyboard/mouse/gamepad)                      │
│      │              └── MessageDisplayComponent (on-screen messages)             │
│      ├── Handlers/    SoundHandler, CollisionsHandler                            │
│      ├── Entities/   Ship, Asteroid, Bullet, Rocket, Player, CCamera,            │
│      │               ScrollingBackground                                        │
│      ├── Particles/   Particle, ParticleSystem                                   │
│      └── Net/  GalactikWars.Net (LAN networking library, UDP)                   │
│                                                                 │
│  Content pipeline: Content.mgcb → textures, .x models, fonts, wav │
│  MonoGame 3.8.6.1 (DesktopGL) on .NET 10                          │
└────────────────────────────────────────────────────────────────┘
```

The codebase descends from the Microsoft **GameStateManagement** and
**NetworkGameStateManagement** XNA samples, extended into a full game by the
original authors. That heritage is still visible in the screen management,
loading screen and network session screens.

## 2. Runtime structure

Every source file's namespace matches its folder location
(`GalactikWars.Screens`, `GalactikWars.Entities`, `GalactikWars.Handlers`,
`GalactikWars.Particles`, `GalactikWars.ScreenManager`, `GalactikWars.Net`,
`GalactikWars.Networking`); root-level files are `GalactikWars`. The two
namespace/type name collisions (`ScreenManager`, `ScreenState`) and their
using-alias workarounds are documented in `AI.md`.

### 2.1 Game loop — `Game.cs`

The `GalactikWars.NetworkStateManagementGame` class owns the MonoGame
`Game` loop, the `GraphicsDeviceManager`, the `ScreenManager` and the
`MessageDisplayComponent`. The original XNA version also added a
`GamerServicesComponent`; that component does not exist in MonoGame and was
removed.

### 2.2 Screen system — `ScreenManager/`

- `ScreenManager` keeps a stack of `GameScreen` instances with per-screen
  transition alpha, shared `SpriteBatch`, input state and content.
- `InputState` unifies keyboard, mouse and gamepad input per player.
- `MenuScreen`/`MenuEntry` provide the animated menu lists used by every menu.
- `LoadingScreen` loads the next screen on a background thread while animating;
  it was adapted to MonoGame's `GameTime` constructor shape.
- `MessageBoxScreen` displays arbitrary yes/no or OK dialogs; its messages
  come from `Resources.resx`.

### 2.3 Screens — `Screens/`

| Screen | Role |
| --- | --- |
| `BackgroundScreen` | Scrolling starfield behind all menus |
| `MainMenuScreen` | Single player / Multiplayer / Options / Credits / Exit |
| `ControlsScreen`, `OptionsMenuScreen`, `SoundScreen`, `CreditsScreen` | Settings and info |
| `PauseMenuScreen` | Pause / resume / quit |
| `ShipSelectionScreen` | Single-player hull pick with a rotating 3D preview and team colour |
| `EscortSelectionScreen` | Single-player escort pick (one loadout advantage per escort) |
| `GameplayScreenSinglePlayer` | Single-player combat |
| `GameplayScreenMultiplayer` | Networked combat (sends/receives game data packets) |
| `LoadingScreen` | Asynchronous world load with progress animation |

`ModelPreviewRenderer` draws a hull as a spinning 3D model inside a
back-buffer rectangle, so the two single-player pickers can show the real
ships before launch. The camera sits in front of the hull, 30 degrees above
its plane, far back with a narrow field of view — the ship is seen from the
front (top and sides) instead of straight from above.

### 2.4 Networking screens — `Networking/`

These screens implement the session flow and are almost untouched from the
XNA sample, since the networking library keeps the XNA `NetworkSession` API:

`CreateOrFindSessionScreen` → (`CreateSessionScreen`
path or `JoinSessionScreen` via session discovery) → `LobbyScreen` →
`GameplayScreenMultiplayer`. `NetworkBusyScreen` and `NetworkErrorScreen`
handle asynchronous operation progress and failures.

`NetworkSessionComponent` is a `GameComponent` singleton that owns the active
session, pumps it every frame (`NetworkSession.Update()`), shows join/leave
messages through `MessageDisplayComponent`, and tears the session down when
the lobby is left.

### 2.5 Gameplay entities

Root-level classes: `Game` (entry point + F11 fullscreen toggle) and
`GameVariables` (global tunables). `Handlers/CollisionsHandler` performs the
collision pass between ships, asteroids and projectiles; `Particles/`
(`Particle`, `ParticleSystem`) renders explosions and trails.
`Entities/ScrollingBackground` is the level 2+ Raptor-style backdrop: it
stretches the starfield texture to the viewport width and loops it top → down
with linear wrapping (the gameplay screen draws it with
`SamplerState.LinearWrap`).

### 2.6 Audio — `Handlers/SoundHandler.cs`

The original game used **XACT** (`Content/Audio/GalactikWars.xap`, cues such
as `music`, `music1`–`music5`, `gameOver`, `engine_1`, `engine_2` and the
effect cues). MonoGame's XACT support is limited, so the audio stack was
rewritten on top of plain **`SoundEffect`**:

- All `.wav` files are imported directly by the MGCB `WavImporter` and loaded
  as `SoundEffect`.
- A single **shared** `SoundHandler` lives on `ScreenManager`
  (`ScreenManager.SoundHandler`): it is loaded once in
  `ScreenManager.LoadContent` and driven every frame from
  `ScreenManager.Update`, so music plays across every screen (menus,
  lobby, gameplay) without each screen owning its own copy.
- **Music is organized as named tracks** — one per game state, selected with
  `PlayTrack(...)` (idempotent; tracks loop until another is selected):

  | Track constant   | File                    | Plays in                    |
  |------------------|-------------------------|-----------------------------|
  | `MenuTrack`      | `menu.wav`              | main menu                   |
  | `LoadingTrack`   | `loading.wav`           | loading screens             |
  | `BattleTrack`    | `battle.wav`            | gameplay (both modes)       |
  | `LobbyTrack`     | `slow-travel.wav`       | multiplayer session screens |
  | `GameOverTrack`  | `in-the-wreckage.wav`   | when all lives are lost     |
  | `CreditsTrack`   | `meet-the-princess.wav` | credits screen              |

- A small `Cue` wrapper class reproduces the XACT cue API used by the game
  (`Play`, `Pause`, `Resume`, `Stop`, `IsPlaying`, `IsPaused`), including
  looping engine sounds implemented as a played-then-restarted effect.
- Effect cue names are preserved, so all gameplay call sites kept their
  original XACT string identifiers. The `.xap` file was deleted.

## 3. Networking — `GalactikWars.Net`

### 3.1 Why

XNA 2.0 multiplayer was built on **Xbox LIVE** (`Microsoft.Xna.Framework.Net`
+ `GamerServices` + `Guide.ShowSignIn`); those services cannot be reimplemented
on open platforms. The port replaces the *services* while keeping the *API*:
`GalactikWars.Net` re-implements the `NetworkSession` / `NetworkGamer` /
`AvailableNetworkSession` / `SignedInGamer` surface that the original screens
consume, over plain UDP sockets.

### 3.2 Design

- **Transport**: UDP. A host binds `0.0.0.0:26900`; clients bind any port.
- **Discovery**: hosts broadcast a discovery beacon every second; a client
  looking for sessions broadcasts a discovery request (2 s window) and hosts
  answer with a unicast reply. Discovered sessions appear as
  `AvailableNetworkSession` entries in the join screen.
- **Join**: client sends a `JoinRequest`; host answers OK (with a gamer id) or
  a failure reason (full, closed, …).
- **Topology**: star. Clients only talk to the host; the host relays game data
  packets to all other gamers. All traffic is length-prefixed binary packets
  (`Packet.cs`).
- **Session state**: the host maintains the roster and broadcasts it
  (`Roster` packets every 500 ms). Clients signal `Ready`; the host sends
  `StartGame`, and either side can send `EndGame`, `Kick`, or `Leave`.
- **Liveness**: heartbeat ping/pong; a gamer is dropped after 5 s of silence
  (`GamerTimeout`), the host after 5 s (`HostTimeout`) at which point clients
  surface a `NetworkException`.
- **Game data**: `Data` packets carry the per-frame game state blobs that
  `GameplayScreenMultiplayer` exchanges; each gamer is identified by an id,
  display name and a procedurally generated "gamer picture" texture
  (XNA used LIVE profile pictures; the port draws simple deterministic
  avatars).
- **Host migration** is accepted by the API surface (the session object can
  change host) but is not currently orchestrated by the protocol.

### 3.3 Protocol summary

| Packet | Direction | Purpose |
| --- | --- | --- |
| `DiscoveryRequest` | client → broadcast | find sessions |
| `DiscoveryReply` | host → client | unicast answer with session info |
| `JoinRequest` / `JoinReplyOk` / `JoinReplyFail` | client ↔ host | join handshake |
| `Roster` | host → clients | membership + ready state, 500 ms interval |
| `Ready` | client → host | lobby ready flag |
| `StartGame` | host → clients | launch gameplay |
| `EndGame` / `Kick` / `Leave` | both | session lifecycle |
| `Data` | host ↔ clients | gameplay state (relayed by host) |
| `Ping` / `Pong` | both | heartbeat, 5 s timeout |

Max 6 gamers per session (`MaxGamers`), matching the original constants
surfaced through `NetworkSessionComponent`.

## 4. Content pipeline

`assets/Content.mgcb` is the single content project
(replacing the old `Content.contentproj` + XNA content build). It sits in the
`assets/` folder at the repository root — all raw assets live outside the
source tree — and its paths are relative to that folder. It imports:

- **Models** — 11 DirectX `.x` meshes under `assets/models/`: the 4 original
  asteroids plus the 7 ships/sphere converted from FBX (see §5). Built with
  the `XImporter`.
- **Textures** — `.png`, `.tga` and `.dds` files under `assets/textures/`
  (plus UI textures under `assets/ui/`), built with their respective
  importers.
- **Fonts** — 3 spritefonts under `assets/fonts/` (`gamefont`, `menufont`,
  `LucidaConsole`). The original `Lucida Console.spritefont` was renamed to
  `LucidaConsole.spritefont` (the MGCB parser does not strip quotes from
  `/build:` arguments, and the space broke the path).
- **Audio** — 18 `.wav` files under `assets/audio/Effects/` and
  `assets/audio/Music/`, imported as `SoundEffect` (music + effects).

Asset names at runtime mirror the folder layout (`models/p1_pencil`,
`textures/starfield`, `fonts/LucidaConsole`, `audio/Effects/explosion1`,
`audio/Music/battle`).

The MGCB tool runs as a **local dotnet tool** (`.config/dotnet-tools.json`,
pinned to 3.8.6.1) and is invoked automatically by
`MonoGame.Content.Builder.Task` during `dotnet build`.

## 5. Model conversion — `tools/FbxToXConverter`

The original ship models (`p1_pencil`, `p1_saucer`, `p1_wedge`, `p2_*`,
`sphere`) are ASCII **FBX 6.0** files exported from Maya in 2008. MonoGame's
FBX importer (Assimp-based) only reads FBX 2011+ and cannot parse these, so a
small converter tool lives in `tools/FbxToXConverter/`:

- Parses the FBX 6.0 ASCII structure (mesh data is embedded in the
  `Model … "Mesh"` node; arrays are inline, spanning multiple lines).
- Unrolls polygon-vertex attributes (normals `ByVertice`/`ByPolygonVertex`,
  UVs `Direct`/`IndexToDirect`) into a split vertex list.
- Fan-triangulates polygons.
- Bakes the owning node transform into a `FrameTransformMatrix` so the bone
  hierarchy matches what the original XNA pipeline produced.
- Writes a DirectX `.x` text file next to the source, referencing the model
  texture from the `Textures` folder, so it can be built by the MGCB
  `XImporter`.

Usage:

```sh
dotnet run --project tools/FbxToXConverter -- assets/models/p1_pencil.fbx
```

The `.fbx` sources are kept in the repository for completeness but are not
referenced by `Content.mgcb`.

## 6. Repository infrastructure

- **Central package management**: `src/Directory.Packages.props` pins
  `MonoGame.Framework.DesktopGL` and `MonoGame.Content.Builder.Task` at
  3.8.6.1; the game project uses version-less `PackageReference`s.
- **Package sources**: `nuget.config` clears inherited feeds (machines with
  extra private feeds would otherwise fail NU1507) and maps all packages to
  nuget.org.
- **Solution**: `src/GalactikWars.slnx` (XML solution format) replaces the
  old `.sln`.

## 7. Porting notes (XNA 2.0 → MonoGame 3.8.6.1)

The mechanical differences encountered during the port, useful when touching
ported code:

| XNA 2.0 | MonoGame 3.8.6.1 |
| --- | --- |
| `SpriteBatch.Begin(SpriteBlendMode…)` | `Begin(SpriteSortMode…, BlendState…)` — sort mode **first** |
| `SpriteBlendMode.None/AlphaBlend/Additive` | `BlendState.Opaque/AlphaBlend/Additive` |
| `new Color(int, int, int, …)` mixed int/byte calls | Ambiguous between `(int…)` and `(byte…)` ctors — byte literals must be cast |
| `GameTime.TotalRealTime` | Removed; use `TotalGameTime` / `ElapsedGameTime` |
| `GameTime` 4-arg ctor | 3-arg ctor `(total, elapsed, isRunningSlowly)` |
| `Microsoft.Xna.Framework.Net` | `GalactikWars.Net` (this repo) |
| `Microsoft.Xna.Framework.GamerServices`, `Guide.ShowSignIn` | Removed; the old Live/System Link menu entries were merged into a single **Multiplayer** entry that goes straight to the session screens |
| `Microsoft.Xna.Framework.Storage` | Removed; no save-data usage |
| `GamerPrivilegeException` | Removed from all `catch` blocks |
| XACT (`XactComponent`, cues from `.xap`) | `SoundEffect` + `Cue` wrapper in `SoundHandler` |
| FBX 6.0 models | Converted to `.x` via `tools/FbxToXConverter` |
| `.contentproj` / `.sln` / `.suo` | `Content.mgcb` / `.slnx` / none |
| `Texture2D` from LIVE gamer pictures | Procedurally drawn avatars in `Net/NetworkSession.cs` |

## 8. Original documentation

`docs/relatorio1` and `docs/relatorio2` contain the original 2008 project
reports (LaTeX + PDF) with screenshots and design discussion from the
authors of the original game.
