# Galactik Wars

Galactik Wars is a 3D multiplayer space combat game. Two players fly hand-drawn
"space ships" (a pencil, a saucer and a wedge) through an asteroid field,
firing bullets, rockets and atom bombs at each other while dodging, zooming
and switching between 2D and 3D camera views.

The game was originally written in 2008 for **XNA Game Studio 2.0** (Windows +
Xbox 360, using Xbox LIVE for multiplayer). This repository is a complete
port to the modern, open-source **MonoGame 3.8.6.1** framework running on
**.NET 10**, with the original Xbox LIVE networking replaced by a custom
UDP LAN implementation.

![Game screenshot](docs/relatorio1/images/game.jpg)

---

## Features

- **Single player** survival mode: fly through an endless asteroid field,
  shoot asteroids and progress through levels as they're cleared.
- **Multiplayer** over a local network (LAN) — up to 6 players, with lobby,
  session discovery, join/leave and host-side session management.
- Three ship hulls per player (pencil, saucer, wedge) with distinct silhouettes.
- Weapons: bullets, homing rockets and an atom bomb.
- Shields, hyperspace jumps, explosions and a full particle system.
- Scrolling starfield background, asteroid field, dynamic camera
  (2D top-down and 3D views, zoom in/out).
- Full soundtrack and sound effects.

## Controls (keyboard)

| Key | Action |
| --- | --- |
| Arrow keys | Move ship / rotate |
| `Z` (or left mouse) | Fire bullets |
| `X` (or right mouse) | Fire rockets |
| `C` (or middle mouse) | Atom bomb |
| `Q` | Respawn after being destroyed |
| `F1` / `F2` | Switch camera (top-down / angled 3D) |
| `E` / `W` | Zoom in / zoom out |
| `Esc` | Pause menu |

The pause menu, options (sound), controls help and credits are all reachable
from the main menu.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A GPU with OpenGL support (the game uses the DesktopGL backend).
- No other dependencies: MonoGame is restored automatically via NuGet.

## Build and run

From the repository root:

```sh
dotnet tool restore        # restores the local MGCB content-build tool
dotnet build src/GalactikWars.slnx
dotnet run --project src/GalactikWars
```

The first build runs the MonoGame content pipeline over
`src/GalactikWars/Content/Content.mgcb` and compiles all textures, models,
fonts and sounds into the game output.

## Project layout

```
├── AI.md                          Notes for AI coding agents working on this repo
├── ARCHITECTURE.md                Detailed architecture and porting documentation
├── LICENSE
├── nuget.config                   Package sources + package source mapping
├── .config/dotnet-tools.json      Local MGCB tool manifest
├── src/
│   ├── GalactikWars.slnx          Solution
│   ├── Directory.Packages.props   Central package management (MonoGame 3.8.6.1)
│   └── GalactikWars/              The game project (net10.0, DesktopGL)
│       ├── Content/               Content project (Content.mgcb) + raw assets
│       ├── Handlers/              Collisions and sound handlers
│       ├── Net/                   LAN networking library (XNA NetworkSession API)
│       ├── Networking/            Session/lobby screens
│       ├── ParticleSystem/        Particle engine
│       ├── ScreenManager/         Game screen infrastructure + input
│       └── Screens/               Menus, loading, gameplay screens
├── tools/
│   └── FbxToXConverter/           FBX 6.0 → DirectX .x model converter
└── docs/                          Original 2008 project reports
```

## Multiplayer on a LAN

The original build used **Xbox LIVE** (`NetworkSession`, `GamerServices` and
`Guide.ShowSignIn`), which does not exist on open platforms. The port replaces
it with a self-contained networking stack (`GalactikWars.Net`) that keeps the
same programming API but works over plain UDP on the local network:

- Sessions are hosted on **UDP port 26900**.
- Clients discover sessions via broadcast beacons (no fixed server needed).
- One machine acts as the host and relays game data between all players
  (star topology).

Just start the game on two or more machines on the same local network and use
**Multiplayer** from the main menu.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full design and the porting
details, and [AI.md](AI.md) if you are an AI coding agent working in this
repository.

## License

See [LICENSE](LICENSE).
