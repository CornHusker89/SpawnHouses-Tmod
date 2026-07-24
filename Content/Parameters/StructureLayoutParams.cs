using System;
using System.Linq;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Types;
using SpawnHouses.Content.Types.Enums;
using SpawnHouses.Content.Types.RootStructureTypes;
using Terraria.DataStructures;

namespace SpawnHouses.Content.Parameters;

public class StructureLayoutParams : IParams {
    public AdvStructure Structure { get; set; }
    public TagMap TagsRequired { get; }

    public readonly bool CanAddEntryPoints;
    public readonly EntryPoint[] EntryPoints;
    public readonly int Size;

    public StructureLayoutParams(TagMap tagsRequired, EntryPoint[] entryPoints, int size, bool canAddEntryPoints) {
        TagsRequired = tagsRequired;
        EntryPoints = entryPoints;
        Size = size;
        CanAddEntryPoints = canAddEntryPoints;

        if (EntryPoints.Count(entryPoint => entryPoint.Purpose is EntryPointPurpose.GroundLevel) > 2)
            throw new ArgumentException("Cannot have more than 2 ground-level entry points");

        if (EntryPoints.Select(entryPoint => entryPoint.Start.Y).Max() - EntryPoints.Select(entryPoint => entryPoint.Start.Y).Min() + 4 > Size / Length)
            throw new ArgumentException($"Entry points are too far away vertically for a minimum height of {Size / Length} (determined by min volume / length)");

        if (Height <= 4)
            throw new ArgumentException($"Volume ({Size}) is too small compared to the length ({Length}) of the structure, resulting in a too-low total height of {Height}");

        bool hasHousing = TagsRequired.GetValueSafe(Tags.Structure_HasHousing, out int housingCount);

        if (hasHousing) {
            bool hasRooms = TagsRequired.GetValueSafe(Tags.Structure_HasRooms, out int roomCount);
            if (!hasRooms) throw new ArgumentException("Must have rooms tag to have housing");

            if (Size / housingCount < 60)
                throw new ArgumentException($"Volume minimum of {Size} is too small given the housing count minimum of {housingCount}");
            if (Size / housingCount < 60)
                throw new ArgumentException($"Volume maximum of {Size} is too small given the housing count maximum of {housingCount}");
            if (housingCount < 1)
                throw new ArgumentException("housing must be greater than 0");
            if (roomCount > housingCount) throw new ArgumentException($"Room count ({roomCount}) must be greater than or equal to housing count ({housingCount})");
        }
    }

    private int CenterYMin => EntryPoints.Min(entryPoint => entryPoint.Start.Y);
    private int CenterYMax => EntryPoints.Max(entryPoint => entryPoint.End.Y);

    public int LeftEntryPointX => EntryPoints.Min(entryPoint => entryPoint.Start.X);
    public int RightEntryPointX => EntryPoints.Max(entryPoint => entryPoint.End.X);

    public int Length => RightEntryPointX - LeftEntryPointX;
    public int Height => Size / Length;

    /// <summary>calculated using entry points</summary>
    public Point16 Center => new(LeftEntryPointX + (LeftEntryPointX + RightEntryPointX) / 2, CenterYMin + (CenterYMin + CenterYMax) / 2);
}
