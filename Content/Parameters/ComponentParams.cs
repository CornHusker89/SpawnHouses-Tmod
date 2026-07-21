using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Types.RootStructureTypes;

namespace SpawnHouses.Content.Parameters;

public class VolumeComponentParams : IParams {
    public AdvStructure Structure { get; set; }
    public TagMap TagsRequired { get; init; }

    public VolumeComponentParams(AdvStructure structure, TagMap tagsRequired = null) {
        Structure = structure;
        TagsRequired = tagsRequired ?? new TagMap();
    }
}

public class PathComponentParams : IParams {
    public AdvStructure Structure { get; set; }
    public TagMap TagsRequired { get; init; }

    public PathComponentParams(AdvStructure structure, TagMap tagsRequired = null) {
        Structure = structure;
        TagsRequired = tagsRequired ?? new TagMap();
    }
}