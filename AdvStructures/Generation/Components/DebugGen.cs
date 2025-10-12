using System.Collections.Generic;
using SpawnHouses.Types;
using Terraria.ID;

namespace SpawnHouses.AdvStructures.Generation.Components;

public class DebugGen {
    /// <summary>
    ///     Fills with emerald gem spark
    /// </summary>
    public class DebugBlocksGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsDebugBlocks
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            param.Component.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = param.Tilemap[x, y];
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
    public class DebugBlocksGenerator2 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsDebugBlocks
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            param.Component.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = param.Tilemap[x, y];
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
    public class DebugBlocksGenerator3 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsDebugBlocks
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            param.Component.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = param.Tilemap[x, y];
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
    public class DebugWallsGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsDebugWalls
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            param.Component.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = param.Tilemap[x, y];
                tile.WallType = WallID.EmeraldGemspark;
                tile.WallColor = PaintID.None;
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills with sapphire gem spark
    /// </summary>
    public class DebugWallsGenerator2 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsDebugWalls
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            param.Component.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = param.Tilemap[x, y];
                tile.WallType = WallID.SapphireGemspark;
                tile.WallColor = PaintID.None;
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills with ruby gem spark
    /// </summary>
    public class DebugWallsGenerator3 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsDebugWalls
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            param.Component.Volume.ExecuteInArea((x, y) => {
                StructureTile tile = param.Tilemap[x, y];
                tile.WallType = WallID.RubyGemspark;
                tile.WallColor = PaintID.None;
            });

            return true;
        }
    }
}