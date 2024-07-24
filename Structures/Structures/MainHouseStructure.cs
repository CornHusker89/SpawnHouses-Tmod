using Terraria;
using Terraria.ID;
using System;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpawnHouses.Structures.Structures;

public class MainHouseStructure : CustomStructure
{
    // constants
    private const byte _type_not_generated = 0;
    
    public static readonly string _filePath_left = "Structures/StructureFiles/mainHouse/mainHouse_Left_v4";
    private const byte _type_left = 1;
    
    public static readonly string _filePath_small_left = "Structures/StructureFiles/mainHouse/mainHouse_Small_Left_v4";
    private const byte _type_small_left = 2;
    
    public static readonly string _filePath_small_basement_left = "Structures/StructureFiles/mainHouse/mainHouse_Small_Basement_Left_v4";
    private const byte _type_small_basement_left = 3;
    
    public static readonly string _filePath_magicstorage_left = "Structures/StructureFiles/mainHouse/mainHouse_MagicStorage_Left_v4";
    private const byte _type_magicstorage_left = 4;
    
    public static readonly string _filePath_basement_left = "Structures/StructureFiles/mainHouse/mainHouse_Basement_Left_v4";
    private const byte _type_basement_left = 5;
    
    
    
    public static readonly string _filePath_right = "Structures/StructureFiles/mainHouse/mainHouse_Right_v4";
    private const byte _type_right = 1;
    
    public static readonly string _filePath_basement_right = "Structures/StructureFiles/mainHouse/mainHouse_Basement_Right_v4";
    private const byte _type_basement_right = 2;
    
    public static readonly string _filePath_small_right = "Structures/StructureFiles/mainHouse/mainHouse_Small_Right_v4";
    private const byte _type_small_right = 3;
    
    public static readonly string _filePath_small_magicstorage_right = "Structures/StructureFiles/mainHouse/mainHouse_Small_MagicStorage_Right_v4";
    private const byte _type_small_magicstorage_right = 4;
    
    public static readonly string _filePath_magicstorage_right = "Structures/StructureFiles/mainHouse/mainHouse_MagicStorage_Right_v4";
    private const byte _type_magicstorage_right = 5;
    
    
    
    public static readonly string _filePath_top = "Structures/StructureFiles/mainHouse/mainHouse_Top_v4"; // 1
    
    
    public static readonly ushort _structureXSize = 63;
    public static readonly ushort _structureYSize = 36;
    
    
    public static readonly Floor[] _floors = 
    [
        new Floor(0, 16, 42)
    ];

    public static readonly ConnectPoint[][] _connectPoints =
    [
        // top
        [], 
        
        // bottom
        [],
        
        // left
        [
            new ConnectPoint(0, 16, Directions.Left)
        ],
        
        // right
        [
            new ConnectPoint(62, 16, Directions.Right)
        ]
    ];
    
    public readonly bool InUnderworld;
    public readonly bool HasBasement;
    public readonly bool HasMagicStorage;
    
    public readonly string LeftFilePath;
    public readonly string RightFilePath;
    public readonly string TopFilePath;
    public readonly ushort LeftSize;
    public readonly ushort RightSize;
    public readonly Point16 StorageHeartPos = new Point16(1000, 1000);
    public readonly Point16 BasementEntryPos = new Point16(1000, 1000);
    public readonly Point16 SignPos = new Point16(1000, 1000);
    
    public readonly byte LeftType;
    public readonly byte RightType;

    private readonly bool LeftSmall = false;
    private readonly bool RightSmall = false;
    private readonly bool generatedBasement = false;
    
    public MainHouseStructure(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, 
        bool hasBasement = false, bool inUnderworld = false, byte leftType = _type_not_generated, byte rightType = _type_not_generated)
    {
        X = x;
        Y = y;
        
        InUnderworld = inUnderworld;
        HasBasement = hasBasement;
        
        Floors = _floors;
        ConnectPoints = _connectPoints;

        // calculate what sides should be small (if we need to)
        if (leftType == _type_not_generated && rightType == _type_not_generated)
        {
            double size = ModContent.GetInstance<SpawnHousesConfig>().SizeMultiplier;
            if (size < 0.7)
            {
                LeftSmall = true;
                RightSmall = true;
            }
            else if (size < 0.85)
            {
                if (Terraria.WorldGen.genRand.NextBool())
                    LeftSmall = true;
                else
                    RightSmall = true;
            }
        }
        
        
        // set left and right side generation varibles
        if (leftType != _type_not_generated)
            LeftType = leftType;
        else
        {
            if (LeftSmall)
                if (hasBasement && (RightSmall || Terraria.WorldGen.genRand.NextBool()))
                {
                    LeftType = _type_small_basement_left;
                    generatedBasement = true;
                }
                else if (hasBasement && SpawnHousesModHelper.IsMSEnabled && !RightSmall)
                {
                    LeftType = _type_small_basement_left;
                    generatedBasement = true;
                }
                else
                    LeftType = _type_small_left;

            else if (SpawnHousesModHelper.IsMSEnabled && RightSmall)
                LeftType = _type_basement_left;
            else if (SpawnHousesModHelper.IsMSEnabled)
                LeftType = _type_magicstorage_left;
            else if (hasBasement && (RightSmall || Terraria.WorldGen.genRand.NextBool()))
                LeftType = _type_basement_left;
            else
                LeftType = _type_left;
        }

        switch (LeftType)
        {
            case _type_left: 
                LeftFilePath = _filePath_left; 
                LeftSize = 33;
                SignPos = new Point16(X + 7, Y + 10);
                break;
            case _type_small_left: 
                LeftFilePath = _filePath_small_left;
                LeftSize = 20;
                LeftSmall = true;
                SignPos = new Point16(X + 1, Y + 10);
                break;
            case _type_small_basement_left: 
                LeftFilePath = _filePath_small_basement_left;
                LeftSize = 20;
                LeftSmall = true;
                BasementEntryPos = new Point16(X + 10, Y + 24);
                SignPos = new Point16(X + 1, Y + 10);
                break;
            case _type_magicstorage_left:
                LeftFilePath = _filePath_magicstorage_left;
                LeftSize = 33;
                HasMagicStorage = true;
                StorageHeartPos = new Point16(X + LeftSize + 25, Y + 17);
                SignPos = new Point16(X + 7, Y + 10);
                break;
            case _type_basement_left:
                LeftFilePath = _filePath_basement_left;
                LeftSize = 33;
                BasementEntryPos = new Point16(X + 22, Y + 24);
                SignPos = new Point16(X + 7, Y + 10);
                break;
        }

        
        if (rightType != _type_not_generated)
            RightType = rightType;
        else
        {
            if (RightSmall)
                if (SpawnHousesModHelper.IsMSEnabled && LeftType != _type_magicstorage_left)
                    RightType = _type_small_magicstorage_right;
                else
                    RightType = _type_small_right;
            
            else
                if (hasBasement && !generatedBasement)
                {
                    RightType = _type_basement_right;
                    generatedBasement = true;
                }
                else if (SpawnHousesModHelper.IsMSEnabled && LeftSmall)
                    RightType = _type_magicstorage_right;
                else
                    RightType = _type_right;
        }
        
        switch (RightType)
        {
            case _type_right: 
                RightFilePath = _filePath_right;
                RightSize = 30;
                break;
            case _type_basement_right: 
                RightFilePath = _filePath_basement_right;
                RightSize = 30;
                BasementEntryPos = new Point16(X + LeftSize + 9, Y + 24);
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
                HasMagicStorage = true;
                StorageHeartPos = new Point16(X + LeftSize + 4, Y + 17);
                break; 
            case _type_magicstorage_right:
                RightFilePath = _filePath_magicstorage_right;
                RightSize = 30;
                HasMagicStorage = true;
                StorageHeartPos = new Point16(X + LeftSize + 4, Y + 17);
                break; 
        }
        

        TopFilePath = _filePath_top;
        
        FilePath = LeftFilePath;

        ConnectPoints[3][0].XOffset = (short)(LeftSize + RightSize - 1);
        Floors[0].FloorLength = (ushort)(LeftSize + RightSize);

        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
        Status = status;
        SetSubstructurePositions();
    }

    public override void OnFound() {}

    [NoJIT]
    public override void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, foundationYOffset: 13);

        if (!InUnderworld)
        {
            ConnectPoints[2][0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);
            ConnectPoints[3][0].BlendRight(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);
        }
        else
        {
            ConnectPoints[2][0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25, maxHeight: 10);
            ConnectPoints[3][0].BlendRight(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25, maxHeight: 10);
        }
        
        _GenerateStructure();
        StructureHelper.Generator.GenerateStructure(RightFilePath, new Point16(X + LeftSize, Y), _mod);
        StructureHelper.Generator.GenerateStructure(TopFilePath, new Point16(X + LeftSize - 14, Y - 10), _mod);
        FrameTiles(X + LeftSize, Y + 4, 40);
        
        int signIndex = Sign.ReadSign(this.SignPos.X, this.SignPos.Y);
        if (signIndex != -1)
            Sign.TextSign(signIndex, "All good adventures start in a tavern...To bad this isn't a tavern :(");
        
        Terraria.WorldGen.PlaceTile(X + LeftSize - 1, Y + 14, TileID.WorkBenches, true, true, style: 0);
        
        Status = StructureStatus.GeneratedAndFound;
    }
}   