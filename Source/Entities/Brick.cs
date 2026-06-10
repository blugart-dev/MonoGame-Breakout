using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.Entities;

/// <summary>
/// One brick. In the modern game a single number from the level file (the
/// "tier", 1-5) drives everything — hit points, color, and score value; the
/// classic 1976 wall instead scores by *row*, so the general constructor takes
/// the three values independently and the tier constructor is just a preset.
/// Bricks never move, so the bounds rectangle is computed once and stays
/// immutable.
/// </summary>
public class Brick
{
    // Index 0 is unused so a tier digit from the level file indexes directly.
    private static readonly Color[] TierColors =
    {
        Color.Transparent,
        new Color(94, 167, 255),   // 1 — blue
        new Color(87, 212, 118),   // 2 — green
        new Color(255, 205, 66),   // 3 — yellow
        new Color(255, 138, 48),   // 4 — orange
        new Color(255, 77, 77),    // 5 — red
    };
    private static readonly Color UnbreakableColor = new(128, 130, 140);

    public Rectangle Bounds { get; }
    public bool IsUnbreakable { get; }
    public int HitPoints { get; private set; }
    public int ScoreValue { get; }
    public Color BaseColor { get; }

    private readonly int _maxHitPoints; // for the damage tint in Draw

    // Entrance animation: a *draw-time* offset only. Bounds (and therefore
    // collision) never move — animating the presentation while the simulation
    // stays put is the cheap, safe way to do entrance effects. The window
    // where a launched ball could meet a still-falling brick is shorter than
    // the ball's flight time to the wall, so the mismatch is unobservable.
    private const float DropDuration = 0.5f;
    private float _dropDelay;
    private float _dropTimer;
    private bool _dropping;

    public bool Alive => IsUnbreakable || HitPoints > 0;

    /// <summary>Modern preset: tier = hit points = color, score = tier x 10.</summary>
    public Brick(Rectangle bounds, int tier)
        : this(bounds, hitPoints: tier, scoreValue: tier * 10, TierColors[tier]) { }

    public Brick(Rectangle bounds, int hitPoints, int scoreValue, Color color)
    {
        Bounds = bounds;
        HitPoints = hitPoints;
        _maxHitPoints = hitPoints;
        ScoreValue = scoreValue;
        BaseColor = color;
    }

    private Brick(Rectangle bounds)
    {
        Bounds = bounds;
        IsUnbreakable = true;
        BaseColor = UnbreakableColor;
    }

    public static Brick Unbreakable(Rectangle bounds) => new(bounds);

    /// <summary>Begin the drop-in entrance after the given stagger delay.</summary>
    public void StartDropIn(float delay)
    {
        _dropDelay = delay;
        _dropTimer = 0f;
        _dropping = true;
    }

    public void UpdateDropIn(float dt)
    {
        if (!_dropping)
            return;
        _dropTimer += dt;
        if (_dropTimer >= _dropDelay + DropDuration)
            _dropping = false;
    }

    /// <summary>
    /// Tween-style animation without a tween library: progress 0→1 over a
    /// fixed duration, shaped by an easing function, applied as an offset.
    /// Cubic ease-out (1 - (1-t)³) starts fast and lands soft — the standard
    /// "object arriving" curve. Compare with the paddle's width lerp, which
    /// chases a target forever; this one has a beginning and an end.
    /// </summary>
    private float DropOffsetY
    {
        get
        {
            if (!_dropping)
                return 0f;
            float t = MathHelper.Clamp((_dropTimer - _dropDelay) / DropDuration, 0f, 1f);
            float eased = 1f - MathF.Pow(1f - t, 3f);
            return -(Bounds.Bottom + 8) * (1f - eased); // start fully above the screen
        }
    }

    /// <returns>true if this hit destroyed the brick.</returns>
    public bool Hit()
    {
        if (IsUnbreakable)
            return false;
        HitPoints--;
        return HitPoints <= 0;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color body = BaseColor;
        if (!IsUnbreakable && HitPoints < _maxHitPoints)
        {
            float damage = 1f - (float)HitPoints / _maxHitPoints;
            body = Color.Lerp(body, Color.Black, damage * 0.45f);
        }

        Rectangle drawRect = Bounds;
        drawRect.Y += (int)DropOffsetY;

        spriteBatch.DrawRect(drawRect, body);
        // 2 px bevel: lighter top edge, darker bottom edge — cheap depth.
        spriteBatch.DrawRect(new Rectangle(drawRect.X, drawRect.Y, drawRect.Width, 2),
            Color.Lerp(body, Color.White, 0.35f));
        spriteBatch.DrawRect(new Rectangle(drawRect.X, drawRect.Bottom - 2, drawRect.Width, 2),
            Color.Lerp(body, Color.Black, 0.4f));
    }
}
