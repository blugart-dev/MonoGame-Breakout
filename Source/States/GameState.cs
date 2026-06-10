using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>
/// Flow control as a small hand-rolled state machine. MonoGame has no built-in
/// "scene" or "screen" concept — a state is just an object that decides what
/// updates and what draws this frame, and changing screens means swapping
/// which object gets those calls. The world itself lives in GameSession and
/// survives state changes.
/// </summary>
public abstract class GameState
{
    protected readonly GameStateManager Manager;

    protected GameSession Session => Manager.Session;
    protected SpriteFont Font => Manager.Font;

    protected GameState(GameStateManager manager) => Manager = manager;

    /// <summary>One-shot setup when the state becomes current.</summary>
    public virtual void Enter() { }

    public abstract void Update(float dt, InputHelper input);
    public abstract void Draw(SpriteBatch spriteBatch);

    protected void DrawWorldAndHud(SpriteBatch spriteBatch)
    {
        Session.DrawWorld(spriteBatch);
        Session.DrawHud(spriteBatch, Font);
    }
}
