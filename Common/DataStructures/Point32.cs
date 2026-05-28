using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace SpawnHouses.Common.DataStructures;

public struct Point32 {
    public readonly int X;
    public readonly int Y;

    public Point32(int size) {
        X = size;
        Y = size;
    }

    public Point32(int x, int y) {
        X = x;
        Y = y;
    }

    public Vector2 ToVector2() => new(X, Y);

    public static Point32 operator +(Point32 first, Point32 second)
        => new(first.X + second.X, first.Y + second.Y);

    public static Point32 operator +(Point16 first, Point32 second)
        => new(first.X + second.X, first.Y + second.Y);

    public static Point32 operator +(Point32 first, Point16 second)
        => new(first.X + second.X, first.Y + second.Y);

    public static Point32 operator -(Point32 first, Point32 second)
        => new(first.X + second.X, first.Y + second.Y);

    public static Point32 operator -(Point16 first, Point32 second)
        => new(first.X + second.X, first.Y + second.Y);

    public static Point32 operator -(Point32 first, Point16 second)
        => new(first.X + second.X, first.Y + second.Y);

    public static Point32 operator *(Point16 first, Point32 second)
        => new(first.X * second.X, first.Y * second.Y);
}