using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureChains;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace SpawnHouses;

internal class SpawnHousesSystem : ModSystem
{
    public static CustomStructure MainHouse { get; set; }
    public static StructureChain MainBasement { get; set; }
    public static CustomStructure BeachHouse { get; set; }
    
    
    public override void SaveWorldData(TagCompound tag) {
        tag["MainHouse"] = MainHouse;
        tag["MainBasement"] = MainBasement;
        tag["BeachHouse"] = BeachHouse;
    }

    public override void LoadWorldData(TagCompound tag) {
        MainHouse = tag.Get<MainHouseStructure>("MainHouse");
        MainBasement = tag.Get<MainBasementChain>("MainBasement");
        BeachHouse = tag.Get<BeachHouseStructure>("BeachHouse");
    }
}



internal class MainHouseStructureSerializer : TagSerializer<MainHouseStructure, TagCompound>
{
    public override TagCompound Serialize(MainHouseStructure structure) => new TagCompound
    {
        ["X"] = structure.X,
        ["Y"] = structure.Y,
        ["Status"] = structure.Status,
        ["HasBasement"] = structure.HasBasement,
        ["InUnderworld"] = structure.InUnderworld
    };

    public override MainHouseStructure Deserialize(TagCompound tag) => new MainHouseStructure(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("X"),
        tag.GetByte("Status"),
        tag.GetBool("HasBasement"),
        tag.GetBool("InUnderworld")
    );
}

internal class MainBasementChainSerializer : TagSerializer<MainBasementChain, TagCompound>
{
    public override TagCompound Serialize(MainBasementChain chain) => new TagCompound
    {
        ["X"] = chain.EntryPosX,
        ["Y"] = chain.EntryPosY,
        ["Seed"] = chain.Seed,
        ["Status"] = chain.Status
    };

    public override MainBasementChain Deserialize(TagCompound tag) => new MainBasementChain(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("Y"),
        tag.GetInt("Seed")
    );
}

internal class BeachHouseStructureSerializer : TagSerializer<BeachHouseStructure, TagCompound>
{
    public override TagCompound Serialize(BeachHouseStructure structure) => new TagCompound
    {
        ["X"] = structure.X,
        ["Y"] = structure.Y,
        ["Reverse"] = structure.Reverse,
    };

    public override BeachHouseStructure Deserialize(TagCompound tag) => new BeachHouseStructure(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("X"),
        tag.GetByte("Status"),
        tag.GetBool("Reverse")
    );
}

