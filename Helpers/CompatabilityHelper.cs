using System;
using MagicStorage.Components;
using SpawnHouses.Testing;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

// ReSharper disable InconsistentNaming

namespace SpawnHouses.Helpers;

public class CompatabilityHelper : ModSystem {
    public static bool IsMSEnabled;
    public static bool ErrorLoadingMS;

    public static int CraftingAccessTileID;
    public static int CraftingAccessTileEntityID;
    public static int RemoteAccessTileID;
    public static int RemoteAccessTileEntityID;
    public static int StorageAccessTileID;
    public static int StorageAccessTileEntityID;
    public static int StorageHeartTileID;
    public static int StorageHeartTileEntityID;
    public static int StorageUnitTileID;
    public static int StorageUnitTileEntityID;
    public static int EnviromentAccessTileID;
    public static int EnviromentAccessTileEntityID;
    public static int AutomatonNpcID;

    public static bool IsRemnantsEnabled;

    public static bool IsWorldGenTestingEnabled;


    [JITWhenModsEnabled("MagicStorage")]
    public static void GetMagicStorage() {
        IsMSEnabled = true;

        try {
            CraftingAccessTileID = ModContent.TileType<CraftingAccess>();
            CraftingAccessTileEntityID = ModContent.TileEntityType<TECraftingAccess>();
            RemoteAccessTileID = ModContent.TileType<RemoteAccess>();
            RemoteAccessTileEntityID = ModContent.TileEntityType<TERemoteAccess>();
            StorageAccessTileID = ModContent.TileType<StorageAccess>();
            StorageAccessTileEntityID = ModContent.TileEntityType<TEStoragePoint>();
            StorageHeartTileID = ModContent.TileType<StorageHeart>();
            StorageHeartTileEntityID = ModContent.TileEntityType<TEStorageHeart>();
            StorageUnitTileID = ModContent.TileType<StorageUnit>();
            StorageUnitTileEntityID = ModContent.TileEntityType<TEStorageUnit>();
            EnviromentAccessTileID = ModContent.TileType<EnvironmentAccess>();
            EnviromentAccessTileEntityID = ModContent.TileEntityType<TEEnvironmentAccess>();
            AutomatonNpcID = ModContent.Find<ModNPC>("MagicStorage/Golem").Type;
        }
        catch (Exception) {
            IsMSEnabled = false;
            ErrorLoadingMS = true;
            ModContent.GetInstance<SpawnHousesMod>().Logger.Error("Failed to retrieve Magic Storage IDs. Contact the mod author about this issue");
        }
    }


    [JITWhenModsEnabled("MagicStorage")]
    public static void LinkRemoteStorage(Point16 remotePos, Point16 heartPos) {
        if (!IsMSEnabled) return;

        void SendError() {
            ModContent.GetInstance<SpawnHousesMod>().Logger.Error("Failed to link Magic Storage's remote storage to storage heart. Contact the mod author about this issue");
        }

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
    }

    [JITWhenModsEnabled("MagicStorage")]
    public static void PlaceMSModule(int x, int y, int tileId, int entityId) {
        Terraria.WorldGen.PlaceTile(x + 1, y + 1, tileId);
        TileEntity.PlaceEntityNet(x, y, entityId);

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendTileSquare(-1, x, y, 2, 2);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: x, number2: y, number3: entityId);
        }
    }

    /// <summary>
    ///     updates local MS network at target location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    [JITWhenModsEnabled("MagicStorage")]
    public static void UpdateStorageNetwork(int x, int y) {
        MagicStorage.NetHelper.SendSearchAndRefresh(x, y);
    }


    [JITWhenModsEnabled("Remnants")]
    public static void GetRemnants() {
        IsRemnantsEnabled = true;
    }


    [JITWhenModsEnabled("WorldGenTesting")]
    public static void GetWorldGenTesting() {
        IsWorldGenTestingEnabled = true;
        SpawnHousesTesting.Initialize();
    }


    public override void OnModLoad() {
        if (ModLoader.HasMod("MagicStorage") && ModContent.GetInstance<SpawnHousesConfig>().MagicStorageIntegrations)
            GetMagicStorage();
        if (ModLoader.HasMod("Remnants"))
            GetRemnants();
        if (ModLoader.HasMod("WorldGenTesting"))
            GetWorldGenTesting();
    }
}