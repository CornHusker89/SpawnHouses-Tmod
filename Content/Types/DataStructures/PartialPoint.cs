using System;
using Terraria.DataStructures;

namespace SpawnHouses.Content.Types.DataStructures;

/// <summary>
///     acts like a normal XNA Point but the presence of each axis is optional
/// </summary>
public readonly struct PartialPoint : IEquatable<PartialPoint> {
    public readonly bool HasX;
    public readonly int X;
    public readonly bool HasY;
    public readonly int Y;

    public PartialPoint(Point16 point, bool hasX = true, bool hasY = true) {
        HasX = hasX;
        HasY = hasY;
        X = HasX ? point.X : -1;
        Y = HasY ? point.Y : -1;
    }

    public PartialPoint(int x, int y, bool hasX = true, bool hasY = true) {
        HasX = hasX;
        HasY = hasY;
        X = HasX ? x : -1;
        Y = HasY ? y : -1;
    }

    public override bool Equals(object obj) {
        if (obj is PartialPoint other) return (X == other.X || !HasX) && (Y == other.Y || HasY);
        return base.Equals(obj);
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => "(" + (HasX ? X : "_") + ", " + (HasY ? Y : "_") + ")";

    public static bool operator ==(PartialPoint left, PartialPoint right) => left.Equals(right);

    public static bool operator !=(PartialPoint left, PartialPoint right) => !(left == right);

    public bool Equals(PartialPoint other) => HasX == other.HasX && X == other.X && HasY == other.HasY && Y == other.Y;
}