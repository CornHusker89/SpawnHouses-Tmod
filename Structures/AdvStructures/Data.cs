using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.AdvStructures;

public static class Data
{
    
    public enum StructureTag
    {
        
        // ===== basic attributes =====
        HasHousing,
        IsSymmetric,
        
        
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
        RoofSlopeNone,
        
        
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
        public bool HasRoof;

        public StructureParams(
            StructureTag[] structureTagsRequired,
            StructureTag[] structureTagBlacklist,
            Point16 start,
            Point16 end,
            Range volumeRange,
            Range housingRange,
            bool hasRoof = true)
        {
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
            HasRoof = hasRoof;
            
            RerollRanges();
        }
        
        public void RerollRanges()
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
        List<Shape> connectingVolumes,
        Point16 start,
        int length)
    {
        public readonly StructureTag[] TagsRequired = tagsRequired.ToArray();
        public readonly StructureTag[] TagsBlacklist = tagsBlacklist.ToArray();
        public List<Shape> ConnectingVolumes = connectingVolumes;
        public Point16 Start = start;
        public int Length = length;
    }
}