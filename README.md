# 🚀 Galactik Wars

> **A 3D multiplayer space combat game, rebuilt for modern PCs.**

Fly hand-drawn "spaceships" — a pencil, a saucer and a wedge — through an
asteroid field, firing bullets, rockets and atom bombs at each other while
dodging, zooming and switching between 2D and 3D camera views.

The game was originally written in 2008 for **XNA Game Studio 2.0**
(Windows + Xbox 360, with multiplayer over Xbox LIVE). This repository is a
complete port to the modern, open-source **MonoGame 3.8.6.1** framework
running on **.NET 10**, with Xbox LIVE replaced by a self-contained UDP LAN
implementation.

[Back to Table of Contents](#toc)

---

## 📑 <a name="toc">Table of Contents

1. [📸 Overview](#overview)
2. [✨ Features](#features)
3. [🎮 Gameplay Loop](#gameplay-loop)
4. [🚀 Ships & Weapons](#ships--weapons)
5. [☄️ Asteroids & Hazards](#asteroids--hazards)
6. [🎮 Controls](#controls)
7. [🗂️ Project Architecture](#project-architecture)
8. [🚀 Installation](#installation)
9. [🌐 Multiplayer on a LAN](#multiplayer-on-a-lan)
10. [🧭 Development Roadmap](#development-roadmap)
11. [🎨 Heritage & Inspiration](#heritage--inspiration)
12. [📬 License, Usage & Contact](#contact)

---

## 📸 <a name="overview">Overview

**Galactik Wars** is a session-based space combat game: destroy everything
that moves, survive the asteroid field, and outfly your opponents.

In single-player you are alone against an endless asteroid field. In
multiplayer you battle other pilots over a local network — every machine on
the LAN runs its own game and they find each other automatically, no central
server required.

![Galactik Wars gameplay](docs/relatorio1/images/game.jpg)

[Back to Table of Contents](#toc)

---

## ✨ <a name="features">Features

* 🛸 **Single-player survival** — fly through an endless asteroid field and progress through levels as they are cleared
* 🌐 **LAN multiplayer for up to 6 players** — host a session, discover sessions automatically, join, ready up in the lobby and fly
* 🖊️ **Three ship hulls** — pencil, saucer and wedge, each with a player-1 and player-2 paint scheme
* 🔫 **Bullets** — fast, unlimited pea-shooter fire
* 🚀 **Homing rockets** — heavier ordnance that seeks its target
* 💥 **Atom bomb** — the big red button of space combat
* 🛡️ **Shields** — absorb collisions so you live to ram another day
* 🌀 **Hyperspace respawn** — re-enter the fight with a flash
* 🎆 **Particle explosions** — debris, flashes and trails everywhere
* 🎥 **Dynamic camera** — 2D top-down and 3D views with zoom in/out
* 🌌 **Scrolling starfield background** with aurora, galaxy and nebula layers
* 🎵 **A full soundtrack** — six music tracks, one per game state (menu, loading, battle, lobby, game over, credits), plus engine loops and combat sound effects
* 🪟 **Modern window** — resizable with maximize, and `F11` for fullscreen

[Back to Table of Contents](#toc)

---

## 🎮 <a name="gameplay-loop">Gameplay Loop

```text
Main Menu
        ↓
Single Player ──────────────┐         Multiplayer
        ↓                   │               ↓
Fly the Asteroid Field      │    Create or Find a Session
(Shoot Asteroids)           │               ↓
        ↓                   │            Lobby
Level Cleared               │     (ready up with the others)
        ↓                   │               ↓
Next Level ─────────────→ repeat         Battle
        ↓                   │     (shoot pilots, dodge asteroids)
Out of Lives                │               ↓
        ↓                   │     Return to the Main Menu
Game Over ──────────────────┘
```

[Back to Table of Contents](#toc)

---

## 🚀 <a name="ships--weapons">Ships & Weapons

### Ships

| Hull | Description |
| ------ | ----------- |
| 🖊️ Pencil | The classic — draws its own contrail across the sky |
| 🛸 Saucer | Round, saucy, hard to hit on a good day |
| 🔺 Wedge | All attitude, all angles |

Each hull comes in two paint schemes (player 1 and player 2). A hull is
picked at random at the start of each match.

### Weapons & Systems

| System | Effect |
| ------ | ------ |
| 🔫 Bullets | Rapid pea-shooter fire, unlimited ammo |
| 🚀 Rockets | Slower, heavier, homing |
| 💥 Atom bomb | One press, one very bad day for everything nearby |
| 🛡️ Shields | Soak up collisions; regenerated over time |
| 🌀 Hyperspace | Destroyed pilots re-enter the battle with a hyperspace jump |

[Back to Table of Contents](#toc)

---

## ☄️ <a name="asteroids--hazards">Asteroids & Hazards

| Hazard | Description |
| ------ | ----------- |
| ☄️ Large asteroids | Drift through the arena; split when destroyed |
| 🪨 Small asteroids | The debris that keeps on giving |
| 🚀 Other pilots | In multiplayer, everyone else is the main threat |
| 💣 Atom bombs | Yours are great. Everyone else's are not |

Collisions hurt: ships bounce, lose their phase and take shield damage. When
the shields are gone, so are you — until you respawn.

[Back to Table of Contents](#toc)

---

## 🎮 <a name="controls">Controls

### Keyboard

| Key | Action |
| --- | ------ |
| `↑` / `↓` | Move ship |
| `←` / `→` | Rotate ship |
| `Z` | Fire bullets |
| `X` | Fire rockets |
| `C` | Atom bomb |
| `Q` | Respawn after being destroyed |
| `F1` / `F2` | Switch camera (2D top-down / 3D) |
| `E` / `W` | Zoom in / zoom out |
| `F11` | Toggle fullscreen |
| `ESC` | Pause menu |

### Mouse

| Input | Action |
| ----- | ------ |
| Move mouse | Steer the ship |
| Left button | Fire bullets |
| Right button | Fire rockets |
| Middle button | Atom bomb |

The pause menu, options (sound), controls help and credits are all reachable
from the main menu.

[Back to Table of Contents](#toc)

---

## 🗂️ <a name="project-architecture">Project Architecture

All game assets live in `assets/` (outside the source tree), driven by a
single MonoGame content project (`assets/Content.mgcb`). Source code is
organized by responsibility:

```text
Galactik-Wars/
├── assets/                        # all raw assets + Content.mgcb
│   ├── audio/Effects, audio/Music # sound effects and music WAVs
│   ├── fonts/                     # spritefonts
│   ├── models/                    # DirectX .x meshes (+ original .fbx sources)
│   ├── textures/                  # texture files
│   └── ui/                        # UI textures (backgrounds, lobby icons)
├── src/
│   ├── GalactikWars.slnx          # solution
│   ├── Directory.Packages.props   # central package management (MonoGame 3.8.6.1)
│   └── GalactikWars/              # the game project (net10.0, DesktopGL)
│       ├── Handlers/              # SoundHandler + CollisionsHandler
│       ├── Entities/              # Ship, Asteroid, Bullet, Rocket, Player,
│       │                           # CCamera, ScrollingBackground
│       ├── Net/                   # LAN networking library (XNA NetworkSession API)
│       ├── Networking/            # session/lobby screens
│       ├── Particles/             # particle engine
│       ├── ScreenManager/         # game screen infrastructure + input
│       └── Screens/               # menus, loading, gameplay screens
├── tools/
│   └── FbxToXConverter/           # FBX 6.0 → DirectX .x model converter
├── AI.md                          # playbook for AI coding agents working on this repo
├── ARCHITECTURE.md                # detailed project architecture & porting notes
├── LICENSE                        # license details
└── README.md                      # main project documentation
```

For a complete breakdown of the runtime design and the port from XNA, see
[ARCHITECTURE.md](ARCHITECTURE.md).

[Back to Table of Contents](#toc)

---

## 🚀 <a name="installation">Installation

### 1. Clone the repository

```bash
git clone https://github.com/g-amador/Galactik-Wars.git
cd Galactik-Wars
```

### 2. Install the requirements

* The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* A GPU with OpenGL support (the game uses the DesktopGL backend)

Everything else — MonoGame packages and all game assets — is already in the
repository. Nothing to download or configure.

### 3. Restore, build and launch

```bash
dotnet tool restore                    # restores the local MGCB content-build tool
dotnet build src/GalactikWars.slnx
dotnet run --project src/GalactikWars
```

The first build runs the MonoGame content pipeline over `assets/Content.mgcb`
and compiles all textures, models, fonts and sounds into the game output.

[Back to Table of Contents](#toc)

---

## 🌐 <a name="multiplayer-on-a-lan">Multiplayer on a LAN

The original build used **Xbox LIVE** (`NetworkSession`, `GamerServices` and
`Guide.ShowSignIn`), which does not exist on open platforms. The port replaces
it with a self-contained networking stack (`GalactikWars.Net`) that keeps the
same programming API but works over plain UDP on the local network:

* Sessions are hosted on **UDP port 26900**.
* Clients discover sessions via broadcast beacons — no fixed server needed.
* One machine acts as the host and relays game data between all players
  (star topology).

To play: start the game on two or more machines on the same local network,
then choose **Multiplayer** from the main menu. The first player creates a
session; everyone else picks **Find Sessions** and joins. Ready up in the
lobby and fly.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full networking design.

[Back to Table of Contents](#toc)

---

## 🧭 <a name="development-roadmap">Development Roadmap

### ✅ Completed

* [x] Full port from XNA Game Studio 2.0 to MonoGame 3.8.6.1 on .NET 10
* [x] Xbox LIVE multiplayer replaced with a UDP LAN implementation
  (`NetworkSession` API preserved)
* [x] All assets moved into an `assets/` folder outside the source tree
* [x] Music game-wide — named tracks per game state with volume controls
* [x] Resizable window with maximize and an `F11` fullscreen toggle
* [x] Source tree reorganization (namespaces match folders)
* [x] Documentation: `ARCHITECTURE.md` and `AI.md`

### 🔮 Planned

* [ ] Better explosions with layered particle effects, and the atom bomb as an expanding EMP-style shockwave ring fired from the ship
* [ ] More multiplayer modes: up to 3 game instances on one PC with distinct controls per player, and AI-controlled opponents in multiplayer
* [ ] Show all controls on the pause screen
* [ ] Rebindable controls with a "reset to default" option in the menus
* [ ] Gamepad support
* [ ] A 2.5D view — tilted top-down perspective on the 3D camera
* [ ] A NeonShooter-style top-down 2D mode for single-player: neon vector ships with bloom, intense additive particles and a warping grid background
* [ ] Ship selection with per-ship advantages and a feature comparison screen

[Back to Table of Contents](#toc)

---

## 🎨 <a name="heritage--inspiration">Heritage & Inspiration

While this port establishes its own identity, it stands on the shoulders of:

* The original **2008 Galactik Wars** project by GNPA & STX — its design
  documents and project reports live in `docs/`, and its hand-drawn art still
  flies in the game today
* The **Microsoft XNA networking samples**, whose screen management and
  session flow this project is modeled on
* The **MonoGame** community, for keeping XNA alive long after its time

[Back to Table of Contents](#toc)

---

## 📬 <a name="contact">License, Usage & Contact

Galactik Wars is released under the MIT License, which allows you to freely
use, modify, distribute, and build upon the project, including creating your
own assets, tools, ports, or derivative works.

You may not claim authorship of Galactik Wars or misrepresent modified
versions as the original project.

For full legal details, refer to the **[LICENSE](LICENSE)** file included
with this repository.

If you have questions, suggestions, bug reports, or wish to share your own
battles and modifications, feel free to get in touch:

📧 **[g.n.p.amador@gmail.com](mailto:g.n.p.amador@gmail.com)**

Good luck out there, pilot — and mind the asteroids.

[Back to Table of Contents](#toc)

---

> Built with ❤️, C#, MonoGame, a sprinkle of AI magic... and an asteroid field that never runs out of things to shoot.
