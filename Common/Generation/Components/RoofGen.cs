using System.Collections.Generic;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Helpers;
using SpawnHouses.Helpers.Complex;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace SpawnHouses.Common.Generation.Components;

public static class RoofGen {
    /// <summary>
    ///     generic, simple roof with optional gap on the bottom
    /// </summary>
    [ModuleGenerator(typeof(Roof))]
    public class RoofGenerator2 : PathComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External,
                Tags.RoofShort,
                Tags.RoofTall,
                Tags.RoofHasOverhang,
            ],
            ComponentHelper.FillShapeTiles.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random) =>
            new(true, geometry.BoundingBox.topLeft + new Point16(0, -4), geometry.BoundingBox.bottomRight);

        public override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            bool extrudeRoof = param.Structure.LayoutRandom.NextBool();

            Roof roofComponent = (Roof)component;
            if (roofComponent.StartExtendable) {
            }

            Path roofLowerPath = extrudeRoof ? roofComponent.Geometry.GetOffsetEvenPath(-1, true, false) : roofComponent.Geometry;
            Path roofTopPath = roofComponent.Geometry.GetOffsetEvenPath(extrudeRoof ? -2 : -1, true, true);
            roofTopPath.Reverse(); // so that it can form a shape with the other path

            Shape offsetShape = roofLowerPath.ToShape(roofTopPath);
            param.TagsRequired.Add(Tags.SlopingAlgorithm, SlopeHelper.SharpTopCorners);
            param.TagsRequired.Add(Tags.SlopeGrouping, SlopeGrouping.LocalSloping);
            ComponentHelper.FillShapeTiles.Action(roofComponent, offsetShape, (_, _) => palette.Roof.Primary);

            // add large roof parts if necessary
            if (param.TagsRequired.HasTag(Tags.RoofTall)) {
                bool tallLeftSide = random.NextBool(3, 4);
                bool tallRightSide = !tallLeftSide || random.NextBool(3, 4);

                switch (tallLeftSide) {
                    case true when tallRightSide: {
                        Shape topShape = roofTopPath.FillFromBoundingBox(new PartialPoint16(0, 0, false), new Point16(0, -1));
                        ComponentHelper.FillShapeWalls.Action(topShape, param.Structure,
                            (x, _) => x > topShape.BoundingBox.topLeft.X && x < topShape.BoundingBox.bottomRight.X
                                ? palette.Roof.PrimaryRoofBackground
                                : null);
                        break;
                    }
                    case true: {
                        Shape topLeftShape = roofTopPath.FillFromCorner(new Point16(0, 0), new Point16(0, 1));
                        ComponentHelper.FillShapeWalls.Action(topLeftShape, param.Structure,
                            (x, _) => x > topLeftShape.BoundingBox.topLeft.X && x < topLeftShape.BoundingBox.bottomRight.X ? palette.Roof.PrimaryRoofBackground : null);
                        break;
                    }
                    default: { // tall right side
                        Shape topRightShape = roofTopPath.FillFromCorner(new Point16(1, 0), new Point16(0, 1));
                        ComponentHelper.FillShapeWalls.Action(topRightShape, param.Structure,
                            (x, _) => x > topRightShape.BoundingBox.topLeft.X && x < topRightShape.BoundingBox.bottomRight.X ? palette.Roof.PrimaryRoofBackground : null);
                        break;
                    }
                }

                component.TagsCurrent.Add(Tags.RoofTall);

                // TODO: add the little window things when filling
            }
            else if (param.TagsRequired.HasTag(Tags.RoofShort))
                component.TagsCurrent.Add(Tags.RoofShort);

            // create endcaps
            // TODO: make endcaps work
            if (param.TagsRequired.HasTag(Tags.RoofHasOverhang))
                component.TagsCurrent.Add(Tags.RoofHasOverhang);
            if (!roofComponent.LowerXExtendable) {
            }

            // create bottom bg wall section
            Path interiorWallPath = new(roofComponent.Geometry.Points);
            interiorWallPath.Reverse();
            Shape wallsShape = interiorWallPath.ToShape(roofComponent.Geometry);
            ComponentHelper.FillShapeWalls.Action(wallsShape, param.Structure,
                (x, _) => (x > wallsShape.BoundingBox.topLeft.X + 8 || !roofComponent.LowerXExtendable)
                          && (x < wallsShape.BoundingBox.bottomRight.X || !roofComponent.HigherXExtendable)
                    ? palette.Roof.PrimaryInteriorBackground
                    : null);

            component.TagsCurrent.Add(Tags.External);
            
            return true;
        }
    }

    /// <summary>
    ///     simple short roof, separated into many small sections with independent sloping
    /// </summary>
    public class RoofGenerator3 : PathComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External,
                Tags.RoofShort,
                Tags.RoofHasOverhang,
                Tags.HasCustomSloping
            ],
            [
                [Tags.SlopingAlgorithm, Tags.SlopeGrouping]
            ],
            ComponentHelper.FillShapeTiles.PossibleTags
        );

        public override bool CanGenerate(PathComponent component, PathComponentParams param, UnifiedRandom random) => false;

        public override Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random) => new(true, geometry.BoundingBox.topLeft + new Point16(0, -4), geometry.BoundingBox.bottomRight);

        public override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) => true;
    }
}