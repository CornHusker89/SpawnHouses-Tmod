using System;
using System.IO;
using MagicStorage.Components;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types.Enums;
using StructureHelper.API;
using StructureHelper.Models;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

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

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="tilemap"></param>
    /// <param name="x">tilemap x</param>
    /// <param name="y">tilemap top y pos</param>
    /// <param name="collDx">column number in the data file</param>
    /// <typeparam name="TType"></typeparam>
    private static void ExportShDataColumn<TType>(StructureData data, StructureTilemap tilemap, int x, int y, int collDx)
        where TType : unmanaged, ITileData {
        ITileDataEntry dataEntry = data.dataEntries[$"Terraria/{typeof(TType).Name}"];

        byte[] fileData = dataEntry.GetData();
        int dataSingleSize = dataEntry.GetSingleSize();

        for (int i = 0; i < data.height; i++) {
            int fileDataOffset = collDx * data.height * dataSingleSize;
            byte[] dataSingle = fileData[new Range(fileDataOffset + i * dataSingleSize, fileDataOffset + (i + 1) * dataSingleSize)];

            // interpret data
            StructureTile tile = tilemap[x, y + i];
            switch (typeof(TType).Name) {
                case nameof(TileTypeData):
                    tile.TileType = (ushort)((dataSingle[1] << 8) + dataSingle[0]);
                    break;

                case nameof(WallTypeData):
                    tile.WallType = (ushort)((dataSingle[1] << 8) + dataSingle[0]);
                    break;

                case nameof(TileWallWireStateData):
                    int bitPack = (dataSingle[3] << 24) + (dataSingle[2] << 16) + (dataSingle[1] << 8) + dataSingle[0];
                    tile.HasTile = TileDataPacking.GetBit(bitPack, 0);
                    tile.IsActuated = TileDataPacking.GetBit(bitPack, 1);
                    tile.HasActuator = TileDataPacking.GetBit(bitPack, 2);
                    tile.TileColor = (byte)TileDataPacking.Unpack(bitPack, 3, 5);
                    tile.WallColor = (byte)TileDataPacking.Unpack(bitPack, 8, 5);
                    if (TileDataPacking.GetBit(bitPack, 24)) // isHalfBlock
                        tile.BlockType = BlockType.HalfBlock;
                    else
                        tile.BlockType = TileDataPacking.Unpack(bitPack, 25, 3) switch {
                            1 => BlockType.SlopeDownLeft,
                            2 => BlockType.SlopeDownRight,
                            3 => BlockType.SlopeUpLeft,
                            4 => BlockType.SlopeUpRight,
                            0 => BlockType.Solid
                        };
                    break;
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
    public static void PlaceShStructure(StructureTilemap tilemap, string filepath, Point16 offset) {
        StructureData data = Generator.GetStructureData(filepath, SpawnHousesMod.Instance);

        for (int k = 0; k < data.width; k++)
            if (!data.slowColumns[k]) {
                ExportShDataColumn<TileTypeData>(data, tilemap, offset.X + k, offset.Y, k);
                ExportShDataColumn<WallTypeData>(data, tilemap, offset.X + k, offset.Y, k);
                ExportShDataColumn<TileWallWireStateData>(data, tilemap, offset.X + k, offset.Y, k);
            }
        // data.ExportDataColumnSlow<TileTypeData>(offset.X + k, offset.Y, k, null);
        // data.ExportDataColumnSlow<WallTypeData>(offset.X + k, offset.Y, k, null);
        // data.ExportDataColumnSlow<LiquidData>(offset.X + k, offset.Y, k, null);
        // data.ExportDataColumnSlow<TileWallBrightnessInvisibilityData>(offset.X + k, offset.Y, k, null);
        // data.ExportDataColumnSlow<TileWallWireStateData>(offset.X + k, offset.Y, k, null);
    }
}