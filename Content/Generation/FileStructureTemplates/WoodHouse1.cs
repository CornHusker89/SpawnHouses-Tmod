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
            "Left_Small",
            "Content/Assets/StructureFiles/mainHouse/mainHouse_Small_Left_v4.shstruct",
            ["Left"]
        ),
        new(
            "Left_Normal",
            "Content/Assets/StructureFiles/mainHouse/mainHouse_Left_v4.shstruct",
            ["Left"]
        ),
        new(
            "Right_Small",
            "Content/Assets/StructureFiles/mainHouse/mainHouse_Small_Right_v4.shstruct",
            ["Right"]
        ),
        new(
            "Right_Normal",
            "Content/Assets/StructureFiles/mainHouse/mainHouse_Right_v4.shstruct",
            ["Right"]
        ),
        new(
            "Top_Normal",
            "Content/Assets/StructureFiles/mainHouse/mainHouse_Top_v4.shstruct",
            ["Top"]
        ),
        new(
            "Rose",
            "Content/Assets/StructureFiles/mainHouse/mainHouse_Rose.shstruct",
            ["Rose"]
        )
    ];

    public override string[] PositionIds => [
        "Left",
        "Right",
        "Top",
        "Rose"
    ];

    public override StructureInfo GetStructureInfo(Dictionary<string, FileSubstructureData> posIdsToStruct) {
        Dictionary<string, Point16> positionIdsToPositions = new([
            new KeyValuePair<string, Point16>("Left", new Point16(0, 10)),
            new KeyValuePair<string, Point16>("Right", new Point16((int)posIdsToStruct["Left"].Size.X, 10)),
            new KeyValuePair<string, Point16>("Top", new Point16(posIdsToStruct["Left"].Size.X - 14, 0)),
            new KeyValuePair<string, Point16>("Rose", new Point16(posIdsToStruct["Left"].Size.X - 1, 18))
        ]);

        EntryPoint[] entryPoints = [
            new(new Point16(0, 10), 3, Direction.Right, EntryPointPurpose.GroundLevel),
            new(new Point16(15, 10), 3, Direction.Left, EntryPointPurpose.GroundLevel)
        ];

        TagMap tags = new();
        tags.Add(Tags.Theme_Forest);
        tags.Add(Tags.Structure_HasHousing, 1);
        tags.Add(Tags.Structure_HasRoof);
        tags.Add(Tags.Structure_HasStorage);
        tags.Add(Tags.Structure_HasSomeRectangleRooms);
        tags.Add(Tags.Structure_RoofExtendsPastEntryPoints, 5);
        tags.Add(Tags.Structure_ProgressionSafe);
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