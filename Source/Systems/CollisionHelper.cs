using Microsoft.Xna.Framework;

namespace Breakout.Systems;

/// <summary>Which side of the target rectangle was struck.</summary>
public enum HitSide { None, Left, Right, Top, Bottom }

/// <summary>
/// Minimal-penetration AABB resolution: of the two axes of the overlap
/// rectangle, the shallower one is the axis of impact, and the relative centers
/// disambiguate which side. This is the classic discrete technique; it is exact
/// enough here because the ball moves at most ~9 px per 60 Hz tick against
/// 20 px bricks, so it can never tunnel through a target in one step. A faster
/// ball or thinner targets would call for a swept (continuous) test instead.
/// </summary>
public static class CollisionHelper
{
    public static HitSide GetCollisionSide(Rectangle moving, Rectangle target)
    {
        Rectangle overlap = Rectangle.Intersect(moving, target);
        if (overlap.IsEmpty)
            return HitSide.None;

        if (overlap.Width < overlap.Height)
            return moving.Center.X < target.Center.X ? HitSide.Left : HitSide.Right;

        return moving.Center.Y < target.Center.Y ? HitSide.Top : HitSide.Bottom;
    }
}
