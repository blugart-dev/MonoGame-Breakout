# Breakout — Learn MonoGame by Reading One Small Game

A complete Breakout clone built with **MonoGame 3.8 (DesktopGL)**, written to be
*read*: every file carries pedagogical comments explaining the MonoGame idioms
it uses and the reasoning behind non-obvious choices. No prior game-engine
experience is assumed — the repo teaches MonoGame from first principles.

It is also entirely **asset-free**: every visual is a tinted 1×1 white texture,
every sound is a square wave synthesized at startup, and the only content
pipeline asset is the HUD font.

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
| `F11` | Borderless fullscreen |
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
4. **Do the exercises** in the study guide's Appendix C — each is a real feature
   (second level, pause state, multiball, …), ordered by how much code it
   touches.

## Features

- Classic loop: Ready → Playing → LifeLost → GameOver, 3 lives, score HUD
- Bricks loaded from a plain-text grid (`Content/Levels/level01.txt`);
  tier digit = hit points = color, `X` = unbreakable
- Paddle-position-controlled bounce angle (the aiming mechanic)
- Speed ramp per brick, wide-paddle power-up drops
- Juice: brick-break particles, trauma-based screen shake, synthesized SFX
- Production layer: virtual 800×480 resolution rendered to a `RenderTarget2D`
  and letterboxed to any window size, resizable window, borderless
  fullscreen, pause-when-unfocused, debug overlay

## Project layout

```
Program.cs             entry point — two lines
BreakoutGame.cs        application shell: window, loop, two-pass draw
Source/
  Screen.cs            virtual resolution constants
  GameSession.cs       the world: entities, score, lives, effects
  Entities/            Paddle, Ball, Brick, PowerUp
  Systems/             input, collision, levels, virtual screen,
                       particles, shake, audio synth, debug overlay
  States/              GameState base + Ready/Playing/LifeLost/GameOver
Content/
  Content.mgcb         pipeline manifest (font only)
  Fonts/Hud.spritefont rasterized at build time by the pipeline
  Levels/level01.txt   runtime-loaded, copied via csproj — not pipelined
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
