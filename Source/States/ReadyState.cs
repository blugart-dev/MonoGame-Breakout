using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>Ball glued to the paddle, waiting for the launch input.</summary>
public sealed class ReadyState : GameState
{
    public ReadyState(GameStateManager manager) : base(manager) { }

    public override void Enter() => Session.Ball.AttachTo(Session.Paddle);

    public override void Update(float dt, InputHelper input)
    {
        Session.Paddle.Update(dt, input);
        Session.Ball.Update(dt); // follows the paddle while attached

        if (input.WasKeyJustPressed(Keys.Space) || input.WasLeftClickJustPressed)
        {
            Session.Ball.Launch(Session.Rng);
            Manager.ChangeState(new PlayingState(Manager));
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawWorldAndHud(spriteBatch);
        spriteBatch.DrawCenteredText(Font, "PRESS SPACE OR CLICK TO LAUNCH",
            new Vector2(Screen.Width / 2f, 300), Color.White);
        spriteBatch.DrawCenteredText(Font, "MOVE: MOUSE / ARROWS / A-D",
            new Vector2(Screen.Width / 2f, 332), new Color(150, 150, 165), 0.75f);
    }
}
