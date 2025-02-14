using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Structures.AdvStructures;


public enum StructureTag
{
    // ===== themes =====
    /// structure is categorized as being above ground (typically has a roof)
    AboveGround, 
    /// structure is categorized as being below ground (typically has a no roof)
    UnderGround, 
    /// structure is categorized as having an overall forest theme 
    Forest,
    /// structure is categorized as having an overall beach theme
    Beach,
    /// structure is categorized as having an overall jungle theme 
    Jungle, 
    /// structure is categorized as having an overall cavern/underground theme 
    Cavern, 
    
    
    // ===== basic attributes =====
    HasHousing,
    IsSymmetric,
    
    
    // ===== floor =====
    HasFloor,
    
    
    // ===== wall =====
    HasWall,
    
    
    // ===== room =====
    HasWindow,
    
    
    // ===== roof =====
    HasRoof,
    RoofSlantNone,
    RoofSlantLeftHigh,
    RoofSlantRightHigh,
    RoofSlantCenterHigh,
    RoofTall,
    RoofShort,
    HasChimney,
    RoofSlope1To1,
    RoofSlopeLessThan1,
    RoofSlopeGreaterThan1,
    RoofSlopeNone
    
}

public enum DataType
{
    /// is a single integer, representing the amount of housing
    HousingCount
}

public enum PaletteTag
{
    Wood,
    Stone,
    DarkGrey,
    LightGrey,
    MediumGrey,
    DarkBrown,
    LightBrown,
    MediumBrown,
    Red,
    Turquoise,
}


public struct StructureParams
{
    public TilePalette Palette; 
    public StructureTag[] StructureTagsRequired;
    public StructureTag[] StructureTagBlacklist;
    public Point16 Start;
    public Point16 End;
    public int Length;
    public int MinVolume;
    public int MaxVolume;
    public int Volume;
    public int Height;
    public int MinHousing;
    public int MaxHousing;
    public int Housing;

    public StructureParams(
        TilePalette tilePalette,
        StructureTag[] structureTagsRequired,
        StructureTag[] structureTagBlacklist,
        Point16 start,
        Point16 end,
        Range volumeRange,
        Range housingRange)
    {
        Palette = tilePalette;
        StructureTagsRequired = structureTagsRequired;
        StructureTagBlacklist = structureTagBlacklist;
        Start = start;
        End = end;
        if (Start.X > End.X)
            (Start, End) = (End, Start);
        Length = End.X - Start.X;
        MinVolume = volumeRange.Min;
        MaxVolume = volumeRange.Max;
        MinHousing = housingRange.Min;
        MaxHousing = housingRange.Max;
        
        ReRollRanges();
    }
    
    public void ReRollRanges()
    {
        double scale = Terraria.WorldGen.genRand.NextDouble();
        Volume = (int)(MinVolume + (MaxVolume - MinVolume) * scale);
        Height = Volume / (End.X - Start.X);
        if (Height <= 4)
            throw new ArgumentException($"Volume ({Volume}) is too small compared to the length ({Length}) of the structure, resulting in a height of {Height}");

        if (MinHousing < 0)
            throw new ArgumentException("Min housing cannot be less than 0");
        if (MaxHousing < 0)
            throw new ArgumentException("Max housing cannot be less than 0");
        if (MaxHousing < MinHousing)
            throw new ArgumentException("Max Housing is less than min housing");
        if (MaxHousing == 0 && StructureTagBlacklist.Contains(StructureTag.HasHousing))
            throw new ArgumentException(
                "Adv structure cannot have a max housing of 0 while blacklisting components with housing");
        Housing = (int)(MinHousing + (MaxHousing - MinHousing) * scale);
    }
}


public struct ComponentParams(
    List<StructureTag> tagsRequired,
    List<StructureTag> tagsBlacklist,
    Shape mainVolume,
    TilePalette tilePalette)
{
    public readonly StructureTag[] TagsRequired = tagsRequired.ToArray();
    public readonly StructureTag[] TagsBlacklist = tagsBlacklist.ToArray();
    public Shape MainVolume = mainVolume;
    public TilePalette TilePalette = tilePalette;
}


public struct PaintedType(ushort type, byte paintType = PaintID.None)
{
    public ushort Type = type;
    public byte PaintType = paintType;
    
    public static PaintedType PickRandom(PaintedType[] paintedTypes)
    {
        int index = Terraria.WorldGen.genRand.Next(paintedTypes.Length - 1);
        return paintedTypes[index];
    }
    
    public static void PlaceTile(int x, int y, PaintedType paintedType, BlockType blockType = BlockType.Solid)
    {
        Tile tile = Main.tile[x, y];
        tile.HasTile = true;
        tile.BlockType = blockType;
        tile.TileType = paintedType.Type;
        tile.TileColor = paintedType.PaintType;
    }
    
    public static void PlaceTile(Point16 position, PaintedType paintedType, BlockType blockType = BlockType.Solid)
    {
        PlaceTile(position.X, position.Y, paintedType, blockType);
    }
    
    public static void PlaceTile(Tile tile, PaintedType paintedType, BlockType blockType = BlockType.Solid)
    {
        tile.HasTile = true;
        tile.BlockType = blockType;
        tile.TileType = paintedType.Type;
        tile.TileColor = paintedType.PaintType;
    }
    
    public static void PlaceWall(int x, int y, PaintedType paintedType)
    {
        Tile tile = Main.tile[x, y];
        tile.WallType = paintedType.Type;
        tile.WallColor = paintedType.PaintType;
    }
    
    public static void PlaceWall(Point16 position, PaintedType paintedType)
    {
        PlaceWall(position.X, position.Y, paintedType);
    }
    
    public static void PlaceWall(Tile tile, PaintedType paintedType)
    {
        tile.WallType = paintedType.Type;
        tile.WallColor = paintedType.PaintType;
    }
}