using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Breakout.Systems;

/// <summary>
/// MonoGame input is pure polling: Keyboard.GetState() returns a snapshot of
/// what is held down *right now* — there are no events and no action map.
/// "Was this key *just* pressed?" therefore requires memory: keep last frame's
/// snapshot and compare. Engines that offer a built-in "just pressed" query
/// are doing exactly this comparison under the hood; here it is two lines you
/// can read. The ActionMap (exposed as Actions) adds the second layer engines
/// bundle: naming inputs by intent so gameplay never mentions a key.
/// </summary>
public sealed class InputHelper
{
    /// <summary>Key bindings; gameplay reads input via actions, not Keys.</summary>
    public ActionMap Actions { get; } = new();

    private KeyboardState _keyboard, _previousKeyboard;
    private MouseState _mouse, _previousMouse;
    private Point _virtualMousePosition;

    /// <summary>Call exactly once per Update tick, before anything reads input.</summary>
    public void Update(VirtualScreen virtualScreen)
    {
        _previousKeyboard = _keyboard;
        _previousMouse = _mouse;
        _keyboard = Keyboard.GetState();
        _mouse = Mouse.GetState();

        // The OS reports the mouse in window-client pixels; the game thinks in
        // virtual pixels. Convert once here so no gameplay code ever knows the
        // window size exists.
        _virtualMousePosition = virtualScreen.WindowToVirtual(new Point(_mouse.X, _mouse.Y));
    }

    public bool IsKeyDown(Keys key) => _keyboard.IsKeyDown(key);

    public bool WasKeyJustPressed(Keys key)
        => _keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    // The action-level queries: any bound key satisfies the action. These
    // loop over an array instead of allocating LINQ enumerators because they
    // run every tick on the hot path.

    public bool IsActionDown(GameAction action)
    {
        IReadOnlyList<Keys> keys = Actions.KeysFor(action);
        for (int i = 0; i < keys.Count; i++)
            if (_keyboard.IsKeyDown(keys[i]))
                return true;
        return false;
    }

    public bool WasActionJustPressed(GameAction action)
    {
        IReadOnlyList<Keys> keys = Actions.KeysFor(action);
        for (int i = 0; i < keys.Count; i++)
            if (_keyboard.IsKeyDown(keys[i]) && _previousKeyboard.IsKeyUp(keys[i]))
                return true;
        return false;
    }

    public bool WasLeftClickJustPressed
        => _mouse.LeftButton == ButtonState.Pressed
           && _previousMouse.LeftButton == ButtonState.Released;

    /// <summary>Mouse X in virtual (800x480) coordinates.</summary>
    public int MouseX => _virtualMousePosition.X;

    // Movement detection compares *raw* positions: the virtual transform
    // rounds to ints, and that rounding could register phantom movement.
    public bool MouseMoved
        => _mouse.X != _previousMouse.X || _mouse.Y != _previousMouse.Y;
}
