using System.Collections.Generic;
using SpawnHouses.Core.Palette;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.Tagging;
using SpawnHouses.Core.Tiles;
using Terraria.Utilities;

namespace SpawnHouses.Core.Interfaces;

public interface IAdvGenerator {
    /// <summary>
    ///     if true, generator will only be used for rebuilding existing structures, and not for making new structures
    /// </summary>
    public bool Depreciated { get; set; }
    
    /// <summary>
    ///     all tags that could be created by using this advGenerator
    /// </summary>
    public HashSet<Tag> PossibleTags { get; }

    /// <summary>
    ///     if this advGenerator is allowed to generate the given parameters
    /// </summary>
    /// <param name="advGeneratable"></param>
    /// <param name="param"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    public bool CanGenerate(IAdvGeneratable advGeneratable, IParams param, UnifiedRandom random);

    /// <summary>
    ///     execute the advGenerator with the given parameters. alters structure <see cref="StructureRoot.Tilemap" />, sets <see cref="IComponent.TagsCurrent" /> for this module
    /// </summary>
    /// <param name="advGeneratable">the advGeneratable that this advGenerator is "completing". modify this object's data</param>
    /// <param name="param">other parameters that are required for generation</param>
    /// <param name="random">the random advGenerator to use for "completing" this advGeneratable</param>
    /// <param name="palette">tile palette to use while "completing" this advGeneratable</param>
    /// <param name="tilemap">tile map to place onto while "completing" this advGeneratable</param>
    /// <remarks>should not be called directly, use <see cref="AdvGeneratable{TSelf,TParams,TGenerator}.ExecuteGenerator" /></remarks>
    /// <returns></returns>
    public bool Generate(IAdvGeneratable advGeneratable, IParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}

public interface IAdvGenerator<TParams, in TGeneratable> : IAdvGenerator
    where TParams : IParams
    where TGeneratable : IAdvGeneratable {
    bool IAdvGenerator.CanGenerate(IAdvGeneratable advGeneratable, IParams param, UnifiedRandom random) => CanGenerate((TGeneratable)advGeneratable, (TParams)param, random);

    bool IAdvGenerator.Generate(IAdvGeneratable advGeneratable, IParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) => Generate((TGeneratable)advGeneratable, (TParams)param, random, palette, tilemap);

    /// <inheritdoc cref="IAdvGenerator.CanGenerate" />
    public bool CanGenerate(TGeneratable generatable, TParams param, UnifiedRandom random);

    /// <inheritdoc cref="IAdvGenerator.Generate" />
    public bool Generate(TGeneratable generatable, TParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap);
}