using System.IO;
using SpawnHouses.Content.Types.DataStructures;
using SpawnHouses.Content.Types.Enums;
using SpawnHouses.Legacy.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Helpers;

public static class NetHelper {
    public static void HandlePacket(BinaryReader reader, int sender) {
        NetMessageType messageType = (NetMessageType)reader.ReadByte();
        switch (messageType) {
            case NetMessageType.UpdateMagicStorage:
                ReceiveUpdateMagicStorage(reader, sender);
                break;
        }
    }

    /// <summary>
    ///     sends message to clients to update their MS networks at a point
    /// </summary>
    /// <remarks>only affects server-side (netmode is 2)</remarks>
    public static void SendUpdateMagicStorage(int x, int y) {
        if (Main.netMode != NetmodeID.Server) return;

        ModPacket packet = SpawnHousesMod.Instance.GetPacket();
        packet.Write((byte)NetMessageType.UpdateMagicStorage);
        packet.Write(x);
        packet.Write(y);
        packet.Send();
    }

    /// <summary>
    ///     receives message from server to update MS networks at a point
    /// </summary>
    public static void ReceiveUpdateMagicStorage(BinaryReader reader, int sender) {
        CompatabilityHelper.UpdateStorageNetwork(reader.ReadInt32(), reader.ReadInt32());
    }

    /// <summary>
    ///     sends a tile square to clients. functions exactly the same as <see cref="NetMessage.SendTileSquare(int, int, int, int, int, TileChangeType)" />
    /// </summary>
    /// <param name="whoAmI"></param>
    /// <param name="boundingBox"></param>
    public static void SendTileSquare(int whoAmI, TileBox boundingBox) {
        NetMessage.SendTileSquare(whoAmI, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
    }
}