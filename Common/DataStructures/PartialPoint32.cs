using System;
using Terraria.DataStructures;

namespace SpawnHouses.Common.DataStructures;

public struct PartialPoint32 {
    public readonly bool HasX;
    public readonly int X;
    public readonly bool HasY;
    public readonly int Y;

    public PartialPoint32(Point16 point, bool hasX = true, bool hasY = true) {
        HasX = hasX;
        HasY = hasY;
        X = HasX ? point.X : -1;
        Y = HasY ? point.Y : -1;
    }

    public PartialPoint32(int x, int y, bool hasX = true, bool hasY = true) {
        HasX = hasX;
        HasY = hasY;
        X = HasX ? x : -1;
        Y = HasY ? y : -1;
    }

    public override bool Equals(object obj) {
        if (obj is PartialPoint32 other) return (X == other.X || !HasX) && (Y == other.Y || HasY);
        return base.Equals(obj);
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => "(" + (HasX ? X : "_") + ", " + (HasY ? Y : "_") + ")";
}