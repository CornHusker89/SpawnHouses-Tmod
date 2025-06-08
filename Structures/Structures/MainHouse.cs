using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using StructureHelper.API;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Structures.Structures;

public sealed class MainHouse : CustomStructure {
    // constants
    // ReSharper disable InconsistentNaming
    private static readonly List<string> _signQuotes = [
        "All good adventures start in a tavern...Too bad this isn't a tavern :(",
        "Welcome to the conveniently placed house in the middle of nowhere!",
        "FINALLY, NO MORE BOX HOTELS!!!",
        "No, we don’t care if this has an impact on official lore.",
        "This house has been generated ~ times!"
    ];

    private const byte _type_not_generated = 0;


    public static readonly string _filePath_left = "Assets/StructureFiles/mainHouse/mainHouse_Left_v4";
    private const byte _type_left = 1;

    public static readonly string _filePath_small_left = "Assets/StructureFiles/mainHouse/mainHouse_Small_Left_v4";
    private const byte _type_small_left = 2;

    public static readonly string _filePath_small_basement_left =
        "Assets/StructureFiles/mainHouse/mainHouse_Small_Basement_Left_v4";

    private const byte _type_small_basement_left = 3;

    public static readonly string _filePath_magicstorage_left =
        "Assets/StructureFiles/mainHouse/mainHouse_MagicStorage_Left_v4";

    private const byte _type_magicstorage_left = 4;

    public static readonly string _filePath_basement_left =
        "Assets/StructureFiles/mainHouse/mainHouse_Basement_Left_v4";

    private const byte _type_basement_left = 5;


    public static readonly string _filePath_right = "Assets/StructureFiles/mainHouse/mainHouse_Right_v4";
    private const byte _type_right = 1;

    public static readonly string _filePath_basement_right =
        "Assets/StructureFiles/mainHouse/mainHouse_Basement_Right_v4";

    private const byte _type_basement_right = 2;

    public static readonly string _filePath_small_right =
        "Assets/StructureFiles/mainHouse/mainHouse_Small_Right_v4";

    private const byte _type_small_right = 3;

    public static readonly string _filePath_small_magicstorage_right =
        "Assets/StructureFiles/mainHouse/mainHouse_Small_MagicStorage_Right_v4";

    private const byte _type_small_magicstorage_right = 4;

    public static readonly string _filePath_magicstorage_right =
        "Assets/StructureFiles/mainHouse/mainHouse_MagicStorage_Right_v4";

    private const byte _type_magicstorage_right = 5;


    public static readonly string _filePath_top = "Assets/StructureFiles/mainHouse/mainHouse_Top_v4"; // 1

    public static readonly ushort _structureXSize = 63;
    public static readonly ushort _structureYSize = 36;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(0, 26, Directions.Left)
        ],

        // right
        [
            new ConnectPoint(62, 26, Directions.Right)
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
        base("Structures/", _structureXSize, _structureYSize, CopyConnectPoints(_connectPoints), status, x, y) {
        InUnderworld = inUnderworld;
        HasBasement = hasBasement;

        // calculate what sides should be small (if we need to)
        if (leftType == _type_not_generated && rightType == _type_not_generated) {
            double size = ModContent.GetInstance<SpawnHousesConfig>().SpawnPointHouseSize;
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
                else if (hasBasement && CompatabilityHelper.IsMSEnabled && !RightSmall) {
                    LeftType = _type_small_basement_left;
                    generatedBasement = true;
                }
                else {
                    LeftType = _type_small_left;
                }
            }
            else if (CompatabilityHelper.IsMSEnabled && RightSmall) {
                LeftType = _type_basement_left;
            }
            else if (CompatabilityHelper.IsMSEnabled) {
                LeftType = _type_magicstorage_left;
            }
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
                SignPos = new Point16(X + 7, Y + 20);
                break;
            case _type_small_left:
                LeftFilePath = _filePath_small_left;
                LeftSize = 20;
                LeftSmall = true;
                SignPos = new Point16(X + 1, Y + 20);
                break;
            case _type_small_basement_left:
                LeftFilePath = _filePath_small_basement_left;
                LeftSize = 20;
                LeftSmall = true;
                BasementEntryPos = new Point16(X + 10, Y + 34);
                SignPos = new Point16(X + 1, Y + 20);
                break;
            case _type_magicstorage_left:
                LeftFilePath = _filePath_magicstorage_left;
                LeftSize = 33;
                StorageHeartPos = new Point16(X + 25, Y + 27);
                SignPos = new Point16(X + 7, Y + 20);
                break;
            case _type_basement_left:
                LeftFilePath = _filePath_basement_left;
                LeftSize = 33;
                BasementEntryPos = new Point16(X + 22, Y + 34);
                SignPos = new Point16(X + 7, Y + 20);
                break;
        }


        if (rightType != _type_not_generated) {
            RightType = rightType;
        }
        else {
            if (RightSmall) {
                if (CompatabilityHelper.IsMSEnabled && LeftType != _type_magicstorage_left)
                    RightType = _type_small_magicstorage_right;
                else
                    RightType = _type_small_right;
            }

            else if (hasBasement && !generatedBasement) {
                RightType = _type_basement_right;
                generatedBasement = true;
            }
            else if (CompatabilityHelper.IsMSEnabled && LeftSmall) {
                RightType = _type_magicstorage_right;
            }
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
                BasementEntryPos = new Point16(X + LeftSize + 9, Y + 34);
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
                StorageHeartPos = new Point16(X + LeftSize + 4, Y + 27);
                break;
            case _type_magicstorage_right:
                RightFilePath = _filePath_magicstorage_right;
                RightSize = 30;
                StorageHeartPos = new Point16(X + LeftSize + 4, Y + 27);
                break;
        }


        TopFilePath = _filePath_top;

        FilePath = LeftFilePath;

        ConnectPoints[3][0].XOffset = (short)(LeftSize + RightSize - 1);

        StructureXSize = (ushort)(LeftSize + RightSize);
        StructureYSize = _structureYSize;

        Status = status;
    }

    public override void OnFound() {
    }

    [NoJIT]
    public override void Generate(bool bare = false) {
        if (!bare) {
            StructureGenHelper.GenerateFoundation(new Point(X + StructureXSize / 2, Y + 26), TileID.Dirt, StructureXSize / 2 + 7, true);

            StructureGenHelper.Blend(ConnectPoints[2][0], 20, TileID.Grass, maxHeight: InUnderworld ? (ushort)10 : (ushort)38);
            StructureGenHelper.Blend(ConnectPoints[3][0], 20, TileID.Grass, maxHeight: InUnderworld ? (ushort)10 : (ushort)38, blendLeftSide: false);
        }

        Generator.GenerateStructure(LeftFilePath, new Point16(X, Y + 10), SpawnHousesMod.Instance);
        Generator.GenerateStructure(RightFilePath, new Point16(X + LeftSize, Y + 10), SpawnHousesMod.Instance);
        Generator.GenerateStructure(TopFilePath, new Point16(X + LeftSize - 14, Y), SpawnHousesMod.Instance);

        string signString = "All good adventures start in a tavern...To bad this isn't a tavern :(";
        Random rnd = new();
        for (int i = 0; i < 25; i++) {
            string possibleString = _signQuotes[rnd.Next(0, _signQuotes.Count)];
            if (possibleString.Contains('~'))
                try {
                    var dict = SpawnHousesMod.WebClient.GetSpawnCount();
                    if (dict is not null) {
                        dict.TryGetValue("main_houses_extrapolated", out int value);
                        if (value is not -1) {
                            signString = possibleString.Replace("~", value.ToString());
                            break;
                        }
                    }

                    continue;
                }
                catch {
                    continue;
                }

            signString = possibleString;
            break;
        }

        int signIndex = Sign.ReadSign(SignPos.X, SignPos.Y);
        if (signIndex != -1)
            Sign.TextSign(signIndex, signString);

        Terraria.WorldGen.PlaceTile(X + LeftSize - 1, Y + 24, TileID.WorkBenches, true, true, style: 0);
        Generator.GenerateStructure("Assets/StructureFiles/mainHouse/mainHouse_Rose",
            new Point16(X + LeftSize - 1, Y + 18), SpawnHousesMod.Instance);

        // bushes
        if (!InUnderworld && !bare) {
            ushort[] blacklistWallIDs =
                [WallID.StoneSlab, WallID.PearlstoneBrick, WallID.SnowBrick, WallID.RichMaogany];
            int leftBushCount = Terraria.WorldGen.genRand.Next(2, 5);
            for (int i = 0; i < leftBushCount; i++) {
                int xOffset = Terraria.WorldGen.genRand.Next(0, 12);
                StructureGenHelper.PlaceBush(new Point(X + xOffset, Y + 25 + Terraria.WorldGen.genRand.Next(0, 2)),
                    wallBlacklistIDs: blacklistWallIDs);
            }

            int rightBushCount = Terraria.WorldGen.genRand.Next(2, 5);
            for (int i = 0; i < rightBushCount; i++) {
                int xOffset = Terraria.WorldGen.genRand.Next(0, 12);
                StructureGenHelper.PlaceBush(
                    new Point(X + StructureXSize - 1 - xOffset, Y + 25 + Terraria.WorldGen.genRand.Next(0, 2)),
                    wallBlacklistIDs: blacklistWallIDs);
            }
        }

        FrameTiles(X + LeftSize, Y + 4, 40);
        Status = StructureStatus.GeneratedAndFound;
    }
}