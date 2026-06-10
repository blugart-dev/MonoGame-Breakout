using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Entities;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>
/// Waiting for a Super Breakout serve. The 1978 serve is the 1976 serve —
/// no ball riding the paddle; press serve and it materializes mid-screen
/// already falling toward you — so this state is ClassicReadyState's twin.
/// One ready state covers all three variants: they differ in play, not in
/// how a serve begins. It restocks Double's and Cavity's wall between serves
/// if the last volley stripped it bare, charges Progressive's one-row
/// re-serve penalty, and picks which playing state runs the volley.
/// </summary>
public sealed class SuperReadyState : GameState
{
    public SuperReadyState(GameStateManager manager) : base(manager) { }

    public override void Enter()
    {
        Session.PrepareSuperServe();

        // Walls regenerate forever in Double and Cavity. Mid-volley rebuilds
        // wait for a clear touch point; between serves there is nothing to
        // wait for. RebuildSuperWall itself no-ops for Progressive — its
        // conveyor never needs restocking.
        if (Session.Bricks.Count == 0)
            Session.RebuildSuperWall();
    }

    public override void Update(float dt, InputHelper input)
    {
        if (input.WasActionJustPressed(GameAction.Pause))
        {
            Manager.ChangeState(new PauseState(Manager, this));
            return;
        }

        foreach (Paddle paddle in Session.Paddles)
            paddle.Update(dt, input);

        if (input.WasActionJustPressed(GameAction.Launch) || input.WasLeftClickJustPressed)
            Serve();
    }

    private void Serve()
    {
        Ball ball = Session.Balls[0]; // PrepareSuperServe guarantees exactly one
        ball.Position = SuperRules.ServePosition(Session.Rng);
        ball.OverrideSpeed(SuperRules.Speeds[0]); // every serve restarts the ladder
        ball.Velocity = SuperRules.ServeDirection(Session.Rng) * ball.Speed;

        // Progressive's opening board has its second wall at mid-screen, on
        // top of the usual serve line — materialize below it. (Once the
        // conveyor has run, bricks may reach any depth; the pass-through
        // rule keeps a mid-wall serve harmless, but the first impression
        // shouldn't be a ball inside a brick.)
        if (Session.Mode == GameMode.SuperProgressive)
            ball.Position.Y = 320f;

        // "When the ball is served, these captive balls bounce inside the
        // cavity" — only the ones still sealed in; WakeCaptiveBalls skips
        // any that are already moving.
        if (Session.Mode == GameMode.SuperCavity)
            Session.WakeCaptiveBalls();

        Manager.ChangeState(Session.Mode switch
        {
            GameMode.SuperDouble => new SuperDoublePlayingState(Manager),
            GameMode.SuperCavity => new SuperCavityPlayingState(Manager),
            GameMode.SuperProgressive => new SuperProgressivePlayingState(Manager),
            // Reaching this state in a non-Super mode is a routing bug —
            // fail loudly rather than silently playing the wrong game.
            _ => throw new System.InvalidOperationException(
                $"{Session.Mode} is not a Super Breakout mode"),
        });
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawWorldAndHud(spriteBatch);
        spriteBatch.DrawCenteredText(Font, "PRESS SPACE OR CLICK TO SERVE",
            new Vector2(Screen.Width / 2f, 300), Color.White);

        // Serves count up, arcade-style, same as the classic ready screen.
        int ballNumber = GameSession.StartingLives - Session.Lives + 1;
        spriteBatch.DrawCenteredText(Font,
            $"BALL {ballNumber} OF {GameSession.StartingLives}",
            new Vector2(Screen.Width / 2f, 332), new Color(150, 150, 165), 0.75f);
    }
}
