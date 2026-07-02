using System.IO;
using SpawnHouses.Helpers;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesMod : Mod {
    public static readonly Mod Instance = ModContent.GetInstance<SpawnHousesMod>();
    public static readonly WebClient WebClient = new();

    public override void HandlePacket(BinaryReader reader, int whoAmI) {
        NetHelper.HandlePacket(reader, whoAmI);
    }
}