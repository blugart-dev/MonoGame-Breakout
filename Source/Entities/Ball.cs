using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.Entities;

/// <summary>
/// The ball: a center position plus a velocity in pixels per second. It knows
/// how to fly, stick to the paddle between serves, bounce off the paddle, and
/// speed up as bricks fall — but it never decides *when* any of that happens;
/// PlayingState drives it. Entities here are data plus motion; rules live in
/// the states.
/// </summary>
public class Ball
{
    public const int Size = 10;

    private const float InitialSpeed = 320f;
    private const float MaxSpeed = 560f;
    private const float SpeedRampPerBrick = 8f;
    private const float MaxBounceAngle = MathHelper.Pi / 3f; // 60° from vertical
    private const float MaxLaunchAngle = MathHelper.Pi / 7f; // ~26° from vertical

    // Public fields rather than properties: Vector2 is a mutable struct, and
    // `ball.Position.X += ...` only compiles against a field (CS1612 through a
    // property, which returns a copy). Idiomatic in XNA/MonoGame gameplay code.
    public Vector2 Position;
    public Vector2 Velocity;

    public float Speed { get; private set; } = InitialSpeed;

    private Paddle _attachedTo;
    public bool IsAttached => _attachedTo != null;

    // Trail: the last N center positions in a ring buffer. Same GC-free shape
    // as the particle pool — a fixed array plus a write index that wraps with
    // modulo. Nothing is ever allocated or shifted; "oldest entry" is simply
    // "the slot the head will overwrite next".
    private const int TrailLength = 12;
    private readonly Vector2[] _trail = new Vector2[TrailLength];
    private int _trailHead;  // next slot to write
    private int _trailCount; // how many slots hold real data (< Length at start)

    public Rectangle Bounds => new(
        (int)(Position.X - Size / 2f), (int)(Position.Y - Size / 2f), Size, Size);

    public void AttachTo(Paddle paddle)
    {
        _attachedTo = paddle;
        Velocity = Vector2.Zero;
        Speed = InitialSpeed; // losing a life also resets the speed ramp
        _trailCount = 0;      // no ghost trail on the freshly served ball
    }

    public void Launch(Random rng)
    {
        float angle = ((float)rng.NextDouble() * 2f - 1f) * MaxLaunchAngle;
        Velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Speed;
        _attachedTo = null;
    }

    public void Update(float dt)
    {
        if (IsAttached)
        {
            Position = new Vector2(
                _attachedTo.Bounds.Center.X,
                _attachedTo.Bounds.Top - Size / 2f - 1);
            return;
        }

        // One trail sample per fixed 60 Hz tick gives evenly spaced ghosts
        // without any timestamp bookkeeping.
        _trail[_trailHead] = Position;
        _trailHead = (_trailHead + 1) % TrailLength;
        if (_trailCount < TrailLength)
            _trailCount++;

        Position += Velocity * dt;
    }

    /// <summary>
    /// The classic Breakout control mechanic: the exit angle depends on *where*
    /// the ball struck the paddle, not on the incoming angle. Center hit goes
    /// straight up; edge hits leave at up to MaxBounceAngle. This single
    /// mapping is what turns the paddle from a wall into an aiming device —
    /// it is the game-feel knob of the whole genre.
    /// </summary>
    public void BounceOffPaddle(Paddle paddle)
    {
        Rectangle p = paddle.Bounds;
        float offset = (Position.X - p.Center.X) / (p.Width / 2f);
        offset = MathHelper.Clamp(offset, -1f, 1f);

        float angle = offset * MaxBounceAngle;
        Velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Speed;
        Position.Y = p.Top - Size / 2f; // lift clear so we can't hit twice
    }

    public void RampSpeed()
    {
        Speed = MathF.Min(Speed + SpeedRampPerBrick, MaxSpeed);
        if (Velocity != Vector2.Zero)
            Velocity = Vector2.Normalize(Velocity) * Speed;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Trail first so the ball draws on top of it. Walk from oldest to
        // newest: index back from the head, oldest sample = furthest back.
        for (int i = 0; i < _trailCount; i++)
        {
            int index = (_trailHead - _trailCount + i + TrailLength) % TrailLength;

            // t runs 0 (oldest) -> 1 (newest); both size and alpha follow it.
            float t = (i + 1f) / (_trailCount + 1f);
            int size = Math.Max(2, (int)(Size * t * 0.8f));
            var rect = new Rectangle(
                (int)(_trail[index].X - size / 2f),
                (int)(_trail[index].Y - size / 2f), size, size);

            // `Color * float` premultiplies — fades cleanly under SpriteBatch's
            // default blend, same trick as the particles.
            spriteBatch.DrawRect(rect, Color.White * (t * t * 0.30f));
        }

        spriteBatch.DrawRect(Bounds, Color.White);
    }
}
