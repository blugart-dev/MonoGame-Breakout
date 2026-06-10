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

    /// <summary>
    /// Make a previously interrupted state current again *without* calling
    /// Enter() — it never conceptually exited, so its one-shot setup (sounds,
    /// re-attaching the ball) must not run a second time.
    /// </summary>
    public void ResumeState(GameState state) => _current = state;

    /// <summary>
    /// Called by the shell when the window loses focus. Auto-pausing the live
    /// game is the professional default — nobody wants to lose a ball while
    /// alt-tabbed. Only Playing needs it: every other state is already static.
    /// </summary>
    public void NotifyFocusLost()
    {
        if (_current is PlayingState)
            ChangeState(new PauseState(this, _current));
    }

    public void Update(float dt, InputHelper input)
    {
        if (!_current.FreezesEffects)
            Session.UpdateEffects(dt); // effects run in (almost) every state
        _current.Update(dt, input);
    }

    public void Draw(SpriteBatch spriteBatch) => _current.Draw(spriteBatch);
}
