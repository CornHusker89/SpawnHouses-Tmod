using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures.Structures.ChainStructures;

// ReSharper disable InconsistentNaming



public class MainBasement_Entry1 : CustomChainStructure
{
    public MainBasement_Entry1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Entry1",
            10,
            16,
            [
                // top
                [
                    new ChainConnectPoint(4, 0, Directions.Up, null, true)
                ],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 15, Directions.Left, new Seal.MainBasement_SealWall(), false)
                ],
        
                // right
                [
                    new ChainConnectPoint(9, 15, Directions.Right, new Seal.MainBasement_SealWall(), false)
                ]
            ],
            x, y, status, cost, weight) 
    {}
}

public class MainBasement_Entry2 : CustomChainStructure
{
    public MainBasement_Entry2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Entry2",
            15,
            15,
            [
                // top
                [
                    new ChainConnectPoint(3, 0, Directions.Up, null, true)
                ],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 14, Directions.Left, new Seal.MainBasement_SealWall(), false)
                ],
        
                // right
                [
                    new ChainConnectPoint(14, 14, Directions.Right, new Seal.MainBasement_SealWall(), false)
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Hallway4 : CustomChainStructure
{
    public MainBasement_Hallway4(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Hallway4",
            6,
            11,
            [
                // top
                [
                    new ChainConnectPoint(2, 0, Directions.Down, new Seal.MainBasement_SealFloor(), true, GenerateChances.Guaranteed)
                ],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 10, Directions.Left, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ],
        
                // right
                [
                    new ChainConnectPoint(5, 10, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Hallway5 : CustomChainStructure
{
    public MainBasement_Hallway5(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Hallway5",
            8,
            22,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true, GenerateChances.Guaranteed),
                    new ChainConnectPoint(1, 21, Directions.Left, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ],
        
                // right
                [
                    new ChainConnectPoint(7, 6, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed),
                    new ChainConnectPoint(6, 21, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ]
            ],
            x, y, status, cost, weight)
    {}
    
    protected override void SetSubstructurePositions()
    {
        base.SetSubstructurePositions();
        
        StructureBoundingBoxes =
        [
            new BoundingBox(X, Y, X + StructureXSize - 1, Y + 7 - 1),
            new BoundingBox(X + 1, Y + 7, X - 1 + StructureXSize - 1, Y + StructureYSize - 1)
        ];
    }
}

public class MainBasement_Hallway9 : CustomChainStructure
{
    public MainBasement_Hallway9(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Hallway9",
            6,
            11,
            [
                // top
                [],
        
                // bottom
                [
                    new ChainConnectPoint(2, 10, Directions.Up, new Seal.MainBasement_SealFloor(), false, GenerateChances.Guaranteed)
                ],
        
                // left
                [
                    new ChainConnectPoint(0, 5, Directions.Left, new Seal.MainBasement_SealWall(), true, GenerateChances.Guaranteed)
                ],
        
                // right
                [
                    new ChainConnectPoint(5, 5, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Room1 : CustomChainStructure
{
    public MainBasement_Room1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room1",
            22,
            9,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 8, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],
        
                // right
                [
                    new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall(), false)
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Room1_WithFloor : CustomChainStructure
{
    public MainBasement_Room1_WithFloor(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room1_WithFloor",
            22,
            9,
            [
                // top
                [],
        
                // bottom
                [
                    new ChainConnectPoint(10, 8, Directions.Down, new Seal.MainBasement_SealFloor(), false),
                ],
        
                // left
                [
                    new ChainConnectPoint(0, 8, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Room2 : CustomChainStructure
{
    public MainBasement_Room2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room2",
            23,
            7,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(22, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Room2_WithRoof : CustomChainStructure
{
    public MainBasement_Room2_WithRoof(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room2_WithRoof",
            23,
            7,
            [
                // top
                [
                    new ChainConnectPoint(3, 0, Directions.Up, new Seal.MainBasement_SealRoof(), false),
                ],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(22, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Room3 : CustomChainStructure
{
    public MainBasement_Room3(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room3",
            10,
            7,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(9, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Room4 : CustomChainStructure
{
    public MainBasement_Room4(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room4",
            13,
            11,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 10, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(12, 10, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class MainBasement_Room5 : CustomChainStructure
{
    public MainBasement_Room5(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base(SpawnHousesModHelper.IsMSEnabled ? "Structures/StructureFiles/mainBasement/mainBasement_Room5" : "Structures/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage",
            22,
            9,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 10, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(12, 10, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
    
    public override void OnFound()
    {
        if (SpawnHousesModHelper.IsMSEnabled && FilePath == "Structures/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage")
        {
            Terraria.WorldGen.PlaceTile(X + 11, Y + 7, SpawnHousesModHelper.RemoteAccessTileID);
            TileEntity.PlaceEntityNet(X + 10, Y + 6, SpawnHousesModHelper.RemoteAccessTileEntityID);
            
            if (SpawnHousesSystem.MainHouse is not null && SpawnHousesSystem.MainHouse.Status != StructureStatus.NotGenerated)
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
            
            GenHelper.GenerateCobwebs(new Point(X, Y), StructureXSize, StructureYSize);
            FrameTiles();
        }
    }
}

public class MainBasement_Room6 : CustomChainStructure
{
    public MainBasement_Room6(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room6",
            28,
            15,
            [
                // top
                [],
        
                // bottom
                [
                    new ChainConnectPoint(15, 14, Directions.Down, new Seal.MainBasement_SealFloor(), false),
                ],
        
                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(15, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
    
    protected override void SetSubstructurePositions()
    {
        base.SetSubstructurePositions();
        
        StructureBoundingBoxes =
        [
            new BoundingBox(X, Y, X + 16 - 1, Y + 7 - 1),
            new BoundingBox(X, Y + 8, X + StructureXSize - 1, Y + StructureYSize - 1)
        ];
    }
}

public class MainBasement_Room7 : CustomChainStructure
{
    public MainBasement_Room7(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/mainBasement_Room7",
            27,
            12,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 11, Directions.Left, new Seal.MainBasement_SealWall(), true),
                ],
        
                // right
                [
                    new ChainConnectPoint(26, 11, Directions.Right, new Seal.MainBasement_SealWall(), false),
                ]
            ],
            x, y, status, cost, weight)
    {}
}