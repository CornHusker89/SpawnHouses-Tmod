using SpawnHouses.Common.Modules;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Stairway : PathComponent {
    public Stairway(Path path, bool placeTilesLowerX, bool isExterior = false) {
        Line = path;
        AddRequiredTag(placeTilesLowerX ? ComponentTags.StairwayTilesLowerX : ComponentTags.StairwayTilesHigherX);
        if (isExterior)
            AddRequiredTag(ComponentTags.External);
    }
}