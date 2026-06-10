using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>
/// Waiting for a classic serve. The 1976 serve is nothing like the modern
/// launch: there is no ball riding the paddle and no aiming. You press serve
/// and the ball *materializes* mid-screen already moving — toward you, at the
/// slowest of the four speeds, on a random angle. The machine served at the
/// player; the player's job started at the first paddle contact.
/// </summary>
public sealed class ClassicReadyState : GameState
{
    public ClassicReadyState(GameStateManager manager) : base(manager) { }

    public override void Enter() => Session.PrepareClassicServe();

    public override void Update(float dt, InputHelper input)
    {
        if (input.WasActionJustPressed(GameAction.Pause))
        {
            Manager.ChangeState(new PauseState(Manager, this));
            return;
        }

        Session.Paddle.Update(dt, input);

        if (input.WasActionJustPressed(GameAction.Launch) || input.WasLeftClickJustPressed)
            Serve();
    }

    private void Serve()
    {
        var ball = Session.Balls[0]; // PrepareClassicServe guarantees exactly one

        // "About midway along the TV screen" (manual) — vertically midway,
        // safely below the wall. The X is random so the player can't camp.
        ball.Position = new Vector2(
            200 + (float)Session.Rng.NextDouble() * (Screen.Width - 400), 280);

        ball.OverrideSpeed(ClassicRules.Speeds[0]); // every serve restarts the speed ladder
        ball.Velocity = ClassicRules.ServeDirection(Session.Rng) * ball.Speed;

        Manager.ChangeState(new ClassicPlayingState(Manager));
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawWorldAndHud(spriteBatch);
        spriteBatch.DrawCenteredText(Font, "PRESS SPACE OR CLICK TO SERVE",
            new Vector2(Screen.Width / 2f, 300), Color.White);

        // The original cabinet displayed the ball number — serves count up,
        // not down, so "BALL 2" means your second serve, not two remaining.
        int ballNumber = GameSession.StartingLives - Session.Lives + 1;
        spriteBatch.DrawCenteredText(Font,
            $"BALL {ballNumber} OF {GameSession.StartingLives}",
            new Vector2(Screen.Width / 2f, 332), new Color(150, 150, 165), 0.75f);
    }
}
