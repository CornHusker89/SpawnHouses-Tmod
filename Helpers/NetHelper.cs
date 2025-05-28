using System;
using System.IO;
using SpawnHouses.Enums;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Helpers;

public static class NetHelper {
    public static void HandlePacket(BinaryReader reader, int sender)
    {
        NetMessageType messageType = (NetMessageType) reader.ReadByte();
        switch (messageType) {
            case NetMessageType.UpdateMagicStorage:
                ReceiveUpdateMagicStorage(reader, sender);
                break;
        }
    }

    /// <summary>
    /// sends message to clients to update their MS networks at a point
    /// </summary>
    /// <remarks>only has an effect on server-side (netmode is 2)</remarks>
    public static void SendUpdateMagicStorage(int x, int y)
    {
        if (Main.netMode != NetmodeID.Server) {
            return;
        }

        ModPacket packet = SpawnHousesMod.Instance.GetPacket();
        packet.Write((byte) NetMessageType.UpdateMagicStorage);
        packet.Write(x);
        packet.Write(y);
        packet.Send();
    }

    /// <summary>
    ///
    /// </summary>
    public static void ReceiveUpdateMagicStorage(BinaryReader reader, int sender)
    {
        Console.WriteLine("received update");
        CompatabilityHelper.UpdateStorageNetwork(reader.ReadInt32(), reader.ReadInt32());
    }
}