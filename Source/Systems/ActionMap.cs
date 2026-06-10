using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Breakout.Systems;

/// <summary>
/// The input-mapping layer big engines ship built in, rebuilt as one
/// dictionary: action → the keys currently bound to it. Two things fall out
/// for free. Alternate bindings are just more entries in the array (arrows
/// AND A/D both mean "move"), and rebinding at runtime is a dictionary
/// write — no gameplay code changes, because no gameplay code names a key.
/// Mouse input deliberately stays outside the map: it is positional, not a
/// binary "pressed" thing a Keys value could stand in for.
/// </summary>
public sealed class ActionMap
{
    private readonly Dictionary<GameAction, Keys[]> _bindings = new()
    {
        [GameAction.MoveLeft] = new[] { Keys.Left, Keys.A },
        [GameAction.MoveRight] = new[] { Keys.Right, Keys.D },
        [GameAction.Launch] = new[] { Keys.Space },
        [GameAction.Pause] = new[] { Keys.P },
        [GameAction.Restart] = new[] { Keys.Enter },
        [GameAction.ToggleFullscreen] = new[] { Keys.F11 },
        [GameAction.ToggleDebugOverlay] = new[] { Keys.F3 },
        [GameAction.ToggleIntegerScaling] = new[] { Keys.F10 },
        [GameAction.Quit] = new[] { Keys.Escape },
    };

    public IReadOnlyList<Keys> KeysFor(GameAction action) => _bindings[action];

    /// <summary>
    /// Replace an action's bindings at runtime. This dictionary write is the
    /// entire rebinding mechanism — a settings screen would collect the new
    /// key (poll for "any key just pressed") and call this.
    /// </summary>
    public void Rebind(GameAction action, params Keys[] keys)
        => _bindings[action] = keys;
}
