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
    public static readonly string _filePath_left = "Structures/StructureFiles/mainHouse/mainHouse_Left_v4"; // 1
    private const ushort length_left = 33;
    public static readonly string _filePath_small_left = "Structures/StructureFiles/mainHouse/mainHouse_Small_Left_v4"; // 2
    private const ushort length_small_left = 20;
    public static readonly string _filePath_small_basement_left = "Structures/StructureFiles/mainHouse/mainHouse_Small_Basement_Left_v4"; // 3
    private const ushort length_small_basement_left = 20;
    
    public static readonly string _filePath_right = "Structures/StructureFiles/mainHouse/mainHouse_Right_v4"; // 1
    private const ushort length_right = 30;
    public static readonly string _filePath_basement_right = "Structures/StructureFiles/mainHouse/mainHouse_Basement_Right_v4"; // 2
    private const ushort length_basement_right = 30;
    public static readonly string _filePath_small_right = "Structures/StructureFiles/mainHouse/mainHouse_Small_Right_v4"; // 3
    private const ushort length_small_right = 21;
    
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
    public readonly string LeftFilePath;
    public readonly string RightFilePath;
    public readonly string TopFilePath;
    public readonly ushort LeftSize;
    public readonly ushort RightSize;
    
    public readonly byte LeftType;
    public readonly byte RightType;

    private readonly bool LeftSmall = false;
    private readonly bool RightSmall = false;
    private readonly bool generatedBasement = false;
    
    public MainHouseStructure(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, 
        bool hasBasement = false, bool inUnderworld = false, byte leftType = 0, byte rightType = 0)
    {
        InUnderworld = inUnderworld;
        HasBasement = hasBasement;
        
        Floors = _floors;
        ConnectPoints = _connectPoints;

        // calculate what sides should be small (if we need to)
        if (leftType == 0 && rightType == 0)
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
        if (leftType == 0)
        {
            if (LeftSmall)
            {
                if (hasBasement && (RightSmall || Terraria.WorldGen.genRand.NextBool() ))
                {
                    LeftFilePath = _filePath_small_basement_left;
                    LeftSize = length_small_basement_left;
                    LeftType = 3;
                    LeftSmall = true;
                    generatedBasement = true;
                }
                else
                {
                    LeftFilePath = _filePath_small_left;
                    LeftSize = length_small_left;
                    LeftType = 2;
                    LeftSmall = true;
                }
            }
            else
            {
                LeftFilePath = _filePath_left;
                LeftSize = length_left;
                LeftType = 1;
            }
        }
        else
        {
            LeftType = leftType;
            switch (leftType)
            {
                case 1: 
                    LeftFilePath = _filePath_left; 
                    LeftSize = length_left;
                    break;
                case 2: 
                    LeftFilePath = _filePath_small_left;
                    LeftSize = length_small_left;
                    LeftSmall = true;
                    break;
                case 3: 
                    LeftFilePath = _filePath_small_basement_left;
                    LeftSize = length_small_basement_left;
                    LeftSmall = true;
                    break;
            }
        }


        if (rightType == 0)
        {
            if (RightSmall)
            {
                RightFilePath = _filePath_small_right;
                RightSize = length_small_right;
                RightType = 3;
                RightSmall = true;
            }
            else
            {
                if (hasBasement && !generatedBasement)
                {
                    RightFilePath = _filePath_basement_right;
                    RightSize = length_basement_right;
                    RightType = 2;
                    generatedBasement = true;
                }
                else
                {
                    RightFilePath = _filePath_right;
                    RightSize = length_right;
                    RightType = 1;
                }
            }
        }
        else
        {
            RightType = rightType;
            switch (rightType)
            {
                case 1: 
                    RightFilePath = _filePath_right;
                    RightSize = length_right;
                    break;
                case 2: 
                    RightFilePath = _filePath_basement_right;
                    RightSize = length_basement_right;
                    break;
                case 3: 
                    RightFilePath = _filePath_small_right;
                    RightSize = length_small_right;
                    RightSmall = true;
                    break;
            }
        }

        TopFilePath = _filePath_top;
        
        FilePath = LeftFilePath;

        ConnectPoints[3][0].XOffset = (short)(LeftSize + RightSize - 1);
        Floors[0].FloorLength = (ushort)(LeftSize + RightSize);

        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
        X = x;
        Y = y;
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
        
        int signIndex = Sign.ReadSign(X + 7, Y + 21);
        if (signIndex != -1)
            Sign.TextSign(signIndex, "All good adventures start in a tavern...To bad this isn't a tavern :(");
        
        Terraria.WorldGen.PlaceTile(X + LeftSize - 1, Y + 14, TileID.WorkBenches, true, true, style: 0);
        
        Status = StructureStatus.GeneratedAndFound;
    }
}   