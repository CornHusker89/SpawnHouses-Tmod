using System.Collections.Generic;
using SpawnHouses.Core.Geometry;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using StructureHelper.API;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Legacy.Structures.StructureTypes;

public sealed class MainHouse : LegacyStructure {
    // constants
    // ReSharper disable InconsistentNaming
    private static readonly List<string> _signQuotes = [
        "All good adventures start in a tavern...Too bad this isn't a tavern :(",
        "Welcome to the conveniently placed house in the middle of nowhere!",
        "FINALLY, NO MORE BOX HOTELS!!!",
        "No, we don’t care if this has an impact on official lore.",
    ];

    private const byte _type_not_generated = 0;


    public static readonly string _filePath_left = "Assets/StructureFiles/mainHouse/mainHouse_Left_v4.shstruct";
    private const byte _type_left = 1;

    public static readonly string _filePath_small_left = "Assets/StructureFiles/mainHouse/mainHouse_Small_Left_v4.shstruct";
    private const byte _type_small_left = 2;

    public static readonly string _filePath_small_basement_left =
        "Assets/StructureFiles/mainHouse/mainHouse_Small_Basement_Left_v4.shstruct";

    private const byte _type_small_basement_left = 3;

    public static readonly string _filePath_magicstorage_left =
        "Assets/StructureFiles/mainHouse/mainHouse_MagicStorage_Left_v4.shstruct";

    private const byte _type_magicstorage_left = 4;

    public static readonly string _filePath_basement_left =
        "Assets/StructureFiles/mainHouse/mainHouse_Basement_Left_v4.shstruct";

    private const byte _type_basement_left = 5;


    public static readonly string _filePath_right = "Assets/StructureFiles/mainHouse/mainHouse_Right_v4.shstruct";
    private const byte _type_right = 1;

    public static readonly string _filePath_basement_right =
        "Assets/StructureFiles/mainHouse/mainHouse_Basement_Right_v4.shstruct";

    private const byte _type_basement_right = 2;

    public static readonly string _filePath_small_right =
        "Assets/StructureFiles/mainHouse/mainHouse_Small_Right_v4.shstruct";

    private const byte _type_small_right = 3;

    public static readonly string _filePath_small_magicstorage_right =
        "Assets/StructureFiles/mainHouse/mainHouse_Small_MagicStorage_Right_v4.shstruct";

    private const byte _type_small_magicstorage_right = 4;

    public static readonly string _filePath_magicstorage_right =
        "Assets/StructureFiles/mainHouse/mainHouse_MagicStorage_Right_v4.shstruct";

    private const byte _type_magicstorage_right = 5;


    public static readonly string _filePath_top = "Assets/StructureFiles/mainHouse/mainHouse_Top_v4.shstruct"; // 1

    public static readonly ushort _structureXSize = 63;
    public static readonly ushort _structureYSize = 36;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(0, 26, LegacyDirections.Left)
        ],

        // right
        [
            new ConnectPoint(62, 26, LegacyDirections.Right)
        ]
    ];

    public readonly bool InUnderworld;
    public readonly bool HasBasement;

    public readonly string LeftFilePath;
    public readonly string RightFilePath;
    public readonly string TopFilePath;
    public readonly ushort LeftSize;
    public readonly ushort RightSize;
    public readonly Point16 StorageHeartPos = new(1000, 1000);
    public readonly Point16 BasementEntryPos = new(1000, 1000);
    public readonly Point16 SignPos = new(1000, 1000);

    public readonly byte LeftType;
    public readonly byte RightType;

    private readonly bool LeftSmall;
    private readonly bool RightSmall;
    private readonly bool generatedBasement;

    public MainHouse(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, bool hasBasement = false, bool inUnderworld = false, byte leftType = _type_not_generated, byte rightType = _type_not_generated) :
        base("RootStructureTypes/", _structureXSize, _structureYSize, CopyConnectPoints(_connectPoints), status, x, y) {
        InUnderworld = inUnderworld;
        HasBasement = hasBasement;
        BoundingBox = new TileBox(x, y, _structureXSize, _structureYSize);

        // calculate what sides should be small (if we need to)
        if (leftType == _type_not_generated && rightType == _type_not_generated) {
            double size = SpawnHousesMod.Config.SpawnPointHouseSize;
            if (size is 3) {
                if (Terraria.WorldGen.genRand.NextBool())
                    LeftSmall = true;
                else
                    RightSmall = true;
            }
            else if (size is 2) {
                LeftSmall = true;
                RightSmall = true;
            }
        }


        // set left and right side generation varibles
        if (leftType != _type_not_generated) {
            LeftType = leftType;
        }
        else {
            if (LeftSmall) {
                if (hasBasement && (RightSmall || Terraria.WorldGen.genRand.NextBool())) {
                    LeftType = _type_small_basement_left;
                    generatedBasement = true;
                }
                // else if (hasBasement && CompatabilityHelper.IsMsEnabled && !RightSmall) {
                //     LeftType = _type_small_basement_left;
                //     generatedBasement = true;
                // }
                else {
                    LeftType = _type_small_left;
                }
            }
            // else if (CompatabilityHelper.IsMsEnabled && RightSmall) {
            //     LeftType = _type_basement_left;
            // }
            // else if (CompatabilityHelper.IsMsEnabled) {
            //     LeftType = _type_magicstorage_left;
            // }
            else if (hasBasement && (RightSmall || Terraria.WorldGen.genRand.NextBool())) {
                LeftType = _type_basement_left;
                generatedBasement = true;
            }
            else {
                LeftType = _type_left;
            }
        }

        switch (LeftType) {
            case _type_left:
                LeftFilePath = _filePath_left;
                LeftSize = 33;
                SignPos = new Point16(BoundingBox.Left + 7, BoundingBox.Top + 20);
                break;
            case _type_small_left:
                LeftFilePath = _filePath_small_left;
                LeftSize = 20;
                LeftSmall = true;
                SignPos = new Point16(BoundingBox.Left + 1, BoundingBox.Top + 20);
                break;
            case _type_small_basement_left:
                LeftFilePath = _filePath_small_basement_left;
                LeftSize = 20;
                LeftSmall = true;
                BasementEntryPos = new Point16(BoundingBox.Left + 10, BoundingBox.Top + 34);
                SignPos = new Point16(BoundingBox.Left + 1, BoundingBox.Top + 20);
                break;
            case _type_magicstorage_left:
                LeftFilePath = _filePath_magicstorage_left;
                LeftSize = 33;
                StorageHeartPos = new Point16(BoundingBox.Left + 25, BoundingBox.Top + 27);
                SignPos = new Point16(BoundingBox.Left + 7, BoundingBox.Top + 20);
                break;
            case _type_basement_left:
                LeftFilePath = _filePath_basement_left;
                LeftSize = 33;
                BasementEntryPos = new Point16(BoundingBox.Left + 22, BoundingBox.Top + 34);
                SignPos = new Point16(BoundingBox.Left + 7, BoundingBox.Top + 20);
                break;
        }


        if (rightType != _type_not_generated) {
            RightType = rightType;
        }
        else {
            if (RightSmall) {
                // if (CompatabilityHelper.IsMsEnabled && LeftType != _type_magicstorage_left)
                //     RightType = _type_small_magicstorage_right;
                // else
                //     RightType = _type_small_right;
            }

            else if (hasBasement && !generatedBasement) {
                RightType = _type_basement_right;
                generatedBasement = true;
            }
            // else if (CompatabilityHelper.IsMsEnabled && LeftSmall) {
            //     RightType = _type_magicstorage_right;
            // }
            else {
                RightType = _type_right;
            }
        }

        switch (RightType) {
            case _type_right:
                RightFilePath = _filePath_right;
                RightSize = 30;
                break;
            case _type_basement_right:
                RightFilePath = _filePath_basement_right;
                RightSize = 30;
                BasementEntryPos = new Point16(BoundingBox.Left + LeftSize + 9, BoundingBox.Top + 34);
                break;
            case _type_small_right:
                RightFilePath = _filePath_small_right;
                RightSize = 21;
                RightSmall = true;
                break;
            case _type_small_magicstorage_right:
                RightFilePath = _filePath_small_magicstorage_right;
                RightSize = 21;
                RightSmall = true;
                StorageHeartPos = new Point16(BoundingBox.Left + LeftSize + 4, BoundingBox.Top + 27);
                break;
            case _type_magicstorage_right:
                RightFilePath = _filePath_magicstorage_right;
                RightSize = 30;
                StorageHeartPos = new Point16(BoundingBox.Left + LeftSize + 4, BoundingBox.Top + 27);
                break;
        }


        TopFilePath = _filePath_top;

        FilePath = LeftFilePath;

        ConnectPoints[3][0].XOffset = (short)(LeftSize + RightSize - 1);
        BoundingBox = new TileBox(x, y, (ushort)(LeftSize + RightSize), _structureYSize);

        Status = status;
    }

    public override void OnFound() {
    }

    [NoJIT]
    public override void Generate(bool bare = false) {
        if (!bare) {
            StructureGenHelper.GenerateFoundation(new Point16(BoundingBox.Left + BoundingBox.Width / 2, BoundingBox.Top + 26), TileID.Dirt, BoundingBox.Width / 2 + 7, true);

            StructureGenHelper.Blend(ConnectPoints[2][0], 20, TileID.Grass, maxHeight: InUnderworld ? (ushort)10 : (ushort)38);
            StructureGenHelper.Blend(ConnectPoints[3][0], 20, TileID.Grass, maxHeight: InUnderworld ? (ushort)10 : (ushort)38, blendLeftSide: false);
        }

        Generator.GenerateStructure(LeftFilePath, new Point16(BoundingBox.Left, BoundingBox.Top + 10), SpawnHousesMod.Instance);
        Generator.GenerateStructure(RightFilePath, new Point16(BoundingBox.Left + LeftSize, BoundingBox.Top + 10), SpawnHousesMod.Instance);
        Generator.GenerateStructure(TopFilePath, new Point16(BoundingBox.Left + LeftSize - 14, BoundingBox.Top), SpawnHousesMod.Instance);

        string signString = _signQuotes[Terraria.WorldGen.genRand.Next(0, _signQuotes.Count)];
        int signIndex = Sign.ReadSign(SignPos.X, SignPos.Y);
        if (signIndex != -1)
            Sign.TextSign(signIndex, signString);

        Terraria.WorldGen.PlaceTile(BoundingBox.Left + LeftSize - 1, BoundingBox.Top + 24, TileID.WorkBenches, true, true, style: 0);
        Generator.GenerateStructure("Assets/StructureFiles/mainHouse/mainHouse_Rose.shstruct",
            new Point16(BoundingBox.Left + LeftSize - 1, BoundingBox.Top + 18), SpawnHousesMod.Instance);

        // bushes
        if (!InUnderworld && !bare) {
            ushort[] blacklistWallIDs =
                [WallID.StoneSlab, WallID.PearlstoneBrick, WallID.SnowBrick, WallID.RichMaogany];
            int leftBushCount = Terraria.WorldGen.genRand.Next(2, 5);
            for (int i = 0; i < leftBushCount; i++) {
                int xOffset = Terraria.WorldGen.genRand.Next(0, 12);
                StructureGenHelper.PlaceBush(new Point16(BoundingBox.Left + xOffset, BoundingBox.Top + 25 + Terraria.WorldGen.genRand.Next(0, 2)),
                    wallBlacklistIDs: blacklistWallIDs);
            }

            int rightBushCount = Terraria.WorldGen.genRand.Next(2, 5);
            for (int i = 0; i < rightBushCount; i++) {
                int xOffset = Terraria.WorldGen.genRand.Next(0, 12);
                StructureGenHelper.PlaceBush(
                    new Point16(BoundingBox.Left + BoundingBox.Width - 1 - xOffset, BoundingBox.Top + 25 + Terraria.WorldGen.genRand.Next(0, 2)),
                    wallBlacklistIDs: blacklistWallIDs);
            }
        }

        FrameTiles(BoundingBox.Left + LeftSize, BoundingBox.Top + 4, 40);
        Status = StructureStatus.GeneratedAndFound;
    }
}