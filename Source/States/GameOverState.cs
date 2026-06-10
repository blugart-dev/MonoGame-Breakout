using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>End screen for both outcomes; Enter or click starts a fresh run.</summary>
public sealed class GameOverState : GameState
{
    private readonly bool _won;

    public GameOverState(GameStateManager manager, bool won) : base(manager)
        => _won = won;

    public override void Enter()
    {
        if (_won)
            AudioBank.Win?.Play();
        else
            AudioBank.LifeLost?.Play();
    }

    public override void Update(float dt, InputHelper input)
    {
        if (input.WasKeyJustPressed(Keys.Enter) || input.WasLeftClickJustPressed)
            Manager.StartNewGame();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawWorldAndHud(spriteBatch);

        // Dim the frozen world so the text owns the screen. `Color * 0.65f`
        // premultiplies, which is what SpriteBatch's default blend expects.
        spriteBatch.DrawRect(new Rectangle(0, 0, Screen.Width, Screen.Height),
            Color.Black * 0.65f);

        var center = new Vector2(Screen.Width / 2f, 200);
        spriteBatch.DrawCenteredText(Font, _won ? "YOU WIN!" : "GAME OVER",
            center, _won ? Color.Gold : Color.OrangeRed, 2f);
        spriteBatch.DrawCenteredText(Font, $"FINAL SCORE  {Session.Score}",
            center + new Vector2(0, 60), Color.White);
        spriteBatch.DrawCenteredText(Font, "PRESS ENTER OR CLICK TO PLAY AGAIN",
            center + new Vector2(0, 110), new Color(150, 150, 165), 0.75f);
    }
}
