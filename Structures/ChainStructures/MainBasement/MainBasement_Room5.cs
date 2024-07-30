using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.ChainStructures.MainBasement;

public class MainBasement_Room5 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room5";
    public static readonly string _filePath_magicstorage = "Structures/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage";
    
    public static readonly ushort _structureXSize = 22;
    public static readonly ushort _structureYSize = 9;

    public static readonly sbyte _boundingBoxMargin = 0;
    
    public static readonly Floor[] _floors = 
    [
        new Floor(0, 8, 22)
    ];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 8, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];
    
    public MainBasement_Room5(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000) : 
        base(SpawnHousesModHelper.IsMSEnabled? _filePath_magicstorage : _filePath, 
            _structureXSize, _structureYSize, CopyFloors(_floors), 
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
        FilePath = SpawnHousesModHelper.IsMSEnabled? _filePath_magicstorage : _filePath;
        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
        X = x;
        Y = y;
        Cost = cost;
        Weight = weight;
        BoundingBoxMargin = (byte)_boundingBoxMargin;
            
        SetSubstructurePositions();
    }

    public override void OnFound()
    {
        if (SpawnHousesModHelper.IsMSEnabled && FilePath == _filePath_magicstorage)
        {
            Terraria.WorldGen.PlaceTile(X + 11, Y + 7, SpawnHousesModHelper.RemoteAccessTileID);
            TileEntity.PlaceEntityNet(X + 10, Y + 6, SpawnHousesModHelper.RemoteAccessTileEntityID);
            
            if (SpawnHousesSystem.MainHouse.Status != StructureStatus.NotGenerated)
                SpawnHousesModHelper.LinkRemoteStorage(
                    new Point16(X + 10, Y + 6),
                    SpawnHousesSystem.MainHouse.StorageHeartPos
                );
            
            Terraria.WorldGen.PlaceTile(X + 9, Y + 4, SpawnHousesModHelper.StorageUnitTileID);
            TileEntity.PlaceEntityNet(X + 8, Y + 3, SpawnHousesModHelper.StorageUnitTileEntityID);
            
            Terraria.WorldGen.PlaceTile(X + 13, Y + 4, SpawnHousesModHelper.StorageUnitTileID);
            TileEntity.PlaceEntityNet(X + 12, Y + 3, SpawnHousesModHelper.StorageUnitTileEntityID);
            
            Terraria.WorldGen.PlaceTile(X + 15, Y + 4, SpawnHousesModHelper.StorageUnitTileID);
            TileEntity.PlaceEntityNet(X + 14, Y + 3, SpawnHousesModHelper.StorageUnitTileEntityID);
            
            Terraria.WorldGen.PlaceTile(X + 7, Y + 7, SpawnHousesModHelper.StorageUnitTileID);
            TileEntity.PlaceEntityNet(X + 6, Y + 6, SpawnHousesModHelper.StorageUnitTileEntityID);
            
            Terraria.WorldGen.PlaceTile(X + 9, Y + 7, SpawnHousesModHelper.StorageUnitTileID);
            TileEntity.PlaceEntityNet(X + 8, Y + 6, SpawnHousesModHelper.StorageUnitTileEntityID);
            
            Terraria.WorldGen.PlaceTile(X + 13, Y + 7, SpawnHousesModHelper.StorageUnitTileID);
            TileEntity.PlaceEntityNet(X + 12, Y + 6, SpawnHousesModHelper.StorageUnitTileEntityID);
            
            Floors[0].GenerateCobwebs(StructureYSize);
            FrameTiles();
        }
    }

    public override void Generate()
    {
        
        base.Generate();
        if (!SpawnHousesModHelper.IsMSEnabled)
            Floors[0].GenerateCobwebs(StructureYSize);
        
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize), Actions.Chain(
            new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe),
            new Actions.PlaceTile(TileID.Dirt)
        ));
        
        FrameTiles();
    }

    public override MainBasement_Room5 Clone()
    {
        return new MainBasement_Room5(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}