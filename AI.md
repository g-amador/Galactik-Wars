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
