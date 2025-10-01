using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.AdvStructures.Generation;
using SpawnHouses.AdvStructures.Generation.Components;
using SpawnHouses.Helpers;
using SpawnHouses.Structures;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures;

public class AdvStructure {
    public ExternalLayout ExternalLayout;
    public bool HasFilledComponents;
    public RoomLayout Layout;
    public StructureParams Params;
    public StructureTilemap Tilemap;
    public int XSize, YSize, HousingCount;


    /// <summary>
    /// </summary>
    /// <param name="structureParams"></param>
    /// <param name="generate">
    ///     if true, will call <see cref="ApplyLayoutMethod" />, <see cref="FillComponents" /> and
    ///     <see cref="PlaceTilemap" />
    /// </param>
    public AdvStructure(StructureParams structureParams, bool generate = true) {
        Params = structureParams;
        if (generate) {
            ApplyLayoutMethod();
            FillComponents();
            PlaceTilemap();
        }
    }

    public bool HasLayout => Layout != null;

    /// <summary>
    ///     calculates a structure's layout, and does not alter tiles
    /// </summary>
    /// <param name="generator">layout generator to be used. leave null for a random method</param>
    public bool ApplyLayoutMethod(IStructureLayoutGenerator generator = null) {
        if (generator == null) {
            List<IStructureLayoutGenerator> validGenerators = [];

            foreach (IStructureLayoutGenerator possibleGenerator in StructureLayoutGenerators) {
                if (!possibleGenerator.CanGenerate(Params)) continue;

                var requiredTags = Params.TagsRequired.ToList();
                bool valid = true;
                foreach (StructureTag possibleTag in possibleGenerator.GetPossibleTags()) {
                    if (Params.TagsBlacklist.Contains(possibleTag)) {
                        valid = false;
                        break;
                    }

                    requiredTags.Remove(possibleTag);
                }

                if (valid && requiredTags.Count == 0)
                    validGenerators.Add(possibleGenerator);
            }

            if (validGenerators.Count == 0) throw new Exception($"No structure layout generators found were compatible with the given parameters. required tags: {EnumHelper.ToString(Params.TagsRequired)}, blacklisted tags: {EnumHelper.ToString(Params.TagsBlacklist)}");

            generator = validGenerators[Terraria.WorldGen.genRand.Next(0, validGenerators.Count)];
        }

        bool result = generator.Generate(this);
        if (!result)
            throw new Exception("error in structure layout generator");

        return true;
    }

    /// <summary>
    ///     sets the outside/exterior component/inside tile data within the tilemap, based on the current external layout
    /// </summary>
    public void SetTilesExternalStatus() {
        foreach (Shape shape in ExternalLayout.Floors.Select(floor => floor.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsFloor = true;
            });

        foreach (Shape shape in ExternalLayout.Walls.Select(wall => wall.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsWall = true;
            });

        foreach (Shape shape in ExternalLayout.Gaps.Select(gap => gap.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsGap = true;
            });

        SearchOutside(0, 0);
        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++) {
            StructureTile tile = Tilemap[x, y];
            if (!tile.IsOutside && !tile.IsExteriorComponent) tile.IsInside = true;
        }

        return;

        void SearchOutside(int x, int y) {
            StructureTile thisTile = Tilemap[x, y];
            thisTile.IsOutside = true;
            thisTile.IsNullTile = true;
            thisTile.IsNullWall = true;

            foreach ((int dx, int dy) in ((int, int)[]) [(1, 0), (-1, 0), (0, 1), (0, -1)]) {
                if (!Tilemap.InBounds(x + dx, y + dy)) continue;
                StructureTile nextTile = Tilemap[x + dx, y + dy];
                if (nextTile.IsOutside || nextTile.IsExteriorComponent) continue;

                SearchOutside(x + dx, y + dy);
            }
        }
    }

    private IComponentGenerator GetComponentGenerator(ComponentParams componentParams, IComponentGenerator[] generators) {
        var validGenerators = generators.Where(gen => gen.CanGenerate(componentParams)
                                                      && gen.GetPossibleTags().IsSubsetOf(componentParams.Component.TagsRequired)
                                                      && !gen.GetPossibleTags().Overlaps(componentParams.Component.TagsBlacklist))
            .ToList();
        
        if (validGenerators.Count == 0)
            throw new Exception($"No component generators were found that are compatible with given parameters. type: {componentParams.Component.GetType().FullName}, required tags: {EnumHelper.ToString(componentParams.Component.TagsRequired)}, blacklisted tags: {EnumHelper.ToString(componentParams.Component.TagsBlacklist)}");

        return validGenerators[Terraria.WorldGen.genRand.Next(0, validGenerators.Count)];
    }

    /// <summary>
    ///     assigns room objects to the gaps in the external layout
    /// </summary>
    public void CompleteExternalGaps() {
        foreach (Gap gap in ExternalLayout.Gaps) gap.LowerRoom = RoomLayoutHelper.GetClosestRoom(Layout.Rooms, gap.Volume.Center);
    }

    /// <summary>
    ///     Fills current layout with tiles
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void FillComponents() {
        if (false) //(!HasLayout)
            throw new Exception("No layout has been set");

        List<IVolumeComponent> components = [];
        components.AddRange(ExternalLayout.Floors);
        components.AddRange(ExternalLayout.Walls);
        components.AddRange(ExternalLayout.Gaps);
        // components.AddRange(ExternalLayout.Roofs);
        components.AddRange(Layout.Floors);
        components.AddRange(Layout.Walls);
        components.AddRange(Layout.Gaps);
        components.AddRange(Layout.Rooms);
        
        Dictionary<Type, List<IComponentGenerator>> generatorQueue = [];
        for (int i = 0; i < components.Count; i++) {
            IComponent component = components[i];
            ComponentParams componentParams = ComponentUtils.CreateComponentParamsForType(component, Params.Palette, Tilemap);
            ComponentUtils.ValidateComponent(component);
            component.Id = (ushort)i;
            
            IComponentGenerator[] generators;
            switch (component) {
                case Floor:
                    generators = FloorGenerators;
                    break;
                case Wall:
                    generators = WallGenerators;
                    break;
                case Room:
                    generators = BackgroundGenerators;
                    // generators = StairwayGenerators;
                    // generators = DecorGenerators;
                    break;
                case Roof:
                    generators = RoofGenerators;
                    break;
                case Gap:
                    generators = GapGenerators;
                    break;
                default: {
                    if (component.TagsRequired.Contains(ComponentTag.IsDebugBlocks) || component.TagsRequired.Contains(ComponentTag.IsDebugWalls))
                        generators = DebugGenerators;
                    else
                        throw new Exception($"component type {component.GetType()} generators not found");
                    break;
                }
            }
            
            componentParams.Component = component;
            int generatorIndex = 0;
            Type componentType = component.GetType();
            if (!generatorQueue.TryGetValue(componentType, out var generatorList)) {
                generatorList = [GetComponentGenerator(componentParams, generators)];
                generatorQueue[componentType] = generatorList;
            }
            else {
                while (!generatorList[generatorIndex].CanGenerate(componentParams)) {
                    generatorIndex++;
                    if (generatorIndex >= generatorQueue.Count) generatorList.Add(GetComponentGenerator(componentParams, generators));
                }
            }

            generatorList[generatorIndex].Generate(componentParams);
        }

        HasFilledComponents = true;
    }

    /// <summary>
    ///     paste tiles from this tilemap into game tilemap 
    /// </summary>
    public void PlaceTilemap() {
        if (!HasFilledComponents)
            throw new Exception("No filled components have been set");

        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++)
            Tilemap[x, y].PasteTile(Tilemap.ConvertToGlobal(x, y));

        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++)
            Tilemap[x, y].SetFrames(Tilemap.ConvertToGlobal(x, y));
    }

    public void FinishHousing() {
    }

    #region Generators

    public static IStructureLayoutGenerator[] StructureLayoutGenerators;
    public static IComponentGenerator[] FloorGenerators;
    public static IComponentGenerator[] WallGenerators;
    public static IComponentGenerator[] BackgroundGenerators;
    public static IComponentGenerator[] StairwayGenerators;
    public static IComponentGenerator[] DecorGenerators;
    public static IComponentGenerator[] RoofGenerators;
    public static IComponentGenerator[] GapGenerators;
    public static IComponentGenerator[] DebugGenerators;

    public static void PopulateGenerators() {
        var types = typeof(StructureLayoutGen).GetNestedTypes();
        StructureLayoutGenerators = types.Select(t => Activator.CreateInstance(t) as IStructureLayoutGenerator).ToArray();
        types = typeof(FloorGen).GetNestedTypes();
        FloorGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
        types = typeof(WallGen).GetNestedTypes();
        WallGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
        types = typeof(BackgroundGen).GetNestedTypes();
        BackgroundGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
        types = typeof(StairwayGen).GetNestedTypes();
        StairwayGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
        types = typeof(DecorGen).GetNestedTypes();
        DecorGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
        types = typeof(RoofGen).GetNestedTypes();
        RoofGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
        types = typeof(GapGen).GetNestedTypes();
        GapGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
        types = typeof(DebugGen).GetNestedTypes();
        DebugGenerators = types.Select(t => Activator.CreateInstance(t) as IComponentGenerator).ToArray();
    }

    #endregion
}