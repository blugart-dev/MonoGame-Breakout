using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakout.Systems;

namespace Breakout.Entities;

/// <summary>
/// The player's paddle. Stored as a center X plus a width (not a rectangle)
/// because both input and the wide power-up think in terms of "where is the
/// middle" and "how wide right now" — the Bounds rectangle is derived on read.
/// </summary>
public class Paddle
{
    public const int Height = 14;
    public const int Y = Screen.Height - 40;

    private const float BaseWidth = 100f;
    private const float WideWidth = 160f;
    private const float KeyboardSpeed = 520f;
    private const float WidthLerpSpeed = 10f;

    private float _centerX = Screen.Width / 2f;
    private float _width = BaseWidth;
    private float _wideTimer;

    public bool IsWide => _wideTimer > 0f;

    public Rectangle Bounds => new((int)(_centerX - _width / 2f), Y, (int)_width, Height);

    public void Update(float dt, InputHelper input)
    {
        // Mouse takes over whenever it moves; keyboard always nudges on top.
        // Both devices stay live without a mode switch — whichever the player
        // touched last wins, which is what players expect.
        if (input.MouseMoved)
            _centerX = input.MouseX;

        if (input.IsKeyDown(Keys.Left) || input.IsKeyDown(Keys.A))
            _centerX -= KeyboardSpeed * dt;
        if (input.IsKeyDown(Keys.Right) || input.IsKeyDown(Keys.D))
            _centerX += KeyboardSpeed * dt;

        if (_wideTimer > 0f)
            _wideTimer -= dt;

        // Animate toward the target width instead of snapping — exponential
        // ease via lerp-with-dt, the cheapest smoothing there is.
        float targetWidth = IsWide ? WideWidth : BaseWidth;
        _width = MathHelper.Lerp(_width, targetWidth, MathF.Min(1f, WidthLerpSpeed * dt));

        float half = _width / 2f;
        _centerX = MathHelper.Clamp(_centerX, half, Screen.Width - half);
    }

    public void ApplyWide(float duration) => _wideTimer = duration;

    public void Draw(SpriteBatch spriteBatch)
    {
        Color body = IsWide ? new Color(120, 220, 255) : new Color(226, 226, 236);
        Rectangle b = Bounds;
        spriteBatch.DrawRect(b, body);
        spriteBatch.DrawRect(new Rectangle(b.X, b.Y, b.Width, 2), Color.White);
        spriteBatch.DrawRect(new Rectangle(b.X, b.Bottom - 2, b.Width, 2),
            Color.Lerp(body, Color.Black, 0.4f));
    }
}
