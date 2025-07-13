using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.AdvStructures.Generation;
using SpawnHouses.AdvStructures.Generation.Components;
using SpawnHouses.Helpers;
using SpawnHouses.StructureHelper;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.WorldBuilding;

namespace SpawnHouses.AdvStructures;

public class AdvStructure {
    public ExternalLayout ExternalLayout;
    public RoomLayout Layout;
    public StructureParams Params;
    public StructureTilemap Tilemap;
    public int XSize, YSize, HousingCount;
    public bool HasFilledComponents;

    public bool HasLayout => Layout != null;


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

            if (validGenerators.Count == 0) {
                throw new Exception($"No structure layout generators found were compatible with the given parameters. required tags: {EnumHelper.ToString(Params.TagsRequired)}, blacklisted tags: {EnumHelper.ToString(Params.TagsBlacklist)}");
            }
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
    public void EvaluateTilemapData() {
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
            foreach ((int dx, int dy) in ((int, int)[]) [(1, 0), (-1, 0), (0, 1), (0, -1)]) {
                if (!Tilemap.IsValidTile(x + dx, y + dy)) continue;

                StructureTile tile = Tilemap[x + dx, y + dy];
                if (tile.IsOutside || tile.IsExteriorComponent) continue;

                tile.IsOutside = true;
                tile.IsNullTile = true;
                tile.IsNullWall = true;
                SearchOutside(x + dx, y + dy);
            }
        }
    }

    private IComponentGenerator GetComponentGenerator(ComponentParams componentParams, IComponentGenerator[] generators) {
        List<IComponentGenerator> validGenerators = [];
        foreach (IComponentGenerator possibleGenerator in generators) {
            var requiredTags = componentParams.TagsRequired.ToList();
            bool valid = true;
            foreach (ComponentTag possibleTag in possibleGenerator.GetPossibleTags()) {
                if (componentParams.TagsBlacklist.Contains(possibleTag)) {
                    valid = false;
                    break;
                }

                requiredTags.Remove(possibleTag);
            }

            if (valid && requiredTags.Count == 0)
                validGenerators.Add(possibleGenerator);
        }

        if (validGenerators.Count == 0)
            throw new Exception($"No components found were compatible with given parameters. required tags: {EnumHelper.ToString(componentParams.TagsRequired)}, blacklisted tags: {EnumHelper.ToString(componentParams.TagsBlacklist)}");

        return validGenerators[Terraria.WorldGen.genRand.Next(0, validGenerators.Count)];
    }

    private void FillComponentSet(List<Shape> volumes, ComponentTag[] tagsRequired, ComponentTag[] tagsBlacklist) {
        if (volumes.Count == 0)
            return;

        ComponentParams componentParams = new(
            tagsRequired,
            tagsBlacklist,
            volumes[0],
            Params.Palette,
            Tilemap
        );

        IComponentGenerator[] generators;
        if (tagsRequired.Contains(ComponentTag.IsFloor))
            generators = FloorGenerators;
        else if (tagsRequired.Contains(ComponentTag.IsWall))
            generators = WallGenerators;
        else if (tagsRequired.Contains(ComponentTag.IsBackground))
            generators = BackgroundGenerators;
        else if (tagsRequired.Contains(ComponentTag.IsStairway))
            generators = StairwayGenerators;
        else if (tagsRequired.Contains(ComponentTag.IsDecor))
            generators = DecorGenerators;
        else if (tagsRequired.Contains(ComponentTag.IsRoof))
            generators = RoofGenerators;
        else if (tagsRequired.Contains(ComponentTag.IsFloorGap) || tagsRequired.Contains(ComponentTag.IsWallGap))
            generators = GapGenerators;
        else if (tagsRequired.Contains(ComponentTag.IsDebugBlocks) || tagsRequired.Contains(ComponentTag.IsDebugWalls))
            generators = DebugGenerators;
        else
            throw new Exception("component category not found");

        List<IComponentGenerator> generatorQueue = [GetComponentGenerator(componentParams, generators)];
        foreach (Shape volume in volumes) {
            componentParams.Volume = volume;
            int generatorIndex = 0;
            while (!generatorQueue[generatorIndex].CanGenerate(componentParams)) {
                generatorIndex++;
                if (generatorIndex >= generatorQueue.Count) {
                    generatorQueue.Add(GetComponentGenerator(componentParams, generators));
                }
            }

            generatorQueue[generatorIndex].Generate(componentParams);
        }
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
        if (!HasLayout)
            throw new Exception("No layout has been set");

        FillComponentSet(ExternalLayout.Floors.Select(floor => floor.Volume).ToList(), [ComponentTag.IsFloor, ComponentTag.External], []);
        FillComponentSet(ExternalLayout.Walls.Select(wall => wall.Volume).ToList(), [ComponentTag.IsWall, ComponentTag.External], []);
        FillComponentSet(ExternalLayout.Gaps.FindAll(gap => !gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsFloorGap, ComponentTag.External], []);
        FillComponentSet(ExternalLayout.Gaps.FindAll(gap => gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsWallGap, ComponentTag.External], []);
        FillComponentSet(ExternalLayout.Roofs.Select(roof => roof.Volume).ToList(), [ComponentTag.IsRoof, ComponentTag.External], []);

        // FillComponentSet(Layout.Floors.Select(floor => floor.Volume).ToList(), [ComponentTag.IsFloor], []);
        // FillComponentSet(Layout.Walls.Select(wall => wall.Volume).ToList(), [ComponentTag.IsWall], []);
        // FillComponentSet(Layout.Gaps.FindAll(gap => !gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsDebugBlocks], []);
        // FillComponentSet(Layout.Gaps.FindAll(gap => gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsDebugBlocks], []);
        //
        // FillComponentSet(Layout.Rooms.Select(room => room.Volume).ToList(), [ComponentTag.IsBackground], []);

        HasFilledComponents = true;
    }

    /// <summary>
    /// </summary>
    public void PlaceTilemap() {
        if (!HasFilledComponents)
            throw new Exception("No filled components have been set");

        int xOffset = Tilemap.WorldTileOffset.X;
        int yOffset = Tilemap.WorldTileOffset.Y;
        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++) {
            Tilemap[x, y].CopyTile(x + xOffset, y + yOffset);
        }

        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++) {
            Tilemap[x, y].SetFrames(x + xOffset, y + yOffset);
        }
    }

    public void FinishHousing() {
    }

    #region Generators

    public static readonly IStructureLayoutGenerator[] StructureLayoutGenerators = [
        new StructureLayoutGen.StructureLayoutGenerator1()
    ];

    public static IComponentGenerator[] FloorGenerators = [
        new FloorGen.FloorGenerator1(),
        new FloorGen.FloorGenerator2(),
        new FloorGen.FloorGenerator3(),
        new FloorGen.FloorGenerator4()
    ];

    public static IComponentGenerator[] WallGenerators = [
        new WallGen.WallGenerator1(),
        new WallGen.WallGenerator2(),
        new WallGen.WallGenerator3()
    ];

    public static IComponentGenerator[] BackgroundGenerators = [
        new BackgroundGen.BackgroundGenerator1(),
        new BackgroundGen.BackgroundGenerator2()
    ];

    public static IComponentGenerator[] StairwayGenerators = [
    ];

    public static IComponentGenerator[] DecorGenerators = [
    ];

    public static IComponentGenerator[] RoofGenerators = [
        new RoofGen.RoofGenerator1()
    ];

    public static IComponentGenerator[] GapGenerators = [
        new GapGen.FloorGapGenerator1(),
        new GapGen.WallGapGenerator1()
    ];

    public static IComponentGenerator[] DebugGenerators = [
        new DebugGen.DebugBlocksGenerator1(),
        new DebugGen.DebugBlocksGenerator2(),
        new DebugGen.DebugBlocksGenerator3(),
        new DebugGen.DebugWallsGenerator1(),
        new DebugGen.DebugWallsGenerator2(),
        new DebugGen.DebugWallsGenerator3()
    ];

    #endregion
}