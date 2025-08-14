using System;
using Microsoft.Xna.Framework;

namespace DungeonSlime.Library.Geometry;

/// <summary>
/// Axis-Aligned Bounding Box (AABB) with integer coordinates.
/// Immutable, value-type semantics similar to <see cref="Circle"/>.
/// </summary>
public readonly struct AABB : IEquatable<AABB>
{
    private static readonly AABB s_empty = new AABB();

    /// <summary>
    /// The x-coordinate of the top-left corner of this AABB.
    /// </summary>
    public readonly int X;

    /// <summary>
    /// The y-coordinate of the top-left corner of this AABB.
    /// </summary>
    public readonly int Y;

    /// <summary>
    /// The width of this AABB in pixels.
    /// </summary>
    public readonly int Width;

    /// <summary>
    /// The height of this AABB in pixels.
    /// </summary>
    public readonly int Height;

    /// <summary>
    /// Gets the location (top-left) of this AABB.
    /// </summary>
    public readonly Point Location => new Point(X, Y);

    /// <summary>
    /// Gets the size of this AABB.
    /// </summary>
    public readonly Point Size => new Point(Width, Height);

    /// <summary>
    /// Gets a value that indicates whether this AABB has zero size at location (0,0).
    /// </summary>
    public readonly bool IsEmpty => X == 0 && Y == 0 && Width == 0 && Height == 0;

    /// <summary>
    /// Gets an empty AABB at (0,0) with zero size.
    /// </summary>
    public static AABB Empty => s_empty;

    /// <summary>
    /// Gets the y-coordinate of the top edge of this AABB.
    /// </summary>
    public readonly int Top => Y;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge of this AABB.
    /// </summary>
    public readonly int Bottom => Y + Height;

    /// <summary>
    /// Gets the x-coordinate of the left edge of this AABB.
    /// </summary>
    public readonly int Left => X;

    /// <summary>
    /// Gets the x-coordinate of the right edge of this AABB.
    /// </summary>
    public readonly int Right => X + Width;

    /// <summary>
    /// Creates a new AABB with the specified top-left corner and size.
    /// </summary>
    public AABB(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a new AABB from a MonoGame Rectangle.
    /// </summary>
    public AABB(Rectangle rect)
    {
        X = rect.X;
        Y = rect.Y;
        Width = rect.Width;
        Height = rect.Height;
    }

    /// <summary>
    /// Returns true if this AABB overlaps the other AABB with positive area.
    /// Touching edges do not count as intersection (consistent with Circle.Intersects).
    /// </summary>
    public bool Intersects(AABB other)
    {
        return Left < other.Right &&
               Right > other.Left &&
               Top < other.Bottom &&
               Bottom > other.Top;
    }

    /// <summary>
    /// Returns true if this AABB intersects the specified circle.
    /// Touching edge does not count as intersection.
    /// </summary>
    public bool Intersects(Circle circle)
    {
        // Clamp circle center to the AABB
        int closestX = Math.Clamp(circle.X, Left, Right);
        int closestY = Math.Clamp(circle.Y, Top, Bottom);

        int dx = circle.X - closestX;
        int dy = circle.Y - closestY;

        int r2 = circle.Radius * circle.Radius;
        int d2 = dx * dx + dy * dy;

        return d2 < r2;
    }

    /// <summary>
    /// Returns true if the specified point lies strictly inside this AABB.
    /// Points on the edge are considered outside to match the intersection policy.
    /// </summary>
    public bool Contains(Point point) => Contains(point.X, point.Y);

    /// <summary>
    /// Returns true if the specified point lies strictly inside this AABB.
    /// Points on the edge are considered outside to match the intersection policy.
    /// </summary>
    public bool Contains(int x, int y)
    {
        return x > Left && x < Right && y > Top && y < Bottom;
    }

    /// <summary>
    /// Converts this AABB to a MonoGame Rectangle.
    /// </summary>
    public Rectangle ToRectangle() => new Rectangle(X, Y, Width, Height);

    public override bool Equals(object obj) => obj is AABB other && Equals(other);

    public bool Equals(AABB other)
        => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    public static bool operator ==(AABB lhs, AABB rhs) => lhs.Equals(rhs);
    public static bool operator !=(AABB lhs, AABB rhs) => !lhs.Equals(rhs);
}