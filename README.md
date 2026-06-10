# Breakout — Learn MonoGame by Reading One Small Game

[![Build](https://github.com/blugart-dev/MonoGame-Breakout/actions/workflows/build.yml/badge.svg)](https://github.com/blugart-dev/MonoGame-Breakout/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A complete Breakout clone built with **MonoGame 3.8 (DesktopGL)**, written to be
*read*: every file carries pedagogical comments explaining the MonoGame idioms
it uses and the reasoning behind non-obvious choices. No prior game-engine
experience is assumed — the repo teaches MonoGame from first principles.

It is also entirely **asset-free**: every visual is a tinted 1×1 white texture,
every sound — the SFX *and* the looping music track — is synthesized at
startup, and the only content pipeline assets are the HUD font and the CRT
shader (which is source code, not art).

> 📖 **[Read the study guide](https://blugart-dev.github.io/MonoGame-Breakout/)** —
> ten concept sections, a project map, an engine-concepts glossary, and
> exercises. (Source: [`docs/index.html`](docs/index.html), served by GitHub Pages.)

## Run

```bash
dotnet run
```

Requires the **.NET 9 SDK** (the project targets `net9.0`; `RollForward`
lets any newer runtime work). No other setup — NuGet restores MonoGame
automatically and the content pipeline runs as part of the build.

> **Linux note:** the HUD font is built from your installed *Arial*. On Linux,
> either install the MS core fonts (`ttf-mscorefonts-installer`) or change
> `FontName` in `Content/Fonts/Hud.spritefont` to a font you have.
> Also, compiling the CRT shader (`Content/Shaders/Crt.fx`) uses Direct3D's
> shader compiler, which on Linux runs under Wine — run MonoGame's
> [one-time setup script](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_ubuntu.html)
> (`net9_mgfxc_wine_setup.sh`). See `.github/workflows/build.yml` for the
> exact CI recipe.

## Controls

| Input | Action |
|---|---|
| `↑` `↓` / `W` `S` (title) | Select mode |
| `←` `→` (title) | Pick starting level or `RANDOM` endless boards (modern / co-op) |
| Mouse / `←` `→` / `A` `D` | Move paddle (gamepad: D-pad / left stick) |
| `Space` / Left click / `A` button | Launch ball (modern) / serve (classic, super) |
| `Enter` / Click (on game over) | Play again |
| `T` (on game over) | Back to the title screen |
| `R` / `X` button (on game over) | Watch a replay of the run (`T` stops it; `P` pauses it) |
| `B` (while paused) | Key rebinding screen |
| `P` | Pause / resume (also auto-pauses when the window loses focus) |
| `M` | Music on / off |
| `F9` | CRT shader (scanlines + curvature) on / off |
| `F11` | Borderless fullscreen |
| `F10` | Integer ("pixel perfect") scaling toggle |
| `F3` | Debug overlay (FPS, ball speed, entity counts) |
| `Esc` | Quit |

In **co-op**, player 1 holds the left half (mouse or `A`/`D`) and player 2
the right half (arrows, or a gamepad's D-pad/stick).

## How to learn from this repo

1. **Play it** (`dotnet run`) so you know what the code produces.
2. **Open the [study guide](https://blugart-dev.github.io/MonoGame-Breakout/)**
   (or `docs/index.html` locally in a browser).
   Ten concept sections in reading order (game loop, fixed timestep,
   SpriteBatch, content pipeline, input polling, AABB collision, state
   machines, virtual resolution, juice, production habits), each naming the
   files to open next to it, plus an engine-concepts glossary and exercises.
3. **Read the code** — the comments are the textbook. A good order:
   `Program.cs` → `BreakoutGame.cs` → `Source/States/PlayingState.cs`,
   then outward to whatever it touches.
4. **Study the feature commits.** Every exercise from the guide's three
   Appendix C rounds is implemented now — one commit each, so
   `git log --oneline` doubles as a reading list: check out any commit's
   diff to see exactly what one feature costs.

## Features

- **Six ways to play, one engine.** A title screen (with level select)
  picks the mode; a mode is just which states drive the shared entities
- **Modern mode:** three tiered boards, power-ups, multiball, continuous
  paddle aiming, per-brick speed ramp, staggered brick drop-in entrance
- **Co-op (2P):** modern rules with two paddles — P1 left half
  (mouse/`A`/`D`), P2 right half (arrows/gamepad); wide-paddle catches are
  personal, lives are shared
- **Classic 1976 mode:** the original arcade rules, reconstructed from
  Atari's operation manual — 8×14 one-hit wall scored 1/1/3/3/5/5/7/7 by row
  (448/wall), four discrete ball speeds (4th hit, 12th hit, instant max on
  orange/red), four fixed paddle rebound exits (never perpendicular), one
  brick per trip, half-width paddle after breaking through, hostile
  mid-screen serve, and exactly two walls: max score 896
- **Super Breakout 1978 modes:** the sequel's three games, from its operation
  manual (TM-118). Shared 1978 physics: five discrete speeds (4th/8th/12th
  return, instant max on a 5/7-point brick), the pass-through rule (an
  unreturned ball sails through bricks; after each kill the ball *bores* for
  four rows), paddle halves on top-boundary contact until the next serve, and
  scores multiply by balls in play. **Double:** two stacked paddles on one
  set of controls, two balls per serve (only the first costs a serve), ×2
  points while both fly. **Cavity:** two captive balls sealed in the wall
  escape through the holes you open and join play for ×2/×3 scoring.
  **Progressive:** an endless wall scrolls toward you, re-priced by screen
  zone (blue 7 → orange 5 → green 3 → yellow 1) every step
- Arcade loop: Title → Ready → Playing → LifeLost → GameOver (plus Pause and
  LevelCleared), 3 lives/serves, score + level HUD
- Three levels as plain-text grids (`Content/Levels/*.txt`);
  tier digit = hit points = color, `X` = unbreakable; score and lives
  carry across levels, ball speed resets per board
- **Procedural boards:** the level select's `RANDOM` slot plays endless
  generated boards — `BoardGenerator` emits the same text format
  `LevelLoader` parses, with difficulty knobs (rows, hit-point budget,
  unbreakable density, mirroring) scaling per board
- Paddle-position-controlled bounce angle (the aiming mechanic)
- Speed ramp per brick; power-up drops: wide paddle (pink), multiball
  (cyan) — a life is lost only when the *last* ball drops — and **falling
  debris**, the anti-power-up: a dark tumbling hazard that halves the
  paddle for eight seconds if you catch it
- **Replays:** every run records itself (RNG seed + one input frame per
  60 Hz tick); `R` on the game-over screen re-simulates it, `T` stops,
  `P` pauses — determinism as a feature
- Juice: brick-break particles, trauma-based screen shake, ball trail,
  synthesized SFX with random pitch variation
- Music: a synthesized chiptune loop on a looping `SoundEffectInstance` —
  ducks (drops low, doesn't stop) on pause and game over, `M` mutes
- Action map: gameplay reads named intents (`GameAction`), not keys —
  bindings live in runtime-rebindable dictionaries (`ActionMap`), with
  full gamepad support and an in-game key rebinding screen (`B` from pause)
- High scores: top five per table (each mode, with endless random runs
  scored separately) persisted as JSON under the per-user
  `ApplicationData` folder; game over shows the table, title shows the best
- **CRT shader** (`F9`): scanlines locked to virtual rows, barrel
  curvature and vignette in one HLSL `Effect`, applied where the frame is
  presented — the content pipeline's other asset type
- Production layer: virtual 800×480 resolution rendered to a `RenderTarget2D`
  and letterboxed to any window size, optional integer ("pixel perfect")
  scaling, resizable window, borderless fullscreen, pause state with
  auto-pause on focus loss, debug overlay

## Project layout

```
Program.cs             entry point — two lines
BreakoutGame.cs        application shell: window, loop, two-pass draw
Source/
  Screen.cs            virtual resolution constants
  GameMode.cs          Modern / Co-op / Classic / 3× Super — which states drive the session
  GameSession.cs       the world: entities, score, lives, level/wall index
  Entities/            Paddle, Ball, Brick, PowerUp (prizes + debris)
  Systems/             input + action map (kbd/pad), replay (snapshot + tape),
                       collision, levels + board generator, classic 1976
                       rules + wall, super 1978 rules + walls, high scores,
                       virtual screen (+ CRT pass), particles, shake,
                       audio synth + music, debug overlay
  States/              GameState base + Title/Ready/Playing/ClassicReady/
                       ClassicPlaying/SuperReady/SuperPlaying (abstract,
                       + Double/Cavity/Progressive)/Pause/Rebind/LifeLost/
                       LevelCleared/GameOver
Content/
  Content.mgcb         pipeline manifest (font + CRT shader)
  Fonts/Hud.spritefont rasterized at build time by the pipeline
  Shaders/Crt.fx       HLSL, compiled to a platform blob at build time
  Levels/level0*.txt   runtime-loaded, copied via csproj — not pipelined
Tests/
  Breakout.Tests/      xUnit suite for the pure logic (rules, generator, …)
docs/index.html        the study guide — open in a browser (or via Pages)
```

## Commands

```bash
dotnet build                      # compile (content pipeline runs as part of the build)
dotnet run                        # play
dotnet test Tests/Breakout.Tests  # unit tests (the pure game logic)
dotnet format                     # code style
```

## Tests

`Tests/Breakout.Tests` (xUnit) covers everything in the game that is a pure
function: the 1976/1978 rule tables (speed ladders, rebound angles, the
never-vertical law), the manual-sourced walls (the 448-point 1976 wall, the
Super walls and cavity holes, Progressive's scroll/re-pricing/feed pattern),
the board generator (determinism, format contract, mirroring, winnability),
the shipped level files (parsed exactly as the game parses them), AABB
collision response, the ball's aiming math, high-score ranking, and the
replay system's recorded-vs-live action contract.
The deliberate boundary is the lesson: game *rules* are unit-testable because
they are pure; the frame loop, rendering and feel are play-tested. CI runs the
suite on both OSes.

## License

[MIT](LICENSE).
