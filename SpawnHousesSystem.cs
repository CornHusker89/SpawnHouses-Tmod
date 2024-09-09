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
    public static MainHouseStructure MainHouse = new MainHouseStructure();
    public static MainBasementChain MainBasement = new MainBasementChain(empty: true);
    public static BeachHouseStructure BeachHouse = new BeachHouseStructure();

    public static string WorldConfig = "";
    
    public override void SaveWorldData(TagCompound tag) {
        tag["MainHouse"] = MainHouse;
        tag["MainBasement"] = MainBasement;
        tag["BeachHouse"] = BeachHouse;
        
        tag["WorldConfig"] = WorldConfig;
    }
    public override void LoadWorldData(TagCompound tag)
    {
        MainHouse = tag.ContainsKey("MainHouse") ? tag.Get<MainHouseStructure>("MainHouse") : new MainHouseStructure();
        MainBasement = tag.ContainsKey("MainBasement") ? tag.Get<MainBasementChain>("MainBasement") : new MainBasementChain(empty: true);
        BeachHouse = tag.ContainsKey("BeachHouse") ? tag.Get<BeachHouseStructure>("BeachHouse") : new BeachHouseStructure();
        
        WorldConfig = tag.ContainsKey("WorldConfig") ? tag.GetString("WorldConfig") : "";
    }
}


internal class MainHouseStructureSerializer : TagSerializer<MainHouseStructure, TagCompound>
{
    public override TagCompound Serialize(MainHouseStructure structure) => 
        new TagCompound
    {
        ["X"] = structure.X,
        ["Y"] = structure.Y,
        ["Status"] = structure.Status,
        ["HasBasement"] = structure.HasBasement,
        ["InUnderworld"] = structure.InUnderworld,
        ["LeftType"] = structure.LeftType,
        ["RightType"] = structure.RightType
    };

    public override MainHouseStructure Deserialize(TagCompound tag) => new MainHouseStructure(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("Y"),
        tag.GetByte("Status"),
        tag.GetBool("HasBasement"),
        tag.GetBool("InUnderworld"),
        tag.GetByte("LeftType") != 0? tag.GetByte("LeftType") : (byte)1, // if its 0 (which only happens if it's a <= v2.7 world) set to default (large) 
        tag.GetByte("RightType") != 0? tag.GetByte("RightType") : (byte)1
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
        tag.GetInt("Seed"),
        tag.GetByte("Status")
    );
}

internal class BeachHouseStructureSerializer : TagSerializer<BeachHouseStructure, TagCompound>
{
    public override TagCompound Serialize(BeachHouseStructure structure) => new TagCompound
    {
        ["X"] = structure.X,
        ["Y"] = structure.Y,
        ["Status"] = structure.Status,
        ["Reverse"] = structure.Reverse,
    };

    public override BeachHouseStructure Deserialize(TagCompound tag) => new BeachHouseStructure(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("Y"),
        tag.GetByte("Status"),
        tag.GetBool("Reverse")
    );
}

