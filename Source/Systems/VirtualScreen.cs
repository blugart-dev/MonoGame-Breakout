using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Breakout.Systems;

/// <summary>
/// Virtual-resolution rendering: the whole game draws into an off-screen
/// RenderTarget2D at a fixed 800x480, which is then scaled onto the real
/// back buffer with aspect-ratio-preserving letterboxing. Gameplay code can
/// assume one resolution forever; the window can be anything.
///
/// This is the piece every shipping MonoGame project has — full engines bundle
/// it as a "stretch mode" or "resolution scaling" project setting. Here you
/// build it yourself, and in exchange you see all the moving parts: the render
/// target, the destination rectangle math, and why mouse coordinates must be
/// transformed back (the OS reports them in *window* pixels, but the game
/// thinks in *virtual* pixels).
/// </summary>
public sealed class VirtualScreen
{
    private readonly GraphicsDevice _device;
    private readonly RenderTarget2D _target;

    public int VirtualWidth { get; }
    public int VirtualHeight { get; }

    public VirtualScreen(GraphicsDevice device, int virtualWidth, int virtualHeight)
    {
        _device = device;
        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;
        _target = new RenderTarget2D(device, virtualWidth, virtualHeight);
    }

    /// <summary>
    /// Where the virtual image lands on the back buffer: scaled by the
    /// smaller of the two axis ratios (so it always fits) and centered
    /// (the leftover area becomes the black letterbox bars).
    /// </summary>
    public Rectangle DestinationBounds
    {
        get
        {
            int windowWidth = _device.PresentationParameters.BackBufferWidth;
            int windowHeight = _device.PresentationParameters.BackBufferHeight;

            float scale = MathF.Min(
                (float)windowWidth / VirtualWidth,
                (float)windowHeight / VirtualHeight);

            int width = (int)(VirtualWidth * scale);
            int height = (int)(VirtualHeight * scale);
            return new Rectangle(
                (windowWidth - width) / 2, (windowHeight - height) / 2, width, height);
        }
    }

    /// <summary>All drawing after this call lands in the virtual target.</summary>
    public void BeginDraw() => _device.SetRenderTarget(_target);

    /// <summary>
    /// Switch back to the real back buffer and draw the virtual image onto it,
    /// scaled. PointClamp keeps the scale-up crisp instead of bilinear-smeared.
    /// </summary>
    public void Present(SpriteBatch spriteBatch)
    {
        _device.SetRenderTarget(null); // null = the actual back buffer
        _device.Clear(Color.Black);    // letterbox bar color

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_target, DestinationBounds, Color.White);
        spriteBatch.End();
    }

    /// <summary>Window-client pixels -> virtual pixels (inverse of the present scale).</summary>
    public Point WindowToVirtual(Point windowPosition)
    {
        Rectangle dest = DestinationBounds;
        if (dest.Width == 0 || dest.Height == 0)
            return windowPosition; // minimized window; avoid dividing by zero

        float scale = (float)VirtualWidth / dest.Width;
        return new Point(
            (int)((windowPosition.X - dest.X) * scale),
            (int)((windowPosition.Y - dest.Y) * scale));
    }
}
