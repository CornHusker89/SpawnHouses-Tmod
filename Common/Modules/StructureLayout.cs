using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;

namespace SpawnHouses.Common.Modules.Components;

public abstract class StructureLayout : IGeneratable<StructureLayoutParams, Shape, StructureLayoutGener> {
    public readonly List<Floor> Floors;
    public readonly List<Gap> Gaps;
    public readonly List<Roof> Roofs;
    public readonly List<Wall> Walls;

    public StructureLayout(List<Floor> floors, List<Wall> walls, List<Gap> gaps, List<Roof> roofs) {
        Floors = floors;
        Walls = walls;
        Gaps = gaps;
        Roofs = roofs;
    }

    /// <summary>
    ///     adds <see cref="Tags.External" /> to every component
    /// </summary>
    public void SetComponentExternal() {
        foreach (Floor floor in Floors)
            floor.TagsRequired.Add(Tags.External);
        foreach (Wall wall in Walls)
            wall.TagsRequired.Add(Tags.External);
        foreach (Gap gap in Gaps)
            gap.TagsRequired.Add(Tags.External);
        // roofs are automatically marked as external
    }
}