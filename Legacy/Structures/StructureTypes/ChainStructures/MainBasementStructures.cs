using SpawnHouses.Common.DataStructures;
using SpawnHouses.Helpers;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Legacy.Structures.StructureTypes.ChainStructures;

// ReSharper disable ConvertToPrimaryConstructor
public class MainBasementEntry1 : LegacyChainStructure {
    public MainBasementEntry1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Entry1.shstruct",
            10,
            16,
            [
                // top
                [
                    new ChainConnectPoint(4, 0, LegacyDirections.Up, null, true)
                ],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 15, LegacyDirections.Left, new Seal.MainBasement_SealWall())
                ],

                // right
                [
                    new ChainConnectPoint(9, 15, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementEntry2 : LegacyChainStructure {
    public MainBasementEntry2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Entry2.shstruct",
            15,
            15,
            [
                // top
                [
                    new ChainConnectPoint(3, 0, LegacyDirections.Up, null, true)
                ],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 14, LegacyDirections.Left, new Seal.MainBasement_SealWall())
                ],

                // right
                [
                    new ChainConnectPoint(14, 14, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }

    protected override void SetSubstructurePositions() {
        base.SetSubstructurePositions();

        DetailedBoundingBoxes = [
            new TileBox(BoundingBox.Left + 1, BoundingBox.Top, BoundingBox.Left + 5, BoundingBox.Top + 4),
            new TileBox(BoundingBox.Left, BoundingBox.Top + 5, BoundingBox.Left + BoundingBox.Width - 1, BoundingBox.Top + BoundingBox.Height - 1)
        ];
    }
}

public class MainBasementHallway4 : LegacyChainStructure {
    public MainBasementHallway4(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Hallway4.shstruct",
            6,
            11,
            [
                // top
                [
                    new ChainConnectPoint(2, 0, LegacyDirections.Down, new Seal.MainBasement_SealFloor(), true, GenerateChances.Guaranteed)
                ],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 10, LegacyDirections.Left, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ],

                // right
                [
                    new ChainConnectPoint(5, 10, LegacyDirections.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementHallway5 : LegacyChainStructure {
    public MainBasementHallway5(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Hallway5.shstruct",
            8,
            22,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true, GenerateChances.Guaranteed),
                    new ChainConnectPoint(1, 21, LegacyDirections.Left, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ],

                // right
                [
                    new ChainConnectPoint(7, 6, LegacyDirections.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed),
                    new ChainConnectPoint(6, 21, LegacyDirections.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ]
            ],
            x, y, status, cost, weight) {
    }

    protected override void SetSubstructurePositions() {
        base.SetSubstructurePositions();

        DetailedBoundingBoxes = [
            new TileBox(BoundingBox.Left, BoundingBox.Top, BoundingBox.Left + BoundingBox.Width - 1, BoundingBox.Top + 7 - 1),
            new TileBox(BoundingBox.Left + 1, BoundingBox.Top + 7, BoundingBox.Left - 1 + BoundingBox.Width - 1, BoundingBox.Top + BoundingBox.Height - 1)
        ];
    }
}

public class MainBasementHallway9 : LegacyChainStructure {
    public MainBasementHallway9(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Hallway9.shstruct",
            6,
            11,
            [
                // top
                [],

                // bottom
                [
                    new ChainConnectPoint(2, 10, LegacyDirections.Up, new Seal.MainBasement_SealFloor(), false, GenerateChances.Guaranteed)
                ],

                // left
                [
                    new ChainConnectPoint(0, 5, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true, GenerateChances.Guaranteed)
                ],

                // right
                [
                    new ChainConnectPoint(5, 5, LegacyDirections.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom1 : LegacyChainStructure {
    public MainBasementRoom1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room1.shstruct",
            22,
            9,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 8, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(21, 8, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom1WithFloor : LegacyChainStructure {
    public MainBasementRoom1WithFloor(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room1_WithFloor.shstruct",
            22,
            9,
            [
                // top
                [],

                // bottom
                [
                    new ChainConnectPoint(10, 8, LegacyDirections.Down, new Seal.MainBasement_SealFloor())
                ],

                // left
                [
                    new ChainConnectPoint(0, 8, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(21, 8, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom2 : LegacyChainStructure {
    public MainBasementRoom2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room2.shstruct",
            23,
            7,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(22, 6, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom2WithRoof : LegacyChainStructure {
    public MainBasementRoom2WithRoof(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room2_WithRoof.shstruct",
            23,
            7,
            [
                // top
                [
                    new ChainConnectPoint(3, 0, LegacyDirections.Up, new Seal.MainBasement_SealRoof())
                ],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(22, 6, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom3 : LegacyChainStructure {
    public MainBasementRoom3(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room3.shstruct",
            10,
            7,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(9, 6, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom4 : LegacyChainStructure {
    public MainBasementRoom4(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room4.shstruct",
            13,
            11,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 10, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(12, 10, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom5 : LegacyChainStructure {
    public MainBasementRoom5(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base(
            CompatabilityHelper.IsMSEnabled
                ? "Assets/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage.shstruct"
                : "Assets/StructureFiles/mainBasement/mainBasement_Room5.shstruct",
            22,
            9,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 8, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(21, 8, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }

    public override void OnFound() {
        MainHouse mainHouse = StructureManager.LegacyStructures.Find(s => s is MainHouse) as MainHouse;
        if (CompatabilityHelper.IsMSEnabled && FilePath == "Assets/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage.shstruct") {
            CompatabilityHelper.PlaceMSModule(BoundingBox.Left + 10, BoundingBox.Top + 6, CompatabilityHelper.RemoteAccessTileID, CompatabilityHelper.RemoteAccessTileEntityID);
            if (mainHouse is not null && mainHouse.Status != StructureStatus.NotGenerated) CompatabilityHelper.LinkRemoteStorage(new Point16(BoundingBox.Left + 10, BoundingBox.Top + 6), mainHouse.StorageHeartPos);

            NetHelper.SendUpdateMagicStorage(BoundingBox.Left + 11, BoundingBox.Top + 7);
            CompatabilityHelper.UpdateStorageNetwork(BoundingBox.Left + 11, BoundingBox.Top + 7);

            CompatabilityHelper.PlaceMSModule(BoundingBox.Left + 8, BoundingBox.Top + 3, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(BoundingBox.Left + 12, BoundingBox.Top + 3, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(BoundingBox.Left + 14, BoundingBox.Top + 3, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(BoundingBox.Left + 6, BoundingBox.Top + 6, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(BoundingBox.Left + 8, BoundingBox.Top + 6, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(BoundingBox.Left + 12, BoundingBox.Top + 6, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);

            StructureGenHelper.GenerateCobwebs(new Point16(BoundingBox.Left, BoundingBox.Top), (ushort)BoundingBox.Width, (ushort)BoundingBox.Height);
            NetMessage.SendTileSquare(-1, BoundingBox.Left, BoundingBox.Top, BoundingBox.Width, BoundingBox.Height);
            FrameTiles();
        }
    }
}

public class MainBasementRoom6 : LegacyChainStructure {
    public MainBasementRoom6(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room6.shstruct",
            28,
            15,
            [
                // top
                [],

                // bottom
                [
                    new ChainConnectPoint(15, 14, LegacyDirections.Down, new Seal.MainBasement_SealFloor())
                ],

                // left
                [
                    new ChainConnectPoint(0, 6, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(15, 6, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }

    protected override void SetSubstructurePositions() {
        base.SetSubstructurePositions();

        DetailedBoundingBoxes = [
            new TileBox(BoundingBox.Left, BoundingBox.Top, BoundingBox.Left + 16 - 1, BoundingBox.Top + 7 - 1),
            new TileBox(BoundingBox.Left, BoundingBox.Top + 8, BoundingBox.Left + BoundingBox.Width - 1, BoundingBox.Top + BoundingBox.Height - 1)
        ];
    }
}

public class MainBasementRoom7 : LegacyChainStructure {
    public MainBasementRoom7(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room7.shstruct",
            27,
            12,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 11, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(26, 11, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom8 : LegacyChainStructure {
    public MainBasementRoom8(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room8.shstruct",
            23,
            9,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 8, LegacyDirections.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(22, 8, LegacyDirections.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }

    public override void OnFound() {
        Terraria.WorldGen.PlaceTile(BoundingBox.Left + 4, BoundingBox.Top + 7, TileID.Furnaces, true, true);
        StructureGenHelper.GenerateCobwebs(new Point16(BoundingBox.Left, BoundingBox.Top), (ushort)BoundingBox.Width, (ushort)BoundingBox.Height);
        NetMessage.SendTileSquare(-1, BoundingBox.Left, BoundingBox.Top, BoundingBox.Width, BoundingBox.Height);
    }
}