using System;
using System.Collections.Generic;
using System.IO;
using MagicStorage.Components;
using SpawnHouses.Core.Enums;
using SpawnHouses.Core.Tiles;
using StructureHelper.Models;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Generator = StructureHelper.API.Generator;

namespace SpawnHouses.Helpers;

public class CompatabilityHelper : ModSystem {
    public override void OnModLoad() {
    }
    
    [JITWhenModsEnabled("MagicStorage")]
    public static void LinkRemoteStorage(Point16 remotePos, Point16 heartPos) {
        try {
            TileEntity.ByPosition.TryGetValue(remotePos, out TileEntity tileEntity);
            TERemoteAccess remoteTileEntity = (TERemoteAccess)tileEntity;
            if (remoteTileEntity == null) {
                SendError();
                return;
            }

            bool success = remoteTileEntity.TryLocate(heartPos, out string message);
            if (!success) SendError();
        }
        catch (Exception) {
            SendError();
        }

        return;

        void SendError() {
            SpawnHousesMod.Instance.Logger.Error("Failed to link Magic Storage's remote storage to storage heart. Contact the Generated Housing's mod author about this issue");
        }
    }

    //[JITWhenModsEnabled("MagicStorage")]
    public static void PlaceMsModule(int x, int y, int tileId, int entityId) {
        WorldGen.PlaceTile(x + 1, y + 1, tileId);
        TileEntity.PlaceEntityNet(x, y, entityId);

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendTileSquare(-1, x, y, 2, 2);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: x, number2: y, number3: entityId);
        }
    }

    /// <summary>
    ///     sends message to clients to update their MS networks at a point
    /// </summary>
    /// <remarks>only affects server-side (netmode is 2)</remarks>
    public static void SendUpdateMs(int x, int y) {
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
    public static void ReceiveUpdateMs(BinaryReader reader, int sender) {
        UpdateMsNetwork(reader.ReadInt32(), reader.ReadInt32());
    }

    /// <summary>
    ///     updates local MS network at target location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    [JITWhenModsEnabled("MagicStorage")]
    public static void UpdateMsNetwork(int x, int y) {
        MagicStorage.NetHelper.SendSearchAndRefresh(x, y);
    }

    private static void ExportShSingleData(StructureTilemap tilemap, string type, byte[] data, int x, int y) {
        StructureTile tile = tilemap[x, y];
        switch (type) {
            case nameof(TileTypeData):
                tile.TileType = (ushort)((data[1] << 8) + data[0]);
                break;

            case nameof(WallTypeData):
                tile.WallType = (ushort)((data[1] << 8) + data[0]);
                break;

            case nameof(TileWallWireStateData):
                int tileFrameXBitPack = (data[1] << 8) + data[0];
                tile.TileFrameX = (short)tileFrameXBitPack;

                int tileFrameYBitPack = (data[3] << 8) + data[2];
                tile.TileFrameY = (short)tileFrameYBitPack;

                int tileDataBitPack = (data[7] << 24) + (data[6] << 16) + (data[5] << 8) + data[4];
                tile.HasTile = TileDataPacking.GetBit(tileDataBitPack, 0);
                tile.IsActuated = TileDataPacking.GetBit(tileDataBitPack, 1);
                tile.HasActuator = TileDataPacking.GetBit(tileDataBitPack, 2);
                tile.TileColor = (byte)TileDataPacking.Unpack(tileDataBitPack, 3, 5);
                tile.WallColor = (byte)TileDataPacking.Unpack(tileDataBitPack, 8, 5);
                if (TileDataPacking.GetBit(tileDataBitPack, 24)) // isHalfBlock
                    tile.BlockType = BlockType.HalfBlock;
                else
                    tile.BlockType = TileDataPacking.Unpack(tileDataBitPack, 25, 3) switch {
                        1 => BlockType.SlopeDownLeft,
                        2 => BlockType.SlopeDownRight,
                        3 => BlockType.SlopeUpLeft,
                        4 => BlockType.SlopeUpRight,
                        0 => BlockType.Solid
                    };
                break;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="tilemap"></param>
    /// <param name="x">tilemap x</param>
    /// <param name="y">tilemap top y pos</param>
    /// <param name="collDx">column number in the data file</param>
    /// <typeparam name="T"></typeparam>
    private static void ExportShDataColumn<T>(StructureData data, StructureTilemap tilemap, int x, int y, int collDx)
        where T : unmanaged, ITileData {
        ITileDataEntry dataEntry = data.dataEntries[$"Terraria/{typeof(T).Name}"];

        byte[] fileData = dataEntry.GetData();
        int dataSingleSize = dataEntry.GetSingleSize();

        for (int rowIdx = 0; rowIdx < data.height; rowIdx++) {
            int fileDataOffset = collDx * data.height * dataSingleSize;
            byte[] dataSingle = fileData[new Range(fileDataOffset + rowIdx * dataSingleSize, fileDataOffset + (rowIdx + 1) * dataSingleSize)];
            ExportShSingleData(tilemap, typeof(T).Name, dataSingle, x, y + rowIdx);
        }
    }

    private static void ExportShDataColumnSlow<T>(StructureData data, StructureTilemap tilemap, int x, int y, int collDx) {
        string key = $"Terraria/{typeof(T).Name}";

        byte[] fileData = data.dataEntries[key].GetData();
        int dataSingleSize = data.dataEntries[key].GetSingleSize();
        int fileDataOffset = collDx * data.height * dataSingleSize;

        byte[] tileTypeData = data.dataEntries["Terraria/TileTypeData"].GetData();
        int tileTypeSingleSize = data.dataEntries["Terraria/TileTypeData"].GetSingleSize();
        int tileTypeOffset = collDx * data.height * tileTypeSingleSize;

        byte[] wallTypeData = data.dataEntries["Terraria/WallTypeData"].GetData();
        int wallTypeSingleSize = data.dataEntries["Terraria/TileTypeData"].GetSingleSize();
        int wallTypeOffset = collDx * data.height * wallTypeSingleSize;

        if (key != "Terraria/WallTypeData")
            for (int rowIdx = 0; rowIdx < data.height; rowIdx++) {
                byte[] dataSingle = fileData[new Range(fileDataOffset + rowIdx * dataSingleSize, fileDataOffset + (rowIdx + 1) * dataSingleSize)];
                byte[] tileTypeDataSingle = tileTypeData[new Range(tileTypeOffset + rowIdx * tileTypeSingleSize, tileTypeOffset + (rowIdx + 1) * tileTypeSingleSize)];

                if ((ushort)((tileTypeDataSingle[1] << 8) + tileTypeDataSingle[0]) != StructureHelper.StructureHelper.NULL_IDENTIFIER) {
                    ExportShSingleData(tilemap, typeof(T).Name, dataSingle, x, y + rowIdx);
                    tilemap[x, y + rowIdx].IsNullTile = false;
                }
            }
        else
            for (int rowIdx = 0; rowIdx < data.height; rowIdx++) {
                byte[] dataSingle = fileData[new Range(fileDataOffset + rowIdx * dataSingleSize, fileDataOffset + (rowIdx + 1) * dataSingleSize)];
                byte[] wallTypeDataSingle = wallTypeData[new Range(wallTypeOffset + rowIdx * wallTypeSingleSize, wallTypeOffset + (rowIdx + 1) * wallTypeSingleSize)];

                if ((ushort)((wallTypeDataSingle[1] << 8) + wallTypeDataSingle[0]) != StructureHelper.StructureHelper.NULL_IDENTIFIER) {
                    ExportShSingleData(tilemap, typeof(T).Name, dataSingle, x, y + rowIdx);
                    tilemap[x, y + rowIdx].IsNullWall = false;
                }
            }
    }

    /// <summary>
    ///     places a structure helper file at a local position in a tilemap
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="filepath"></param>
    /// <param name="offset"></param>
    // note: don't need conditional JIT because structurehelper is required
    public static List<(StructureNBTEntry nbt, Point16 localPos)> PlaceShStructure(StructureTilemap tilemap, string filepath, Point16 offset) {
        StructureData data = Generator.GetStructureData(filepath, SpawnHousesMod.Instance);

        for (int k = 0; k < data.width; k++) {
            if (!data.slowColumns[k]) {
                ExportShDataColumn<TileTypeData>(data, tilemap, offset.X + k, offset.Y, k);
                ExportShDataColumn<WallTypeData>(data, tilemap, offset.X + k, offset.Y, k);
                ExportShDataColumn<TileWallWireStateData>(data, tilemap, offset.X + k, offset.Y, k);
            }
            else {
                ExportShDataColumnSlow<TileTypeData>(data, tilemap, offset.X + k, offset.Y, k);
                ExportShDataColumnSlow<WallTypeData>(data, tilemap, offset.X + k, offset.Y, k);
                ExportShDataColumnSlow<TileWallWireStateData>(data, tilemap, offset.X + k, offset.Y, k);
            }
        }

        if (!data.containsNbt)
            return [];

        List<(StructureNBTEntry nbt, Point16 localPos)> result = [];
        foreach (StructureNBTEntry nbt in data.nbtData) {
            result.Add((nbt, new Point16(nbt.x + offset.X, nbt.y + offset.Y)));
        }

        return result;
    }
}