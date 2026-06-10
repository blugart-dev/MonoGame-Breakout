using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.Entities;

public enum PowerUpType
{
    WidePaddle,
    Multiball,
}

/// <summary>
/// A falling pickup dropped by a destroyed brick. It only knows how to fall
/// and pulse; PlayingState detects the catch and applies the actual effect,
/// so adding a new power-up means a new enum member and one switch arm there.
/// </summary>
public class PowerUp
{
    public const float WidePaddleDuration = 10f;

    private const int Width = 26;
    private const int Height = 14;
    private const float FallSpeed = 130f;

    public PowerUpType Type { get; }
    public Vector2 Position; // center

    private float _age;

    public PowerUp(PowerUpType type, Vector2 position)
    {
        Type = type;
        Position = position;
    }

    public Rectangle Bounds => new(
        (int)(Position.X - Width / 2f), (int)(Position.Y - Height / 2f), Width, Height);

    public bool IsBelowScreen => Position.Y - Height / 2f > Screen.Height;

    public void Update(float dt)
    {
        Position.Y += FallSpeed * dt;
        _age += dt;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // With no sprites, color *is* the identity: pink ring = wide paddle,
        // cyan ring = multiball. Players learn a color code in one catch.
        Color ring = Type == PowerUpType.WidePaddle
            ? new Color(255, 80, 200)
            : new Color(80, 200, 255);

        // Pulse so it reads as "catch me", not as a falling brick.
        int pulse = (int)(MathF.Sin(_age * 8f) * 2f);
        Rectangle b = Bounds;
        b.Inflate(pulse, pulse / 2);
        spriteBatch.DrawRect(b, ring);
        b.Inflate(-3, -3);
        spriteBatch.DrawRect(b, Color.White);
    }
}
