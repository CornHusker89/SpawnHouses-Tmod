using System.Collections.Generic;
using SpawnHouses.Core.Attributes;
using SpawnHouses.Core.DataStructures;
using SpawnHouses.Core.Enums;
using SpawnHouses.Core.Tagging;
using Terraria.DataStructures;
using StructureInfo = (System.Collections.Generic.Dictionary<string, Terraria.DataStructures.Point16> positionIdsToPositions, SpawnHouses.Core.DataStructures.EntryPoint[] entryPoints, SpawnHouses.Core.Tagging.TagMap tags);

namespace SpawnHouses.Core.Generation.FileStructureTemplates;

[StructureTemplateLoadable(true, false)]
public class WoodHouse1 : FileStructureTemplate {
    public override FileSubstructureData[] Substructures => [
        new(
            "Left_Small",
            "Assets/StructureFiles/mainHouse/mainHouse_Small_Left_v4.shstruct",
            ["Left"]
        ),
        new(
            "Left_Normal",
            "Assets/StructureFiles/mainHouse/mainHouse_Left_v4.shstruct",
            ["Left"]
        ),
        new(
            "Right_Small",
            "Assets/StructureFiles/mainHouse/mainHouse_Small_Right_v4.shstruct",
            ["Right"]
        ),
        new(
            "Right_Normal",
            "Assets/StructureFiles/mainHouse/mainHouse_Right_v4.shstruct",
            ["Right"]
        ),
        new(
            "Top_Normal",
            "Assets/StructureFiles/mainHouse/mainHouse_Top_v4.shstruct",
            ["Top"]
        ),
        new(
            "Rose",
            "Assets/StructureFiles/mainHouse/mainHouse_Rose.shstruct",
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

        int storageCount = 4;
        if (posIdsToStruct["Left"].Name == "Left_Small")
            storageCount += 4;
        else
            storageCount += 8;
        if (posIdsToStruct["Right"].Name == "Right_Small")
            storageCount += 4;
        else
            storageCount += 8;
        
        TagMap tags = new();
        tags.Add(Tags.Theme_Forest);
        tags.Add(Tags.Structure_HasHousing, 3);
        tags.Add(Tags.Structure_HasRoof);
        tags.Add(Tags.Structure_HasStorage, storageCount);
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
}