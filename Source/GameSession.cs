using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Entities;
using Breakout.Systems;

namespace Breakout;

/// <summary>
/// Everything one run of the game owns: the entities, the score, the lives,
/// the effects. States (Ready/Playing/...) are *modes of operating on* this
/// data — the data lives here so switching state never moves the world around.
/// Starting a new game is simply `new GameSession()`.
/// </summary>
public class GameSession
{
    public const int StartingLives = 3;

    // The run's level sequence. Adding a level is two steps: drop the .txt
    // into Content/Levels and list its path here — nothing else changes.
    private static readonly string[] LevelPaths =
    {
        "Content/Levels/level01.txt",
        "Content/Levels/level02.txt",
        "Content/Levels/level03.txt",
    };

    public static int LevelCount => LevelPaths.Length; // the title screen's level select needs the range

    public readonly Random Rng = new();
    public readonly Paddle Paddle = new();

    // A list, not a single Ball — the multiball power-up was the feature that
    // forced this. Worth noticing how far the one-to-many change rippled:
    // every consumer that said "the ball" (serve, collision, loss check,
    // debug overlay) had to decide what it means when there are several.
    public readonly List<Ball> Balls = new();

    public List<Brick> Bricks { get; private set; }
    public readonly List<PowerUp> PowerUps = new();
    public readonly ParticleSystem Particles = new();
    public readonly ScreenShake Shake;

    public int Score;
    public int Lives = StartingLives;
    public int LevelIndex { get; private set; } // 0-based; the HUD shows +1

    public GameMode Mode { get; }
    public int StartLevelIndex { get; } // modern: where the title screen started this run

    // Classic-mode wall progression. The 1976 game is exactly two walls: when
    // the first is cleared, AwaitingSecondWall arms, and the wall materializes
    // mid-volley on the ball's next paddle-or-backwall contact (the manual's
    // rule — no interstitial, no new serve).
    public int WallNumber { get; private set; } = 1;
    public bool AwaitingSecondWall { get; set; }

    public bool HasNextLevel => LevelIndex + 1 < LevelPaths.Length;

    // Cached HUD text: $"SCORE {Score}" allocates a new string every call, and
    // DrawHud runs every frame. Desktop GC absorbs this easily, but rebuilding
    // a string 60 times a second when it changes a few times a minute is the
    // kind of habit that matters on consoles — so cache it and rebuild only
    // when the value actually changed.
    private string _scoreText;
    private int _scoreTextValue = -1;
    private string _levelText;
    private int _levelTextValue = -1;

    public GameSession(GameMode mode, int startLevel = 0)
    {
        Mode = mode;
        StartLevelIndex = startLevel;
        LevelIndex = startLevel;
        Shake = new ScreenShake(Rng);
        Bricks = mode == GameMode.Classic
            ? ClassicWall.Build()
            : LevelLoader.Load(LevelPaths[startLevel]);

        if (mode == GameMode.Classic)
            PrepareClassicServe();
        else
            ResetForServe();
    }

    /// <summary>
    /// Back to exactly one ball, attached to the paddle — the serve position.
    /// Runs on every ReadyState entry, which is what cleans up leftover
    /// multiballs after a lost life (Balls may be empty) or a cleared level
    /// (Balls may hold several).
    /// </summary>
    public void ResetForServe()
    {
        EnsureSingleBall();
        Balls[0].AttachTo(Paddle);
    }

    /// <summary>
    /// The classic counterpart: the 1976 ball is never carried on the paddle —
    /// it materializes mid-screen on serve — so between serves the ball is
    /// parked off-screen. The paddle also recovers from the half-width
    /// breakout penalty here: the manual ties the penalty to the volley.
    /// </summary>
    public void PrepareClassicServe()
    {
        EnsureSingleBall();
        Balls[0].Park();
        Paddle.ResetWidth();
    }

    private void EnsureSingleBall()
    {
        if (Balls.Count == 0)
            Balls.Add(new Ball());
        else if (Balls.Count > 1)
            Balls.RemoveRange(1, Balls.Count - 1);
    }

    /// <summary>Classic only: restore the full 8x14 wall as wall two.</summary>
    public void SpawnSecondWall()
    {
        WallNumber = 2;
        AwaitingSecondWall = false;
        Bricks = ClassicWall.Build();
    }

    /// <summary>
    /// Load the next board. Score and lives carry across — the *run* is the
    /// unit of play, a level is just the current wall. Ball speed does not
    /// carry: ReadyState re-attaches the ball, which resets the ramp, so each
    /// level climbs from base speed again instead of starting at the ceiling.
    /// That asymmetry (keep progress, reset difficulty) is the standard arcade
    /// answer to the carry-or-reset design question.
    /// </summary>
    public void AdvanceLevel()
    {
        LevelIndex++;
        PowerUps.Clear(); // falling pickups belong to the previous board
        Bricks = LevelLoader.Load(LevelPaths[LevelIndex]);
    }

    // Destroyed bricks are removed from the list, so "cleared" means only
    // unbreakable ones remain.
    public bool LevelCleared => Bricks.All(b => b.IsUnbreakable);

    /// <summary>
    /// Effects animate in every state — particles keep falling and the shake
    /// keeps settling even on the game-over screen.
    /// </summary>
    public void UpdateEffects(float dt)
    {
        Particles.Update(dt);
        Shake.Update(dt);

        // The brick entrance is presentation, not simulation — it animates a
        // draw offset while Bounds stay put — so it ticks with the effects
        // (and therefore freezes correctly when PauseState freezes them).
        foreach (Brick brick in Bricks)
            brick.UpdateDropIn(dt);
    }

    public void DrawWorld(SpriteBatch spriteBatch)
    {
        foreach (Brick brick in Bricks)
            brick.Draw(spriteBatch);
        foreach (PowerUp powerUp in PowerUps)
            powerUp.Draw(spriteBatch);
        Paddle.Draw(spriteBatch);
        foreach (Ball ball in Balls)
            ball.Draw(spriteBatch);
        Particles.Draw(spriteBatch);
    }

    public void DrawHud(SpriteBatch spriteBatch, SpriteFont font)
    {
        if (_scoreTextValue != Score)
        {
            _scoreTextValue = Score;
            _scoreText = $"SCORE {Score}";
        }
        spriteBatch.DrawString(font, _scoreText, new Vector2(12, 8), Color.White);

        int levelValue = Mode == GameMode.Classic ? WallNumber : LevelIndex;
        if (_levelTextValue != levelValue)
        {
            _levelTextValue = levelValue;
            _levelText = Mode == GameMode.Classic
                ? $"CLASSIC - WALL {WallNumber}"
                : $"LEVEL {LevelIndex + 1}";
        }
        spriteBatch.DrawCenteredText(font, _levelText,
            new Vector2(Screen.Width / 2f, 16), new Color(150, 150, 165), 0.75f);

        // Lives as little paddle icons, arcade-style.
        for (int i = 0; i < Lives; i++)
            spriteBatch.DrawRect(
                new Rectangle(Screen.Width - 12 - (i + 1) * 32, 14, 26, 7),
                new Color(226, 226, 236));
    }
}
