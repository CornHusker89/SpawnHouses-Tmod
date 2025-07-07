using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Enums;
using SpawnHouses.Helpers;
using SpawnHouses.Structures.Chains;
using SpawnHouses.Structures.Structures;
using SpawnHouses.Types;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpawnHouses;

#nullable enable

internal class StructureManager : ModSystem {
    public static Version WorldModVersion = SpawnHousesMod.Instance.Version;

    public static MainHouse? MainHouse;
    public static MainBasement? MainBasement;
    public static Mineshaft? Mineshaft;
    public static BeachHouse? BeachHouse;

    public static List<BoundingBox> MainBasementBoundingBoxes = [];

    public override void SaveWorldData(TagCompound tag) {
        tag["WorldModVersion"] = WorldModVersion;

        tag["MainHouse"] = MainHouse;
        tag["MainBasement"] = MainBasement;
        tag["Mineshaft"] = Mineshaft;
        tag["BeachHouse"] = BeachHouse;
    }

    public override void LoadWorldData(TagCompound tag) {
        // "WorldVersion" is the old name
        WorldModVersion = tag.ContainsKey("WorldModVersion")
            ? new Version(tag.GetString("WorldModVersion"))
            : tag.ContainsKey("WorldVersion")
                ? new Version(tag.GetString("WorldVersion"))
                : new Version("0.3.2");

        if (WorldModVersion.Major < 1) {
            // the rest are unrecoverable. mainhouse might use just 1 structure, basement uses seeds, mineshaft doesn't exist
            BeachHouse = tag.ContainsKey("BeachHouse") ? tag.Get<BeachHouse>("BeachHouse") : null;
        }
        else {
            MainHouse = tag.ContainsKey("MainHouse") ? tag.Get<MainHouse>("MainHouse") : null;
            MainBasement = tag.ContainsKey("MainBasement") ? tag.Get<MainBasement>("MainBasement") : null;
            Mineshaft = tag.ContainsKey("Mineshaft") ? tag.Get<Mineshaft>("Mineshaft") : null;
            BeachHouse = tag.ContainsKey("BeachHouse") ? tag.Get<BeachHouse>("BeachHouse") : null;

            MainBasement?.ActionOnEachStructure(structure => {
                MainBasementBoundingBoxes.AddRange(structure.StructureBoundingBoxes);
            });
        }
    }

    public override void ClearWorld() {
        WorldModVersion = SpawnHousesMod.Instance.Version;
        MainHouse = null;
        MainBasement = null;
        Mineshaft = null;
        BeachHouse = null;
    }
}

internal static class ChainProcessor {
    internal static Dictionary<string, object> SerializeChain(CustomChainStructure processingStructure) {
        var dict = new Dictionary<string, object> {
            ["ID"] = (ushort)processingStructure.Id,
            ["X"] = processingStructure.X,
            ["Y"] = processingStructure.Y,
            ["Status"] = processingStructure.Status
        };

        int i = 0;
        processingStructure.ActionOnEachConnectPoint(connectPoint => {
            if (connectPoint.ChildStructure is not null) {
                dict[$"Substructure{i}"] = SerializeChain(connectPoint.ChildStructure);
                dict[$"Substructure{i}Bridge"] = new Dictionary<string, object> {
                    ["ID"] = (ushort)connectPoint.ChildBridge.Id,
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

    internal static CustomChainStructure DeserializeChain(StructureChain structureChain, TagCompound structureDict) {
        CustomChainStructure? structure = (CustomChainStructure)StructureIdHelper.CreateStructure(
            (ushort)(short)structureDict["ID"],
            (ushort)(short)structureDict["X"],
            (ushort)(short)structureDict["Y"],
            (byte)structureDict["Status"]
        );

        int i = 0;
        structure.ParentStructureChain = structureChain;
        structure.ActionOnEachConnectPoint(point => {
            if (structureDict.ContainsKey($"Substructure{i}")) {
                point.ChildStructure = DeserializeChain(structureChain, (TagCompound)structureDict[$"Substructure{i}"]);
                if (structureDict.ContainsKey($"Substructure{i}Bridge")) {
                    TagCompound? bridgeDict = (TagCompound)structureDict[$"Substructure{i}Bridge"];
                    Bridge? bridge = BridgeIdHelper.CreateBridge((ushort)(short)bridgeDict["ID"]);

                    // get the child connect point
                    ushort goalX = (ushort)(short)bridgeDict["X2"];
                    ushort goalY = (ushort)(short)bridgeDict["Y2"];
                    bool found = false;
                    point.ChildStructure.ActionOnEachConnectPoint(nextPoint => {
                        if (nextPoint.X == goalX && nextPoint.Y == goalY) {
                            found = true;
                            point.ChildConnectPoint = nextPoint;
                            bridge.SetPoints(point, nextPoint);
                        }
                    });
                    if (!found) throw new Exception("Bridge loading failed");

                    point.ChildBridge = bridge;
                }
            }

            i++;
        });

        return structure;
    }
}

internal class DictionarySerializer : TagSerializer<Dictionary<string, object>, TagCompound> {
    public override TagCompound Serialize(Dictionary<string, object> data) {
        TagCompound tag = new();
        foreach (var kvp in data)
            tag[kvp.Key] = kvp.Value;
        return tag;
    }

    public override Dictionary<string, object> Deserialize(TagCompound tag) {
        return tag.ToDictionary();
    }
}

internal class MainHouseSerializer : TagSerializer<MainHouse, TagCompound> {
    public override TagCompound Serialize(MainHouse structure) {
        return new TagCompound {
            ["X"] = structure.X,
            ["Y"] = structure.Y,
            ["Status"] = structure.Status,
            ["HasBasement"] = structure.HasBasement,
            ["InUnderworld"] = structure.InUnderworld,
            ["LeftType"] = structure.LeftType,
            ["RightType"] = structure.RightType
        };
    }

    public override MainHouse Deserialize(TagCompound tag) {
        return new MainHouse(
            tag.Get<ushort>("X"),
            tag.Get<ushort>("Y"),
            tag.GetByte("Status"),
            tag.GetBool("HasBasement"),
            tag.GetBool("InUnderworld"),
            tag.GetByte("LeftType") != 0 ? tag.GetByte("LeftType") : (byte)1, // if its 0 (which only happens if it's a <= v0.2.7 world) set to default (large)
            tag.GetByte("RightType") != 0 ? tag.GetByte("RightType") : (byte)1
        );
    }
}

internal class MainBasementSerializer : TagSerializer<MainBasement, TagCompound> {
    public override TagCompound Serialize(MainBasement chain) {
        return new TagCompound {
            ["X"] = chain.EntryPosX,
            ["Y"] = chain.EntryPosY,
            ["Status"] = chain.Status,
            ["RootStructure"] = ChainProcessor.SerializeChain(chain.RootStructure)
        };
    }

    public override MainBasement Deserialize(TagCompound tag) {
        MainBasement basement = new(
            (ushort)tag.Get<short>("X"),
            (ushort)tag.Get<short>("Y"),
            tag.GetByte("Status")
        );
        basement.RootStructure = ChainProcessor.DeserializeChain(basement, (TagCompound)tag["RootStructure"]);
        return basement;
    }
}

internal class MineshaftSerializer : TagSerializer<Mineshaft, TagCompound> {
    public override TagCompound Serialize(Mineshaft structure) {
        return new TagCompound {
            ["X"] = structure.X,
            ["Y"] = structure.Y,
            ["Status"] = structure.Status
        };
    }

    public override Mineshaft Deserialize(TagCompound tag) {
        return new Mineshaft(
            tag.Get<ushort>("X"),
            tag.Get<ushort>("Y"),
            tag.GetByte("Status")
        );
    }
}

internal class BeachHouseSerializer : TagSerializer<BeachHouse, TagCompound> {
    public override TagCompound Serialize(BeachHouse structure) {
        return new TagCompound {
            ["X"] = structure.X,
            ["Y"] = structure.Y,
            ["Status"] = structure.Status,
            ["Reverse"] = structure.Reverse,
            ["HasDeck"] = structure.HasDeck
        };
    }

    public override BeachHouse Deserialize(TagCompound tag) {
        return new BeachHouse(
            tag.Get<ushort>("X"),
            tag.Get<ushort>("Y"),
            tag.GetByte("Status"),
            tag.GetBool("Reverse"),
            tag.ContainsKey("HasDeck") && tag.GetBool("HasDeck")
        );
    }
}