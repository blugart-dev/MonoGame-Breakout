using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakout.Systems;

namespace Breakout.States;

/// <summary>
/// The menu before anything exists. Every other state operates on a running
/// GameSession; this one's job is to *create* it — which flushed out the
/// hidden assumption that "the game has already started" (the manager now
/// builds a placeholder session at boot just so the shell has a Shake matrix
/// to read). Selecting an entry calls StartNewGame with a mode and, for the
/// modern game, a starting level — the session parameter the level-select
/// exercise was designed to force.
/// </summary>
public sealed class TitleState : GameState
{
    private const int ModernEntry = 0;
    private const int ClassicEntry = 1;
    private const int EntryCount = 2;

    private static readonly Color DimColor = new(150, 150, 165);

    private int _selected;
    private int _startLevel; // modern only; 0-based like Session.LevelIndex

    public TitleState(GameStateManager manager) : base(manager) { }

    public override void Update(float dt, InputHelper input)
    {
        // Two entries means up and down both "go to the other one", but the
        // modulo form is written for N — adding a mode is one constant.
        if (input.WasActionJustPressed(GameAction.MenuUp))
            _selected = (_selected + EntryCount - 1) % EntryCount;
        if (input.WasActionJustPressed(GameAction.MenuDown))
            _selected = (_selected + 1) % EntryCount;

        // Left/right reuse the move actions — on a menu they read as "adjust".
        if (_selected == ModernEntry)
        {
            if (input.WasActionJustPressed(GameAction.MoveLeft))
                _startLevel = MathHelper.Clamp(_startLevel - 1, 0, GameSession.LevelCount - 1);
            if (input.WasActionJustPressed(GameAction.MoveRight))
                _startLevel = MathHelper.Clamp(_startLevel + 1, 0, GameSession.LevelCount - 1);
        }

        if (input.WasActionJustPressed(GameAction.Launch)
            || input.WasActionJustPressed(GameAction.Restart)
            || input.WasLeftClickJustPressed)
        {
            if (_selected == ClassicEntry)
                Manager.StartNewGame(GameMode.Classic);
            else
                Manager.StartNewGame(GameMode.Modern, _startLevel);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var center = new Vector2(Screen.Width / 2f, 0);

        spriteBatch.DrawCenteredText(Font, "BREAKOUT",
            center + new Vector2(0, 120), Color.Gold, 3f);

        // A nod to the 1976 cabinet: its colors were cellophane strips over a
        // black-and-white monitor, so the title wears the same four bands.
        DrawColorBands(spriteBatch);

        string modernLabel = $"MODERN GAME    < LEVEL {_startLevel + 1} >";
        DrawEntry(spriteBatch, modernLabel, 280, _selected == ModernEntry);
        DrawEntry(spriteBatch, "CLASSIC 1976", 320, _selected == ClassicEntry);

        spriteBatch.DrawCenteredText(Font,
            "UP/DOWN SELECT   LEFT/RIGHT START LEVEL   SPACE PLAY",
            center + new Vector2(0, 400), DimColor, 0.75f);
        spriteBatch.DrawCenteredText(Font,
            "P PAUSE   M MUSIC   F11 FULLSCREEN   ESC QUIT",
            center + new Vector2(0, 430), DimColor, 0.75f);
    }

    private void DrawEntry(SpriteBatch spriteBatch, string text, int y, bool selected)
    {
        spriteBatch.DrawCenteredText(Font, selected ? $"> {text} <" : text,
            new Vector2(Screen.Width / 2f, y),
            selected ? Color.White : DimColor,
            selected ? 1f : 0.85f);
    }

    private static void DrawColorBands(SpriteBatch spriteBatch)
    {
        Color[] bands =
        {
            new(255, 77, 77), new(255, 138, 48), new(87, 212, 118), new(255, 205, 66),
        };
        for (int i = 0; i < bands.Length; i++)
            spriteBatch.DrawRect(
                new Rectangle(Screen.Width / 2 - 140, 170 + i * 10, 280, 8), bands[i]);
    }
}
