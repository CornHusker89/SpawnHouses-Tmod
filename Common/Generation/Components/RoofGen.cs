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
    ///     Intended to be a taller, gothic, dramatic roof type. Has custom roof sloping
    /// </summary>
    [ModuleGenerator(typeof(Roof))]
    public class RoofGenerator2 : PathComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External,
                Tags.RoofShort,
                Tags.RoofTall,
                Tags.RoofHasOverhang,
                Tags.HasCustomSloping
            ],
            [
                [Tags.SlopingAlgorithm, Tags.SlopeGrouping]
            ],
            ComponentHelper.FillShapeTiles.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random) => new(true, geometry.BoundingBox.topLeft + new Point16(0, -2), geometry.BoundingBox.bottomRight);

        public override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            bool bigEndCaps = param.Structure.LayoutRandom.NextBool();
            bool extrudeRoof = param.Structure.LayoutRandom.NextBool();

            Roof roofComponent = (Roof)component;
            if (roofComponent.StartExtendable) {
            }

            Path upperMiddlePath = extrudeRoof ? roofComponent.Geometry.GetOffsetEvenPath(-1, true, true) : roofComponent.Geometry;
            Path topPath = roofComponent.Geometry.GetOffsetEvenPath(extrudeRoof ? -2 : -1, true, true);
            topPath.Reverse(); // so that it can form a shape with the other path

            Shape offsetShape = upperMiddlePath.ToShape(topPath);
            param.TagsRequired.Add(Tags.SlopingAlgorithm, SlopeHelper.SharpTopCorners);
            param.TagsRequired.Add(Tags.SlopeGrouping, SlopeGrouping.LocalSloping);
            ComponentHelper.FillShapeTiles.Action(roofComponent, offsetShape, (_, _) => palette.Roof.Primary);

            // add large roof parts if necessary
            if (param.TagsRequired.HasTag(Tags.RoofTall)) {
                bool tallLeftSide = random.NextBool(3, 4);
                bool tallRightSide = !tallLeftSide || random.NextBool(3, 4);

                switch (tallLeftSide) {
                    case true when tallRightSide: {
                        Shape topShape = topPath.FillFromBoundingBox(new PartialPoint16(0, 0, false), new Point16(0, -1));
                        ComponentHelper.FillShapeWalls.Action(topShape, param.Structure,
                            (x, _) => x > topShape.BoundingBox.topLeft.X && x < topShape.BoundingBox.bottomRight.X
                                ? palette.Roof.PrimaryRoofBackground
                                : null);
                        break;
                    }
                    case true: {
                        Shape topLeftShape = topPath.FillFromCorner(new Point16(0, 0), new Point16(0, 1));
                        ComponentHelper.FillShapeWalls.Action(topLeftShape, param.Structure,
                            (x, _) => x > topLeftShape.BoundingBox.topLeft.X && x < topLeftShape.BoundingBox.bottomRight.X ? palette.Roof.PrimaryRoofBackground : null);
                        break;
                    }
                    default: { // tall right side
                        Shape topRightShape = topPath.FillFromCorner(new Point16(1, 0), new Point16(0, 1));
                        ComponentHelper.FillShapeWalls.Action(topRightShape, param.Structure,
                            (x, _) => x > topRightShape.BoundingBox.topLeft.X && x < topRightShape.BoundingBox.bottomRight.X ? palette.Roof.PrimaryRoofBackground : null);
                        break;
                    }
                }
            }

            // create endcaps
            if (!roofComponent.LowerXExtendable) {
            }
            
            // create bottom wall section
            if (extrudeRoof) {
                Shape wallsShape = upperMiddlePath.ToShape(roofComponent.Geometry);
                ComponentHelper.FillShapeWalls.Action(wallsShape, param.Structure,
                    (x, _) => (x > wallsShape.BoundingBox.topLeft.X || !roofComponent.LowerXExtendable)
                              && (x < wallsShape.BoundingBox.bottomRight.X || !roofComponent.HigherXExtendable)
                        ? palette.Roof.PrimaryInteriorBackground
                        : null);
            }

            return true;
        }
    }
}