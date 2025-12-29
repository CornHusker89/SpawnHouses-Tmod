namespace SpawnHouses.Common.Parameters;

public class ComponentParams : Params {
    protected ComponentParams(AdvStructure structure) : base(structure) {
    }
}

public class VolumeComponentParams : ComponentParams {
    public VolumeComponentParams(AdvStructure structure) : base(structure) {
    }
}

public class PathComponentParams : ComponentParams {
    public PathComponentParams(AdvStructure structure) : base(structure) {
    }
}