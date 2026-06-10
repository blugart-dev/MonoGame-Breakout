using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.Entities;

/// <summary>
/// A player's paddle — "a", not "the", since local co-op fields two. Stored
/// as a center X plus a width (not a rectangle) because both input and the
/// wide power-up think in terms of "where is the middle" and "how wide right
/// now" — the Bounds rectangle is derived on read. Which actions move it,
/// whether the mouse drives it, and which slice of the court it may occupy
/// are all constructor data: the multiball lesson again, applied to input —
/// an entity that reads *its own* bindings multiplies cleanly, one that
/// hard-codes "the" bindings does not.
/// </summary>
public class Paddle
{
    public const int Height = 14;
    public const int Y = Screen.Height - 40;

    private const float BaseWidth = 100f;
    private const float WideWidth = 160f;
    private const float KeyboardSpeed = 520f;
    private const float WidthLerpSpeed = 10f;

    private static readonly Color DefaultBodyColor = new(226, 226, 236);

    private readonly GameAction _moveLeft;
    private readonly GameAction _moveRight;
    private readonly bool _useMouse;
    private readonly float _minX, _maxX; // court slice (full screen in 1P)
    private readonly Color _bodyColor;

    private float _centerX;
    private float _width = BaseWidth;
    private float _wideTimer;
    private bool _shrunk;

    public bool IsWide => _wideTimer > 0f;

    /// <summary>The single-player paddle: default actions, mouse, full court.</summary>
    public Paddle()
        : this(GameAction.MoveLeft, GameAction.MoveRight, useMouse: true, 0f, Screen.Width) { }

    public Paddle(GameAction moveLeft, GameAction moveRight, bool useMouse,
        float minX, float maxX, Color? bodyColor = null)
    {
        _moveLeft = moveLeft;
        _moveRight = moveRight;
        _useMouse = useMouse;
        _minX = minX;
        _maxX = maxX;
        _bodyColor = bodyColor ?? DefaultBodyColor;
        _centerX = (minX + maxX) / 2f;
    }

    public Rectangle Bounds => new((int)(_centerX - _width / 2f), Y, (int)_width, Height);

    public void Update(float dt, InputHelper input)
    {
        // Mouse takes over whenever it moves; keyboard always nudges on top.
        // Both devices stay live without a mode switch — whichever the player
        // touched last wins, which is what players expect.
        if (_useMouse && input.MouseMoved)
            _centerX = input.MouseX;

        if (input.IsActionDown(_moveLeft))
            _centerX -= KeyboardSpeed * dt;
        if (input.IsActionDown(_moveRight))
            _centerX += KeyboardSpeed * dt;

        if (_wideTimer > 0f)
            _wideTimer -= dt;

        // Animate toward the target width instead of snapping — exponential
        // ease via lerp-with-dt, the cheapest smoothing there is. Shrunk wins
        // over wide: the classic penalty is a rule, not a power-up to outbid.
        float targetWidth = _shrunk ? BaseWidth / 2f : IsWide ? WideWidth : BaseWidth;
        _width = MathHelper.Lerp(_width, targetWidth, MathF.Min(1f, WidthLerpSpeed * dt));

        float half = _width / 2f;
        _centerX = MathHelper.Clamp(_centerX, _minX + half, _maxX - half);
    }

    public void ApplyWide(float duration) => _wideTimer = duration;

    /// <summary>
    /// The 1976 penalty: once the ball breaks through to the zone behind the
    /// wall, the paddle plays the rest of the volley at half width.
    /// </summary>
    public void ApplyClassicShrink() => _shrunk = true;

    /// <summary>Back to normal width for a fresh serve (clears wide too).</summary>
    public void ResetWidth()
    {
        _shrunk = false;
        _wideTimer = 0f;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color body = _shrunk ? new Color(255, 170, 90)
            : IsWide ? new Color(120, 220, 255) : _bodyColor;
        Rectangle b = Bounds;
        spriteBatch.DrawRect(b, body);
        spriteBatch.DrawRect(new Rectangle(b.X, b.Y, b.Width, 2), Color.White);
        spriteBatch.DrawRect(new Rectangle(b.X, b.Bottom - 2, b.Width, 2),
            Color.Lerp(body, Color.Black, 0.4f));
    }
}
