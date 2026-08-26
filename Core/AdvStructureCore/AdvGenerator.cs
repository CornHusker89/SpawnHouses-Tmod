#nullable enable
using System.Collections.Generic;
using SpawnHouses.Core.AdvGeneratables;
using SpawnHouses.Core.Geometry;
using SpawnHouses.Core.Palette;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.Tagging;
using SpawnHouses.Core.Tiles;
using Terraria.Utilities;

namespace SpawnHouses.Core.AdvStructureCore;

public abstract class AdvGenerator<TParams, TGeneratable> : IAdvGenerator<TParams, TGeneratable>
    where TParams : IParams
    where TGeneratable : IAdvGeneratable {
    public bool Depreciated { get; set; }
    
    public abstract HashSet<Tag> PossibleTags { get; }

    public abstract bool CanGenerate(TGeneratable generatable, TParams param, UnifiedRandom random);

    public abstract bool Generate(TGeneratable generatable, TParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public abstract class StructureLayoutAdvGenerator : AdvGenerator<StructureLayoutParams, StructureLayout> {
    public override bool CanGenerate(StructureLayout structureLayout, StructureLayoutParams param, UnifiedRandom random) => true;

    public abstract override bool Generate(StructureLayout structureLayout, StructureLayoutParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public abstract class VolumeComponentAdvGenerator : AdvGenerator<VolumeComponentParams, VolumeComponent> {
    public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => true;

    public abstract override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public abstract class PathComponentAdvGenerator : AdvGenerator<PathComponentParams, PathComponent> {
    public override bool CanGenerate(PathComponent component, PathComponentParams param, UnifiedRandom random) => true;

    public abstract override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);

    public abstract Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random);
}