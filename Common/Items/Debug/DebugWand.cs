#if SPAWNHOUSES_DEBUG

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Core;
using SpawnHouses.Core.RootStructureTypes;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Common.Items.Debug;


public class DebugWand : ModItem {
    private enum ItemMode : byte {
        StructureSelect,
        CycleStructureSelect,
        ToggleLockCameraSelected,
        CycleStructureDebugInfo,
        StructureUpgrade
    }

    private static ItemMode _itemMode;
    private static bool _camLocked;

    public static StructureRoot? SelectedStructure;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }
    
    public override bool AltFunctionUse(Player player) {
        if (Main.dedServ)
            return true;

        byte itemModeNum = (byte)(_itemMode + 1);
        if (itemModeNum >= Enum.GetValuesAsUnderlyingType(typeof(ItemMode)).Length)
            _itemMode = 0;
        else
            _itemMode = (ItemMode)itemModeNum;

        Main.NewText($"mode changed to {_itemMode.ToString()}", Color.Yellow);

        return true;
    }

    public override bool? UseItem(Player player) {
        if (Main.mouseRight || Main.dedServ)
            return true;

        switch (_itemMode) {
            case ItemMode.StructureSelect:
                Point16 worldMousePos = (Main.MouseWorld / 16).ToPoint16();
                var structureList = StructureManager.GetAllStructuresList();

                // if multiple structures are within bounds, select the next structure index after the current one
                List<int> selectedStructureIndexes = [];
                int curSelectedStructureIndex = -1;
                for (int i = 0; i < structureList.Length; i++) {
                    StructureRoot structure = structureList[i];
                    if (structure == SelectedStructure)
                        curSelectedStructureIndex = i;

                    if (structure.Tilemap.InBounds(structure.Tilemap.ConvertToRelative(worldMousePos)))
                        selectedStructureIndexes.Add(i);
                }

                if (selectedStructureIndexes.Count == 0 || (selectedStructureIndexes.Count == 1 && selectedStructureIndexes[0] == curSelectedStructureIndex)) {
                    Main.NewText(SelectedStructure != null ? "deselected current structure" : "no structure found on cursor", Color.Yellow);
                    SelectedStructure = null;
                    CameraManager.ReleaseCamera();
                    return true;
                }

                if (curSelectedStructureIndex == -1)
                    SelectedStructure = structureList[selectedStructureIndexes[0]];
                else
                    // if the last selected structure was at/past the end of the selection candidates, wrap around to the front
                    SelectedStructure = curSelectedStructureIndex >= selectedStructureIndexes[^1] ? structureList[0] : structureList[selectedStructureIndexes.First(index => index > curSelectedStructureIndex)];
                if (_camLocked)
                    CameraManager.TransitionToStructure(SelectedStructure);
                Main.NewText($"selected structure id {SelectedStructure.Id}, name: {SelectedStructure.InternalName}", Color.Yellow);
                return true;

            case ItemMode.CycleStructureSelect:
                ushort selectedIdNum = SelectedStructure?.Id ?? 0;
                var allStructures = StructureManager.GetAllStructuresList();
                if (allStructures.Length == 0) {
                    Main.NewText("no structures currently in world", Color.Yellow);
                    return true;
                }

                int nextStructureIndex = Array.FindIndex(allStructures, s => s.Id > selectedIdNum);
                if (nextStructureIndex == -1)
                    nextStructureIndex = 0;
                SelectedStructure = allStructures[nextStructureIndex];
                if (_camLocked)
                    CameraManager.TransitionToStructure(SelectedStructure);
                Main.NewText($"selected structure id {SelectedStructure.Id}, name: {SelectedStructure.InternalName}", Color.Yellow);
                return true;

            case ItemMode.ToggleLockCameraSelected:
                _camLocked = !_camLocked;
                if (_camLocked && SelectedStructure != null)
                    CameraManager.TransitionToStructure(SelectedStructure);
                else
                    CameraManager.ReleaseCamera();
                Main.NewText($"selected structure camera lock toggled to {_camLocked}", Color.Yellow);
                return true;

            case ItemMode.CycleStructureDebugInfo when SelectedStructure == null:
                StructureManager.DefaultDebugInfoLevel.EnableNext();

                Main.NewText($"set default DebugInfoVisibility to {StructureManager.DefaultDebugInfoLevel.GetDetailedString()}", Color.Yellow);
                return true;

            case ItemMode.CycleStructureDebugInfo:
                SelectedStructure.DebugInfoVisibility.EnableNext();
                if (SelectedStructure is AdvStructure advStructure)
                    advStructure.UpdateDebugVisibility();
                Main.NewText($"set structure id {SelectedStructure.Id}'s DebugInfoVisibility to {SelectedStructure.DebugInfoVisibility.GetDetailedString()}", Color.Yellow);
                return true;

            case ItemMode.StructureUpgrade when SelectedStructure == null:
                Main.NewText("no structure selected", Color.Yellow);
                return true;

            default:
                return true;
        }
    }
}

#endif