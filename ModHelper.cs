using System;
using MagicStorage.Components;
using SpawnHouses.Testing;
using Terraria.DataStructures;
using Terraria.ModLoader;

// ReSharper disable InconsistentNaming

namespace SpawnHouses;

public class ModHelper : ModSystem {
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

            //AutomatonEntityID = ModContent.NPCType<MagicStorage.NPCs.Golem>()
        }
        catch (Exception) {
            IsMSEnabled = false;
            ErrorLoadingMS = true;
            ModContent.GetInstance<SpawnHouses>().Logger
                .Error("Failed to retrieve Magic Storage TileIDs. Contact the mod author about this issue");
        }
    }


    [JITWhenModsEnabled("MagicStorage")]
    public static bool LinkRemoteStorage(Point16 remotePos, Point16 heartPos) {
        if (!IsMSEnabled) return false;

        void SendError() {
            ModContent.GetInstance<SpawnHouses>().Logger
                .Error(
                    "Failed to link Magic Storage's remote storage to storage heart. Contact the mod author about this issue");
        }

        try {
            TileEntity.ByPosition.TryGetValue(remotePos, out var tileEntity);
            var remoteTileEntity = (TERemoteAccess)tileEntity;
            if (remoteTileEntity == null) {
                SendError();
                return false;
            }

            var success = remoteTileEntity.TryLocate(heartPos, out var message);
            if (!success) SendError();
            return success;
        }
        catch (Exception) {
            SendError();
            return false;
        }
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