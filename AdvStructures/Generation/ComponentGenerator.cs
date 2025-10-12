using System;
using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation;

public abstract class ComponentGenerator {
    public abstract HashSet<ComponentTag> GetPossibleTags();

    public abstract bool CanGenerate(ComponentParams componentParams);

    public abstract bool Generate(ComponentParams componentParams);
}

public abstract class VolumeComponentGenerator : ComponentGenerator {
    public sealed override bool CanGenerate(ComponentParams componentParams) {
        if (componentParams is VolumeComponentParams vParams)
            return CanGenerate(vParams);

        throw new Exception("VolumeComponentGenerators must use VolumeComponentParams");
    }

    public virtual bool CanGenerate(VolumeComponentParams componentParams) {
        return true;
    }

    public sealed override bool Generate(ComponentParams componentParams) {
        if (componentParams is VolumeComponentParams vParams)
            return Generate(vParams);

        throw new Exception("VolumeComponentGenerators must use VolumeComponentParams");
    }

    public virtual bool Generate(VolumeComponentParams componentParams) {
        return true;
    }
}

public abstract class PathComponentGenerator : ComponentGenerator {
    public sealed override bool CanGenerate(ComponentParams componentParams) {
        if (componentParams is PathComponentParams pParams)
            return CanGenerate(pParams);

        throw new Exception("PathComponentGenerators must use PathComponentParams");
    }

    public bool CanGenerate(PathComponentParams componentParams) {
        return true;
    }

    public sealed override bool Generate(ComponentParams componentParams) {
        if (componentParams is PathComponentParams vParams)
            return Generate(vParams);

        throw new Exception("PathComponentGenerators must use PathComponentParams");
    }

    public virtual bool Generate(PathComponentParams componentParams) {
        return true;
    }
}