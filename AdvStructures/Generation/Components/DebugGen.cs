using SpawnHouses.Types;
using Terraria.ID;

namespace SpawnHouses.AdvStructures.Generation.Components;

public class DebugGen {
    /// <summary>
    ///     Fills with emerald gem spark
    /// </summary>
    public class DebugBlocksGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsDebugBlocks
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = componentParams.Tilemap[x, y];
                tile.HasTile = true;
                tile.BlockType = BlockType.Solid;
                tile.TileType = TileID.EmeraldGemspark;
                tile.TileColor = PaintID.None;
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills with sapphire gem spark
    /// </summary>
    public class DebugBlocksGenerator2 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsDebugBlocks
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = componentParams.Tilemap[x, y];
                tile.HasTile = true;
                tile.BlockType = BlockType.Solid;
                tile.TileType = TileID.SapphireGemspark;
                tile.TileColor = PaintID.None;
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills with ruby gem spark
    /// </summary>
    public class DebugBlocksGenerator3 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsDebugBlocks
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = componentParams.Tilemap[x, y];
                tile.HasTile = true;
                tile.BlockType = BlockType.Solid;
                tile.TileType = TileID.RubyGemspark;
                tile.TileColor = PaintID.None;
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills with emerald gem spark
    /// </summary>
    public class DebugWallsGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsDebugWalls
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = componentParams.Tilemap[x, y];
                tile.WallType = WallID.EmeraldGemspark;
                tile.WallColor = PaintID.None;
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills with sapphire gem spark
    /// </summary>
    public class DebugWallsGenerator2 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsDebugWalls
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = componentParams.Tilemap[x, y];
                tile.WallType = WallID.SapphireGemspark;
                tile.WallColor = PaintID.None;
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills with ruby gem spark
    /// </summary>
    public class DebugWallsGenerator3 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsDebugWalls
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = componentParams.Tilemap[x, y];
                tile.WallType = WallID.RubyGemspark;
                tile.WallColor = PaintID.None;
            });

            return true;
        }
    }
}