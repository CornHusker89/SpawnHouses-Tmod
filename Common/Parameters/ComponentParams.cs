using SpawnHouses.Common.Tagging;

namespace SpawnHouses.Common.Parameters;

public class VolumeComponentParams : IParams {
    public AdvStructure Structure { get; init; }
    public TagMap TagsRequired { get; init; }

    public VolumeComponentParams(AdvStructure structure, TagMap tagsRequired = null) {
        Structure = structure;
        TagsRequired = tagsRequired ?? new TagMap();
    }
}

public class PathComponentParams : IParams {
    public AdvStructure Structure { get; init; }
    public TagMap TagsRequired { get; init; }

    public PathComponentParams(AdvStructure structure, TagMap tagsRequired = null) {
        Structure = structure;
        TagsRequired = tagsRequired ?? new TagMap();
    }
}