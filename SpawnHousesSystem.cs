using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Chains;
using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpawnHouses;

#nullable enable

internal class SpawnHousesSystem : ModSystem
{
    public static string WorldVersion = "0.3.2";

    public static MainHouse? MainHouse;
    public static MainBasement? MainBasement;
    public static Mineshaft? Mineshaft;
    public static BeachHouse? BeachHouse;
    
    public override void SaveWorldData(TagCompound tag)
    {
        tag["WorldVersion"] = WorldVersion;
        
        tag["MainHouse"] = MainHouse;
        tag["MainBasement"] = MainBasement;
        tag["Mineshaft"] = Mineshaft;
        tag["BeachHouse"] = BeachHouse;
    }
    public override void LoadWorldData(TagCompound tag)
    {
        WorldVersion = tag.ContainsKey("WorldVersion") ? tag.GetString("WorldVersion") : "0.3.2";
        
        MainHouse = tag.ContainsKey("MainHouse") ? tag.Get<MainHouse>("MainHouse") : null;
        MainBasement = tag.ContainsKey("MainBasement") ? tag.Get<MainBasement>("MainBasement") : null;
        Mineshaft = tag.ContainsKey("Mineshaft") ? tag.Get<Mineshaft>("Mineshaft") : null;
        BeachHouse = tag.ContainsKey("BeachHouse") ? tag.Get<BeachHouse>("BeachHouse") : null;
    }
    public override void ClearWorld()
    {
        WorldVersion = "";
        MainHouse = null;
        MainBasement = null;
        Mineshaft = null;
        BeachHouse = null;
    }
}

internal class DictionarySerializer : TagSerializer<Dictionary<string, object>, TagCompound>
{
    public override TagCompound Serialize(Dictionary<string, object> data) 
    {
        TagCompound tag = new TagCompound();
        foreach (var kvp in data)
            tag[kvp.Key] = kvp.Value;
        return tag;
    }

    public override Dictionary<string, object> Deserialize(TagCompound tag)
    {
        return tag.ToDictionary();
    }
}

internal class MainHouseSerializer : TagSerializer<MainHouse, TagCompound>
{
    public override TagCompound Serialize(MainHouse structure) => new TagCompound
    {
        ["X"] = structure.X,
        ["Y"] = structure.Y,
        ["Status"] = structure.Status,
        ["HasBasement"] = structure.HasBasement,
        ["InUnderworld"] = structure.InUnderworld,
        ["LeftType"] = structure.LeftType,
        ["RightType"] = structure.RightType
    };

    public override MainHouse Deserialize(TagCompound tag) => new MainHouse(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("Y"),
        tag.GetByte("Status"),
        tag.GetBool("HasBasement"),
        tag.GetBool("InUnderworld"),
        tag.GetByte("LeftType") != 0? tag.GetByte("LeftType") : (byte)1, // if its 0 (which only happens if it's a <= v0.2.7 world) set to default (large) 
        tag.GetByte("RightType") != 0? tag.GetByte("RightType") : (byte)1
    );
}

internal class MainBasementSerializer : TagSerializer<MainBasement, TagCompound>
{
    public override TagCompound Serialize(MainBasement chain) 
    {
        chain.ActionOnEachStructure(structure =>
        {
            structure.ActionOnEachConnectPoint((ChainConnectPoint connectPoint) =>
            {
                if (connectPoint.ChildBridge is not null)
                {
                    Bridge bridge = connectPoint.ChildBridge;
                    Console.WriteLine("structure id: " + structure + ",  bridge stuff: " + bridge.Point1.X);

                }
            });
        });
        
        
        
        Dictionary<string, object> ProcessStructure(CustomChainStructure processingStructure)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                ["ID"] = processingStructure.ID,
                ["X"] = processingStructure.X,
                ["Y"] = processingStructure.Y,
                ["Status"] = processingStructure.Status
            };

            int i = 0;
            processingStructure.ActionOnEachConnectPoint((ChainConnectPoint connectPoint) =>
            {
                
                if (connectPoint.ChildStructure is not null)
                {
                    dict[$"Substructure{i}"] = ProcessStructure(connectPoint.ChildStructure);
                    Console.WriteLine("cp2 - " + (connectPoint.ChildBridge.Point2 is null));
                    dict[$"Substructure{i}Bridge"] = new Dictionary<string, object>
                    {
                        ["ID"] = connectPoint.ChildBridge.ID,
                        ["X1"] = connectPoint.ChildBridge.Point1.X,
                        ["Y1"] = connectPoint.ChildBridge.Point1.Y,
                        ["X2"] = connectPoint.ChildBridge.Point2.X,
                        ["Y2"] = connectPoint.ChildBridge.Point2.Y
                    };
                }
                i++;
            });

            return dict;
        }

        return new TagCompound
        {
            ["X"] = chain.EntryPosX,
            ["Y"] = chain.EntryPosY,
            ["Status"] = chain.Status,
            ["RootStructure"] = ProcessStructure(chain.RootStructure)
        };
    }

    public override MainBasement Deserialize(TagCompound tag)
    {
        MainBasement basement = new MainBasement(
            (ushort)tag.Get<short>("X"),
            (ushort)tag.Get<short>("Y"),
            tag.GetByte("Status"),
            generateSubstructures: false
        );

        CustomChainStructure ProcessSubstructure(TagCompound structureObj)
        {
            CustomChainStructure structure = (CustomChainStructure)StructureIDUtils.CreateStructure(
                (ushort)(short)structureObj["ID"],
                (ushort)(short)structureObj["X"],
                (ushort)(short)structureObj["Y"],
                (byte)structureObj["Status"]
            );
            
            int i = 0;
            structure.ActionOnEachConnectPoint((ChainConnectPoint point) =>
            {
                if (structureObj.ContainsKey($"Substructure{i}"))
                {
                    point.ChildStructure = ProcessSubstructure((TagCompound)structureObj[$"Substructure{i}"]);
                    if (structureObj.ContainsKey($"Substructure{i}Bridge"))
                    {
                        Console.WriteLine("loading bridge to substructure id " + point.ChildStructure.ID);
                        TagCompound bridgeObj = (TagCompound)structureObj[$"Substructure{i}Bridge"];
                        Bridge bridge = BridgeIDUtils.CreateBridge((ushort)(short)bridgeObj["ID"]);
                        
                        // get the child connect point
                        ushort goalX = (ushort)(short)bridgeObj["X2"];
                        ushort goalY = (ushort)(short)bridgeObj["Y2"];
                        bool found = false;
                        point.ChildStructure.ActionOnEachConnectPoint((ChainConnectPoint nextPoint) =>
                        {
                            if (nextPoint.X == goalX && nextPoint.Y == goalY)
                            {
                                Console.WriteLine("loading bridge opposite connect points");
                                found = true;
                                point.ChildConnectPoint = nextPoint;
                                bridge.SetPoints(point, nextPoint);
                            }
                        });
                        if (!found)
                        {
                            Console.WriteLine("goal: " + goalX + ", " + goalY);
                            point.ChildStructure.ActionOnEachConnectPoint((ChainConnectPoint nextPoint) =>
                            {
                                Console.WriteLine(nextPoint.X + ", " + nextPoint.Y);
                            });
                        }
                        
                        point.ChildBridge = bridge;
                    }
                }
                i++;
            });

            return structure;
        }
        basement.RootStructure = ProcessSubstructure((TagCompound)tag["RootStructure"]);
        return basement;
    }
}

internal class MineshaftSerializer : TagSerializer<Mineshaft, TagCompound>
{
    public override TagCompound Serialize(Mineshaft structure) => new TagCompound
    {
        ["X"] = structure.X,
        ["Y"] = structure.Y,
        ["Status"] = structure.Status
    };

    public override Mineshaft Deserialize(TagCompound tag) => new Mineshaft(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("Y"),
        tag.GetByte("Status")
    );
}

internal class BeachHouseSerializer : TagSerializer<BeachHouse, TagCompound>
{
    public override TagCompound Serialize(BeachHouse structure) => new TagCompound
    {
        ["X"] = structure.X,
        ["Y"] = structure.Y,
        ["Status"] = structure.Status,
        ["Reverse"] = structure.Reverse,
    };

    public override BeachHouse Deserialize(TagCompound tag) => new BeachHouse(
        tag.Get<ushort>("X"),
        tag.Get<ushort>("Y"),
        tag.GetByte("Status"),
        tag.GetBool("Reverse")
    );
}