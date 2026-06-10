using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>
/// Holds the one active state and the session it operates on. Note how small
/// a "screen changer" really is: ChangeState is a field assignment plus an
/// Enter() call, and "new game" is just constructing a fresh GameSession.
/// </summary>
public sealed class GameStateManager
{
    public GameSession Session { get; private set; }
    public SpriteFont Font { get; }

    private GameState _current;

    public GameStateManager(SpriteFont font)
    {
        Font = font;
        StartNewGame();
    }

    public void StartNewGame()
    {
        Session = new GameSession();
        ChangeState(new ReadyState(this));
    }

    public void ChangeState(GameState next)
    {
        _current = next;
        next.Enter();
    }

    public void Update(float dt, InputHelper input)
    {
        Session.UpdateEffects(dt); // effects run in every state
        _current.Update(dt, input);
    }

    public void Draw(SpriteBatch spriteBatch) => _current.Draw(spriteBatch);
}
