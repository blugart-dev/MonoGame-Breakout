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

        spriteBatch.DrawRect(Bounds, body);
        // 2 px bevel: lighter top edge, darker bottom edge — cheap depth.
        spriteBatch.DrawRect(new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 2),
            Color.Lerp(body, Color.White, 0.35f));
        spriteBatch.DrawRect(new Rectangle(Bounds.X, Bounds.Bottom - 2, Bounds.Width, 2),
            Color.Lerp(body, Color.Black, 0.4f));
    }
}
