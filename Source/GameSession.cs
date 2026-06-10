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
    private const string LevelPath = "Content/Levels/level01.txt";

    public readonly Random Rng = new();
    public readonly Paddle Paddle = new();
    public readonly Ball Ball = new();
    public readonly List<Brick> Bricks;
    public readonly List<PowerUp> PowerUps = new();
    public readonly ParticleSystem Particles = new();
    public readonly ScreenShake Shake;

    public int Score;
    public int Lives = StartingLives;

    // Cached HUD text: $"SCORE {Score}" allocates a new string every call, and
    // DrawHud runs every frame. Desktop GC absorbs this easily, but rebuilding
    // a string 60 times a second when it changes a few times a minute is the
    // kind of habit that matters on consoles — so cache it and rebuild only
    // when the value actually changed.
    private string _scoreText;
    private int _scoreTextValue = -1;

    public GameSession()
    {
        Shake = new ScreenShake(Rng);
        Bricks = LevelLoader.Load(LevelPath);
        Ball.AttachTo(Paddle);
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
    }

    public void DrawWorld(SpriteBatch spriteBatch)
    {
        foreach (Brick brick in Bricks)
            brick.Draw(spriteBatch);
        foreach (PowerUp powerUp in PowerUps)
            powerUp.Draw(spriteBatch);
        Paddle.Draw(spriteBatch);
        Ball.Draw(spriteBatch);
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

        // Lives as little paddle icons, arcade-style.
        for (int i = 0; i < Lives; i++)
            spriteBatch.DrawRect(
                new Rectangle(Screen.Width - 12 - (i + 1) * 32, 14, 26, 7),
                new Color(226, 226, 236));
    }
}
