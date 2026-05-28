using Microsoft.Xna.Framework;
using SpawnHouses.Common.DataStructures;
using Terraria.DataStructures;

namespace SpawnHouses.Helpers;

public static class Utils {
    public static Point32 ToPoint32(this Vector2 vector) => new((int)vector.X, (int)vector.Y);
    public static Point32 ToPoint32(this Point16 point) => new(point.X, point.Y);
}