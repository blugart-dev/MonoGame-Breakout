using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Entities;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>
/// The live simulation: ball flight, all collision responses, power-ups,
/// scoring, and the transitions out (life lost, game over, level cleared).
/// </summary>
public sealed class PlayingState : GameState
{
    private const float PowerUpDropChance = 0.15f;

    public PlayingState(GameStateManager manager) : base(manager) { }

    public override void Update(float dt, InputHelper input)
    {
        Session.Paddle.Update(dt, input);
        Session.Ball.Update(dt);

        BounceOffWalls();
        BounceOffPaddle();
        HandleBrickCollision();
        UpdatePowerUps(dt);

        if (Session.Ball.Position.Y - Ball.Size > Screen.Height)
            OnBallLost();
        else if (Session.LevelCleared)
            Manager.ChangeState(new GameOverState(Manager, won: true));
    }

    public override void Draw(SpriteBatch spriteBatch) => DrawWorldAndHud(spriteBatch);

    private void BounceOffWalls()
    {
        Ball ball = Session.Ball;
        const float half = Ball.Size / 2f;
        bool bounced = false;

        if (ball.Position.X < half)
        {
            ball.Position.X = half;
            ball.Velocity.X = MathF.Abs(ball.Velocity.X);
            bounced = true;
        }
        else if (ball.Position.X > Screen.Width - half)
        {
            ball.Position.X = Screen.Width - half;
            ball.Velocity.X = -MathF.Abs(ball.Velocity.X);
            bounced = true;
        }

        if (ball.Position.Y < half)
        {
            ball.Position.Y = half;
            ball.Velocity.Y = MathF.Abs(ball.Velocity.Y);
            bounced = true;
        }

        if (bounced)
            AudioBank.WallHit?.Play();
    }

    private void BounceOffPaddle()
    {
        Ball ball = Session.Ball;

        // Only while descending: once the ball bounces it overlaps the paddle
        // for a few frames, and re-bouncing every one of them would glue it down.
        if (ball.Velocity.Y <= 0f)
            return;
        if (!ball.Bounds.Intersects(Session.Paddle.Bounds))
            return;

        ball.BounceOffPaddle(Session.Paddle);
        AudioBank.PaddleHit?.Play();
    }

    private void HandleBrickCollision()
    {
        Ball ball = Session.Ball;

        foreach (Brick brick in Session.Bricks)
        {
            HitSide side = CollisionHelper.GetCollisionSide(ball.Bounds, brick.Bounds);
            if (side == HitSide.None)
                continue;

            ReflectAndSeparate(ball, brick.Bounds, side);

            if (brick.Hit())
            {
                Session.Score += brick.ScoreValue;
                Session.Particles.Emit(brick.Bounds.Center.ToVector2(), brick.BaseColor, 18, Session.Rng);
                Session.Shake.Add(0.3f);
                Session.Ball.RampSpeed(); // difficulty ramps with progress
                AudioBank.BrickBreak?.Play();
                MaybeDropPowerUp(brick.Bounds.Center.ToVector2());
            }
            else
            {
                Session.Shake.Add(0.1f);
                AudioBank.BrickHit?.Play();
            }

            // One brick per tick: after reflecting, a second simultaneous
            // overlap resolves cleanly on the next 60 Hz step anyway, and
            // processing both would double-flip the velocity.
            break;
        }

        Session.Bricks.RemoveAll(b => !b.Alive);
    }

    /// <summary>
    /// Push the ball out of the rectangle along the impact axis, then point
    /// the velocity away from it. Setting the sign outright (rather than
    /// negating) makes the response idempotent if we ever resolve twice.
    /// </summary>
    private static void ReflectAndSeparate(Ball ball, Rectangle target, HitSide side)
    {
        Rectangle overlap = Rectangle.Intersect(ball.Bounds, target);
        switch (side)
        {
            case HitSide.Left:
                ball.Position.X -= overlap.Width;
                ball.Velocity.X = -MathF.Abs(ball.Velocity.X);
                break;
            case HitSide.Right:
                ball.Position.X += overlap.Width;
                ball.Velocity.X = MathF.Abs(ball.Velocity.X);
                break;
            case HitSide.Top:
                ball.Position.Y -= overlap.Height;
                ball.Velocity.Y = -MathF.Abs(ball.Velocity.Y);
                break;
            case HitSide.Bottom:
                ball.Position.Y += overlap.Height;
                ball.Velocity.Y = MathF.Abs(ball.Velocity.Y);
                break;
        }
    }

    private void MaybeDropPowerUp(Vector2 origin)
    {
        if (Session.Rng.NextDouble() > PowerUpDropChance)
            return;
        Session.PowerUps.Add(new PowerUp(PowerUpType.WidePaddle, origin));
    }

    private void UpdatePowerUps(float dt)
    {
        // Backwards so RemoveAt doesn't shift unvisited elements.
        for (int i = Session.PowerUps.Count - 1; i >= 0; i--)
        {
            PowerUp powerUp = Session.PowerUps[i];
            powerUp.Update(dt);

            if (powerUp.Bounds.Intersects(Session.Paddle.Bounds))
            {
                Session.Paddle.ApplyWide(PowerUp.WidePaddleDuration);
                AudioBank.PowerUpCatch?.Play();
                Session.PowerUps.RemoveAt(i);
            }
            else if (powerUp.IsBelowScreen)
            {
                Session.PowerUps.RemoveAt(i);
            }
        }
    }

    private void OnBallLost()
    {
        Session.Lives--;
        if (Session.Lives > 0)
            Manager.ChangeState(new LifeLostState(Manager));
        else
            Manager.ChangeState(new GameOverState(Manager, won: false));
    }
}
