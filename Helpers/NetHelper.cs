using SpawnHouses.Core.Geometry;
using Terraria;
using Terraria.ID;

namespace SpawnHouses.Helpers;

public static class NetHelper {
    
    /// <summary>
    ///     sends a tile square to clients. functions exactly the same as <see cref="NetMessage.SendTileSquare(int, int, int, int, int, TileChangeType)" />
    /// </summary>
    /// <param name="whoAmI"></param>
    /// <param name="boundingBox"></param>
    public static void SendTileSquare(int whoAmI, TileBox boundingBox) {
        NetMessage.SendTileSquare(whoAmI, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
    }
}