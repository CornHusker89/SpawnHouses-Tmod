using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.AdvStructures;

public static class Data
{
    public enum ComponentTag
    {
        // ===== component types =====
        /// component is described as a roof
        Roof,
        /// component is described as a room, typically with some form of npc housing
        Room,
        /// component is described as a volume, usually empty or filled with tiles
        Volume,
                        
        
        // ===== room tags =====
        RoomHasHousing,
        RoomFlatFloor,
        RoomFlatCeiling,
        /// room is 10 - 19 tiles wide
        RoomSmallLength,
        /// room is 20 - 29 tiles wide
        RoomMediumLength,
        /// room is 30 - 39 tiles wide
        RoomLargeLength,
        /// room is 4 - 5 tiles tall
        RoomSmallHeight,
        /// room is 6 - 8 tiles tall
        RoomMediumHeight,
        /// room is 9 - 20 tiles tall
        RoomLargeHeight,

        
        // ===== roof tags =====
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
    
    public enum StructureTag
    {
        
        // ===== basic attributes =====
        HasHousing,
        HasRoof,
        IsSymmetric,
        
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
        public ComponentTag[] ComponentTagsRequiredAll;
        public ComponentTag[] ComponentTagsRequiredOne;
        public ComponentTag[] ComponentTagBlacklist;
        public StructureTag[] StructureTagsRequired;
        public StructureTag[] StructureTagBlacklist;
        public Point16 Start;
        public Point16 End;
        public int MinVolume;
        public int MaxVolume;
        public int MinHousing;
        public int MaxHousing;
        public bool HasRoof;

        public StructureParams(ComponentTag[] componentTagsRequiredAll,
            ComponentTag[] componentTagsRequiredOne,
            ComponentTag[] componentTagBlacklist,
            StructureTag[] structureTagsRequired,
            StructureTag[] structureTagBlacklist,
            Point16 start,
            Point16 end,
            int minVolume,
            int maxVolume,
            int minHousing = 0,
            int maxHousing = 0,
            bool hasRoof = true)
        {
            ComponentTagsRequiredAll = componentTagsRequiredAll;
            ComponentTagsRequiredOne = componentTagsRequiredOne;
            ComponentTagBlacklist = componentTagBlacklist;
            StructureTagsRequired = structureTagsRequired;
            StructureTagBlacklist = structureTagBlacklist;
            Start = start;
            End = end;
            if (Start.X > End.X)
                (Start, End) = (End, Start);
            MinVolume = minVolume;
            MaxVolume = maxVolume;
            MinHousing = minHousing;
            MaxHousing = maxHousing;
            HasRoof = hasRoof;
        }
    }
    
    public struct ComponentParams(
        List<ComponentTag> tagsRequired,
        List<ComponentTag> tagsBlacklist,
        List<Shape> connectingVolumes,
        Point16 start,
        int length)
    {
        public readonly ComponentTag[] TagsRequired = tagsRequired.ToArray();
        public readonly ComponentTag[] TagsBlacklist = tagsBlacklist.ToArray();
        public List<Shape> ConnectingVolumes = connectingVolumes;
        public Point16 Start = start;
        public int Length = length;
    }
}