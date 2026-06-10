# Breakout — Learn MonoGame by Reading One Small Game

[![Build](https://github.com/blugart-dev/MonoGame-Breakout/actions/workflows/build.yml/badge.svg)](https://github.com/blugart-dev/MonoGame-Breakout/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A complete Breakout clone built with **MonoGame 3.8 (DesktopGL)**, written to be
*read*: every file carries pedagogical comments explaining the MonoGame idioms
it uses and the reasoning behind non-obvious choices. No prior game-engine
experience is assumed — the repo teaches MonoGame from first principles.

It is also entirely **asset-free**: every visual is a tinted 1×1 white texture,
every sound — the SFX *and* the looping music track — is synthesized at
startup, and the only content pipeline asset is the HUD font.

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

## Controls

| Input | Action |
|---|---|
| `↑` `↓` / `W` `S` (title) | Select mode |
| `←` `→` (title) | Pick starting level (modern / co-op) |
| Mouse / `←` `→` / `A` `D` | Move paddle (gamepad: D-pad / left stick) |
| `Space` / Left click / `A` button | Launch ball (modern) / serve (classic) |
| `Enter` / Click (on game over) | Play again |
| `T` (on game over) | Back to the title screen |
| `B` (while paused) | Key rebinding screen |
| `P` | Pause / resume (also auto-pauses when the window loses focus) |
| `M` | Music on / off |
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
4. **Study the feature commits.** The guide's original exercises (pause
   state, multiball, action map, …) are all implemented now — one commit
   each, so `git log --oneline` doubles as a reading list: check out any
   commit's diff to see exactly what one feature costs. Appendix C lists
   the next round of exercises.

## Features

- **Three ways to play, one engine.** A title screen (with level select)
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
- Arcade loop: Title → Ready → Playing → LifeLost → GameOver (plus Pause and
  LevelCleared), 3 lives/serves, score + level HUD
- Three levels as plain-text grids (`Content/Levels/*.txt`);
  tier digit = hit points = color, `X` = unbreakable; score and lives
  carry across levels, ball speed resets per board
- Paddle-position-controlled bounce angle (the aiming mechanic)
- Speed ramp per brick; power-up drops: wide paddle (pink) and
  multiball (cyan) — a life is lost only when the *last* ball drops
- Juice: brick-break particles, trauma-based screen shake, ball trail,
  synthesized SFX with random pitch variation
- Music: a synthesized chiptune loop on a looping `SoundEffectInstance` —
  ducks (drops low, doesn't stop) on pause and game over, `M` mutes
- Action map: gameplay reads named intents (`GameAction`), not keys —
  bindings live in runtime-rebindable dictionaries (`ActionMap`), with
  full gamepad support and an in-game key rebinding screen (`B` from pause)
- High scores: top five per mode persisted as JSON under the per-user
  `ApplicationData` folder; game over shows the table, title shows the best
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
  GameMode.cs          Modern vs Classic — which states drive the session
  GameSession.cs       the world: entities, score, lives, level/wall index
  Entities/            Paddle, Ball, Brick, PowerUp
  Systems/             input + action map (kbd/pad), collision, levels,
                       classic 1976 rules + wall, high scores, virtual
                       screen, particles, shake, audio synth + music,
                       debug overlay
  States/              GameState base + Title/Ready/Playing/ClassicReady/
                       ClassicPlaying/Pause/Rebind/LifeLost/LevelCleared/
                       GameOver
Content/
  Content.mgcb         pipeline manifest (font only)
  Fonts/Hud.spritefont rasterized at build time by the pipeline
  Levels/level0*.txt   runtime-loaded, copied via csproj — not pipelined
docs/index.html        the study guide — open in a browser (or via Pages)
```

## Commands

```bash
dotnet build    # compile (content pipeline runs as part of the build)
dotnet run      # play
dotnet format   # code style
```

## License

[MIT](LICENSE).
