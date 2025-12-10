using System;
using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation;

public abstract class ComponentGenerator {
    public readonly HashSet<ComponentTag> PossibleTags = null;
    
    public abstract bool CanGenerate(ComponentParams componentParams);

    public abstract bool Generate(ComponentParams componentParams);
}

public abstract class VolumeComponentGenerator : ComponentGenerator {
    // 2 sets of each method, to handle generic and explicit calls
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
    // 2 sets of each method, to handle generic and explicit calls
    public sealed override bool CanGenerate(ComponentParams componentParams) {
        if (componentParams is PathComponentParams pParams)
            return CanGenerate(pParams);

        throw new Exception("PathComponentGenerators must use PathComponentParams");
    }

    public virtual bool CanGenerate(PathComponentParams componentParams) {
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