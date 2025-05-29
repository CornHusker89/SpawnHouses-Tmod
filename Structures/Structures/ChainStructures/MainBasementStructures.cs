using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.Structures.Chains;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using BoundingBox = SpawnHouses.Types.BoundingBox;

namespace SpawnHouses.Structures.Structures.ChainStructures;

// ReSharper disable ConvertToPrimaryConstructor
public class MainBasementEntry1 : CustomChainStructure {
    public MainBasementEntry1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Entry1",
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
                    new ChainConnectPoint(0, 15, Directions.Left, new Seal.MainBasement_SealWall())
                ],

                // right
                [
                    new ChainConnectPoint(9, 15, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementEntry2 : CustomChainStructure {
    public MainBasementEntry2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Entry2",
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
                    new ChainConnectPoint(0, 14, Directions.Left, new Seal.MainBasement_SealWall())
                ],

                // right
                [
                    new ChainConnectPoint(14, 14, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }

    protected override void SetSubstructurePositions() {
        base.SetSubstructurePositions();

        StructureBoundingBoxes = [
            new BoundingBox(X + 1, Y, X + 5, Y + 4),
            new BoundingBox(X, Y + 5, X + StructureXSize - 1, Y + StructureYSize - 1)
        ];
    }
}

public class MainBasementHallway4 : CustomChainStructure {
    public MainBasementHallway4(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Hallway4",
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
            x, y, status, cost, weight) {
    }
}

public class MainBasementHallway5 : CustomChainStructure {
    public MainBasementHallway5(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Hallway5",
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
            x, y, status, cost, weight) {
    }

    protected override void SetSubstructurePositions() {
        base.SetSubstructurePositions();

        StructureBoundingBoxes = [
            new BoundingBox(X, Y, X + StructureXSize - 1, Y + 7 - 1),
            new BoundingBox(X + 1, Y + 7, X - 1 + StructureXSize - 1, Y + StructureYSize - 1)
        ];
    }
}

public class MainBasementHallway9 : CustomChainStructure {
    public MainBasementHallway9(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Hallway9",
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
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom1 : CustomChainStructure {
    public MainBasementRoom1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room1",
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
                    new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom1WithFloor : CustomChainStructure {
    public MainBasementRoom1WithFloor(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room1_WithFloor",
            22,
            9,
            [
                // top
                [],

                // bottom
                [
                    new ChainConnectPoint(10, 8, Directions.Down, new Seal.MainBasement_SealFloor())
                ],

                // left
                [
                    new ChainConnectPoint(0, 8, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom2 : CustomChainStructure {
    public MainBasementRoom2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room2",
            23,
            7,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(22, 6, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom2WithRoof : CustomChainStructure {
    public MainBasementRoom2WithRoof(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room2_WithRoof",
            23,
            7,
            [
                // top
                [
                    new ChainConnectPoint(3, 0, Directions.Up, new Seal.MainBasement_SealRoof())
                ],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(22, 6, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom3 : CustomChainStructure {
    public MainBasementRoom3(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room3",
            10,
            7,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(9, 6, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom4 : CustomChainStructure {
    public MainBasementRoom4(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room4",
            13,
            11,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 10, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(12, 10, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class MainBasementRoom5 : CustomChainStructure {
    public MainBasementRoom5(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base(
            CompatabilityHelper.IsMSEnabled
                ? "Assets/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage"
                : "Assets/StructureFiles/mainBasement/mainBasement_Room5",
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
                    new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }

    public override void OnFound() {
        if (CompatabilityHelper.IsMSEnabled && FilePath == "Assets/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage") {
            CompatabilityHelper.PlaceMSModule(X + 10, Y + 6, CompatabilityHelper.RemoteAccessTileID, CompatabilityHelper.RemoteAccessTileEntityID);
            if (StructureManager.MainHouse is not null && StructureManager.MainHouse.Status != StructureStatus.NotGenerated) {
                CompatabilityHelper.LinkRemoteStorage(new Point16(X + 10, Y + 6), StructureManager.MainHouse.StorageHeartPos);
            }

            NetHelper.SendUpdateMagicStorage(X + 11, Y + 7);
            CompatabilityHelper.UpdateStorageNetwork(X + 11, Y + 7);

            CompatabilityHelper.PlaceMSModule(X + 8, Y + 3, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(X + 12, Y + 3, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(X + 14, Y + 3, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(X + 6, Y + 6, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(X + 8, Y + 6, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);
            CompatabilityHelper.PlaceMSModule(X + 12, Y + 6, CompatabilityHelper.StorageUnitTileID, CompatabilityHelper.StorageUnitTileEntityID);

            StructureGenHelper.GenerateCobwebs(new Point(X, Y), StructureXSize, StructureYSize);
            FrameTiles();
        }
    }
}

public class MainBasementRoom6 : CustomChainStructure {
    public MainBasementRoom6(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room6",
            28,
            15,
            [
                // top
                [],

                // bottom
                [
                    new ChainConnectPoint(15, 14, Directions.Down, new Seal.MainBasement_SealFloor())
                ],

                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(15, 6, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }

    protected override void SetSubstructurePositions() {
        base.SetSubstructurePositions();

        StructureBoundingBoxes = [
            new BoundingBox(X, Y, X + 16 - 1, Y + 7 - 1),
            new BoundingBox(X, Y + 8, X + StructureXSize - 1, Y + StructureYSize - 1)
        ];
    }
}

public class MainBasementRoom7 : CustomChainStructure {
    public MainBasementRoom7(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/mainBasement_Room7",
            27,
            12,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 11, Directions.Left, new Seal.MainBasement_SealWall(), true)
                ],

                // right
                [
                    new ChainConnectPoint(26, 11, Directions.Right, new Seal.MainBasement_SealWall())
                ]
            ],
            x, y, status, cost, weight) {
    }
}