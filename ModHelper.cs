using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
// ReSharper disable InconsistentNaming

namespace SpawnHouses;

public class ModHelper : ModSystem
{
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
    
    
    
    [JITWhenModsEnabled("MagicStorage")]
    public static void GetMagicStorage()
    {
        IsMSEnabled = true;

        try
        {
            CraftingAccessTileID = ModContent.TileType<MagicStorage.Components.CraftingAccess>();
            CraftingAccessTileEntityID = ModContent.TileEntityType<MagicStorage.Components.TECraftingAccess>();
            RemoteAccessTileID = ModContent.TileType<MagicStorage.Components.RemoteAccess>();
            RemoteAccessTileEntityID = ModContent.TileEntityType<MagicStorage.Components.TERemoteAccess>();
            StorageAccessTileID = ModContent.TileType<MagicStorage.Components.StorageAccess>();
            StorageAccessTileEntityID = ModContent.TileEntityType<MagicStorage.Components.TEStoragePoint>();
            StorageHeartTileID = ModContent.TileType<MagicStorage.Components.StorageHeart>();
            StorageHeartTileEntityID = ModContent.TileEntityType<MagicStorage.Components.TEStorageHeart>();
            StorageUnitTileID = ModContent.TileType<MagicStorage.Components.StorageUnit>();
            StorageUnitTileEntityID = ModContent.TileEntityType<MagicStorage.Components.TEStorageUnit>();
            EnviromentAccessTileID = ModContent.TileType<MagicStorage.Components.EnvironmentAccess>();
            EnviromentAccessTileEntityID = ModContent.TileEntityType<MagicStorage.Components.TEEnvironmentAccess>();
            
            //AutomatonEntityID = ModContent.NPCType<MagicStorage.NPCs.Golem>()
        }
        catch (Exception)
        {
            IsMSEnabled = false;
            ErrorLoadingMS = true;
            ModContent.GetInstance<SpawnHouses>().Logger.Error("Failed to retrieve Magic Storage TileIDs. Contact the mod author about this issue");
        }
    }
    

    [JITWhenModsEnabled("MagicStorage")]
    public static bool LinkRemoteStorage(Point16 remotePos, Point16 heartPos)
    {
        if (!IsMSEnabled) return false;

        void SendError()
        {
            ModContent.GetInstance<SpawnHouses>().Logger.Error("Failed to link Magic Storage's remote storage to storage heart. Contact the mod author about this issue");
        }
        
        try
        {
            TileEntity.ByPosition.TryGetValue(remotePos, out TileEntity tileEntity);
            MagicStorage.Components.TERemoteAccess remoteTileEntity = (MagicStorage.Components.TERemoteAccess)tileEntity;
            if (remoteTileEntity == null)
            {
                SendError();
                return false;
            }
            bool success = remoteTileEntity.TryLocate(heartPos, out string message);
            if (!success) SendError();
            return success;
        }
        catch (Exception)
        {
            SendError();
            return false;
        }
    }


    [JITWhenModsEnabled("Remnants")]
    public static void GetRemnants()
    {
        IsRemnantsEnabled = true;
    }
    

    public override void OnModLoad()
    {
        if (ModLoader.HasMod("MagicStorage") && ModContent.GetInstance<SpawnHousesConfig>().MagicStorageIntegrations)
            GetMagicStorage();
        if (ModLoader.HasMod("Remnants"))
            GetRemnants();
        
    }
}