#nullable enable
using System.Collections.Generic;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types.Geometry;
using Terraria.Utilities;

namespace SpawnHouses.Common.Types;

public interface IGenerator {
    /// <summary>
    ///     all tags that could be created by using this generator
    /// </summary>
    public HashSet<Tag> PossibleTags { get; }

    /// <summary>
    ///     if this generator is allowed to generate the given parameters
    /// </summary>
    /// <param name="generatable"></param>
    /// <param name="param"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    public bool CanGenerate(IGeneratable generatable, IParams param, UnifiedRandom random);

    /// <summary>
    ///     execute the generator with the given parameters. alters structure <see cref="AdvStructure.Tilemap"/>, sets <see cref="IComponent.TagsCurrent"/> for this module
    /// </summary>
    /// <param name="generatable">the generatable that this generator is "completing". modify this object's data</param>
    /// <param name="param">other parameters that are required for generation</param>
    /// <param name="random">the random generator to use for "completing" this generatable</param>
    /// <param name="palette">tile palette to use while "completing" this generatable</param>
    /// <param name="tilemap">tile map to place onto while "completing" this generatable</param>
    /// <remarks>should not be called directly, use <see cref="Generatable{TSelf, TParams,TGenerator}.ExecuteGenerator"/></remarks>
    /// <returns></returns>
    public bool Generate(IGeneratable generatable, IParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public interface IGenerator<TParams, in TGeneratable> : IGenerator
    where TParams : IParams
    where TGeneratable : IGeneratable {
    bool IGenerator.CanGenerate(IGeneratable generatable, IParams param, UnifiedRandom random) => CanGenerate((TGeneratable)generatable, (TParams)param, random);

    bool IGenerator.Generate(IGeneratable generatable, IParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) => Generate((TGeneratable)generatable, (TParams)param, random, palette, tilemap);

    /// <inheritdoc cref="IGenerator.CanGenerate" />
    public bool CanGenerate(TGeneratable generatable, TParams param, UnifiedRandom random);

    /// <inheritdoc cref="IGenerator.Generate" />
    public bool Generate(TGeneratable generatable, TParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public abstract class Generator<TParams, TGeneratable> : IGenerator<TParams, TGeneratable>
    where TParams : IParams
    where TGeneratable : IGeneratable {
    public abstract HashSet<Tag> PossibleTags { get; }

    public abstract bool CanGenerate(TGeneratable generatable, TParams param, UnifiedRandom random);

    public abstract bool Generate(TGeneratable generatable, TParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public abstract class StructureLayoutGenerator : Generator<StructureLayoutParams, StructureLayout> {
    public override bool CanGenerate(StructureLayout structureLayout, StructureLayoutParams param, UnifiedRandom random) => true;

    public abstract override bool Generate(StructureLayout structureLayout, StructureLayoutParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public abstract class VolumeComponentGenerator : Generator<VolumeComponentParams, VolumeComponent> {
    public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => true;

    public abstract override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public abstract class PathComponentGenerator : Generator<PathComponentParams, PathComponent> {
    public override bool CanGenerate(PathComponent component, PathComponentParams param, UnifiedRandom random) => true;

    public abstract override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);

    public abstract Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random);
}
