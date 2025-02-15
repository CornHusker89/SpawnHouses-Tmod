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
    /// structure is categorized as having an overall icy/cold theme 
    Ice,
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
    IsFloorGap,
    FloorSolid,
    FloorHollow,
    FloorGroundLevel,
    FloorElevated,
    /// floor is either 1 or 2 tiles thick
    FloorThin,
    /// floor is larger than 2 tiles thick
    FloorThick,
    
    
    // ===== wall =====
    HasWall,
    IsWallGap,
    WallGroundLevel,
    WallElevated,
    
    
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
    public static bool ContainsAny(StructureTag[] tags1, StructureTag[] tags2)
    {
        return tags1.Any(tag => tags2.Contains(tag));
    }
    
    public TilePalette Palette; 
    public StructureTag[] StructureTagsRequired;
    public StructureTag[] StructureTagBlacklist;
    public Point16 Start;
    public Point16 End;
    public int Length;
    public Range VolumeRange;
    public int Volume;
    public int Height;
    public Range HousingRange;
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
        VolumeRange = volumeRange;
        HousingRange = housingRange;
        
        ReRollRanges();
    }
    
    public void ReRollRanges()
    {
        double scale = Terraria.WorldGen.genRand.NextDouble();
        Volume = (int)(VolumeRange.Min + (VolumeRange.Max - VolumeRange.Min) * scale);
        Height = Volume / (End.X - Start.X);
        if (Height <= 4)
            throw new ArgumentException($"Volume ({Volume}) is too small compared to the length ({Length}) of the structure, resulting in a height of {Height}");

        if (HousingRange.Min < 0)
            throw new ArgumentException("Min housing cannot be less than 0");
        if (HousingRange.Max < 0)
            throw new ArgumentException("Max housing cannot be less than 0");
        if (HousingRange.Max < HousingRange.Min)
            throw new ArgumentException("Max Housing is less than min housing");
        if (HousingRange.Max == 0 && StructureTagBlacklist.Contains(StructureTag.HasHousing))
            throw new ArgumentException(
                "Adv structure cannot have a max housing of 0 while blacklisting components with housing");
        Housing = (int)(HousingRange.Min + (HousingRange.Max - HousingRange.Min) * scale);
    }
}


public struct ComponentParams(
    StructureTag[] tagsRequired,
    StructureTag[] tagsBlacklist,
    Shape mainVolume,
    TilePalette tilePalette)
{
    public readonly StructureTag[] TagsRequired = tagsRequired;
    public readonly StructureTag[] TagsBlacklist = tagsBlacklist;
    public Shape MainVolume = mainVolume;
    public readonly TilePalette TilePalette = tilePalette;
}


public struct PaintedType(ushort type, byte paintType = PaintID.None, short style = -1)
{
    public ushort Type = type;
    public byte PaintType = paintType;
    public short Style = style;
    
    public static PaintedType PickRandom(PaintedType[] paintedTypes)
    {
        int index = Terraria.WorldGen.genRand.Next(paintedTypes.Length);
        return paintedTypes[index];
    }
    
    public static void PlaceTile(int x, int y, PaintedType paintedType, BlockType blockType = BlockType.Solid)
    {
        if (paintedType.Style == -1)
        {
            Tile tile = Main.tile[x, y];
            tile.HasTile = true;
            tile.BlockType = blockType;
            tile.TileType = paintedType.Type;
            tile.TileColor = paintedType.PaintType;
        }
        else
        {
            Terraria.WorldGen.PlaceTile(x, y, paintedType.Type, true, true, style: paintedType.Style);
            Tile tile = Main.tile[x, y];
            tile.TileColor = paintedType.PaintType;
        }
    }
    
    public static void PlaceTile(Point16 position, PaintedType paintedType, BlockType blockType = BlockType.Solid)
    {
        PlaceTile(position.X, position.Y, paintedType, blockType);
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
}