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

        // A session exists even under the title screen: the shell reads
        // Session.Shake every Draw, and the debug overlay reads its counts.
        // The title state simply never draws or updates the world.
        Session = new GameSession(GameMode.Modern);
        ChangeState(new TitleState(this));
    }

    public void StartNewGame(GameMode mode, int startLevel = 0)
    {
        Session = new GameSession(mode, startLevel);
        ChangeState(CreateServeState());
    }

    /// <summary>Same mode, same starting level — the "play again" path.</summary>
    public void RestartCurrentGame()
        => StartNewGame(Session.Mode, Session.StartLevelIndex);

    public void GoToTitle() => ChangeState(new TitleState(this));

    /// <summary>
    /// The one place that maps a mode to its rule-set states. Every "back to
    /// the serve" transition (life lost, new game) routes through here, so
    /// adding a mode never means hunting down transitions.
    /// </summary>
    public GameState CreateServeState()
        => Session.Mode == GameMode.Classic ? new ClassicReadyState(this)
            : Session.IsSuper ? new SuperReadyState(this)
            : new ReadyState(this);

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
        // SuperPlayingState is abstract, so this `is` covers all three of
        // its variant subclasses at once.
        if (_current is PlayingState or ClassicPlayingState or SuperPlayingState)
            ChangeState(new PauseState(this, _current));
    }

    public bool CurrentCapturesAllInput => _current.CapturesAllInput;

    public void Update(float dt, InputHelper input)
    {
        if (!_current.FreezesEffects)
            Session.UpdateEffects(dt); // effects run in (almost) every state
        _current.Update(dt, input);
    }

    public void Draw(SpriteBatch spriteBatch) => _current.Draw(spriteBatch);
}
