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
| Mouse / `←` `→` / `A` `D` | Move paddle |
| `Space` / Left click | Launch ball |
| `Enter` / Click (on game over) | Play again |
| `P` | Pause / resume (also auto-pauses when the window loses focus) |
| `M` | Music on / off |
| `F11` | Borderless fullscreen |
| `F10` | Integer ("pixel perfect") scaling toggle |
| `F3` | Debug overlay (FPS, ball speed, entity counts) |
| `Esc` | Quit |

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

- Classic loop: Ready → Playing → LifeLost → GameOver (plus Pause and
  LevelCleared), 3 lives, score + level HUD
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
  bindings live in one runtime-rebindable dictionary (`ActionMap`)
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
  GameSession.cs       the world: entities, score, lives, level index
  Entities/            Paddle, Ball, Brick, PowerUp
  Systems/             input + action map, collision, levels, virtual screen,
                       particles, shake, audio synth + music, debug overlay
  States/              GameState base + Ready/Playing/Pause/LifeLost/
                       LevelCleared/GameOver
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
