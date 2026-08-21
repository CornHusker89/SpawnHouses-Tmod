using System.IO;
using SpawnHouses.Core.Enums;
using SpawnHouses.Helpers;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesMod : Mod {
    public static readonly Mod Instance = ModContent.GetInstance<SpawnHousesMod>();
    public static readonly SpawnHousesConfig Config = ModContent.GetInstance<SpawnHousesConfig>();

    public override void HandlePacket(BinaryReader reader, int sender) {
        NetMessageType messageType = (NetMessageType)reader.ReadByte();
        switch (messageType) {
            case NetMessageType.UpdateMagicStorage:
                CompatabilityHelper.ReceiveUpdateMs(reader, sender);
                break;
        }
    }
}