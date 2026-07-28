using System.Collections.Generic;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Types;
using SpawnHouses.Content.Types.Attributes;
using SpawnHouses.Content.Types.DataStructures;
using SpawnHouses.Content.Types.Enums;
using SpawnHouses.Content.Types.RootStructureTypes;
using Terraria.DataStructures;
using StructureInfo = (System.Collections.Generic.Dictionary<string, Terraria.DataStructures.Point16> positionIdsToPositions, SpawnHouses.Content.Types.EntryPoint[] entryPoints, SpawnHouses.Content.Tagging.TagMap tags);

namespace SpawnHouses.Content.Generation.FileStructureTemplates;

[StructureTemplateLoadable(true, false)]
public class WoodHouse1 : FileStructureTemplate {
    public override FileSubstructureData[] Substructures => [
        new(
            "RightLarge",
            "Content/Assets/StructureFiles/mainHouse/mainHouse_Right_v4.shstruct",
            ["Main"]
        )
    ];

    public override string[] PositionIds => [
        "Main"
    ];

    public override StructureInfo GetStructureInfo(Dictionary<string, string> positionIdsToNames) {
        Dictionary<string, Point16> positionIdsToPositions = new([
            new KeyValuePair<string, Point16>("Main", new Point16(0, 0))
        ]);

        EntryPoint[] entryPoints = [
            new(new Point16(0, 10), 3, Direction.Right, EntryPointPurpose.GroundLevel),
            new(new Point16(15, 10), 3, Direction.Left, EntryPointPurpose.GroundLevel)
        ];

        TagMap tags = new();
        tags.Add(Tags.Structure_ForestTheme);
        tags.Add(Tags.Structure_HasHousing, 1);
        tags.Add(Tags.Structure_HasRoof);
        tags.Add(Tags.Structure_HasStorage);
        tags.Add(Tags.Palette_MediumGrey);
        tags.Add(Tags.Palette_MediumBrown);
        tags.Add(Tags.Palette_Stone);
        tags.Add(Tags.Palette_Wood);
        tags.Add(Tags.Roof_Short);

        return (positionIdsToPositions, entryPoints, tags);
    }

    public override void OnTilemapLoaded(FileStructure structure) {
    }
}