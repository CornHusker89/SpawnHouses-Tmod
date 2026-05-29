using Microsoft.Xna.Framework;

namespace SpawnHouses.Helpers;

public static class Utils {
    public static Point ToPoint(this Vector2 vector) => new((int)vector.X, (int)vector.Y);
}