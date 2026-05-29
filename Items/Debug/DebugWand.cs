#nullable enable
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class DebugWand : ModItem {
    private static Dictionary<int, string> _itemModes = new([
        new KeyValuePair<int, string>(0, "StructureSelectMode"),
        new KeyValuePair<int, string>(1, "CycleStructureDebugInfoMode")
    ]);

    private static int _itemMode;

    public static AdvStructure? SelectedStructure;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }
    
    public override bool AltFunctionUse(Player player) {
        if (Main.dedServ)
            return true;
        
        _itemMode += 1;
        if (_itemMode >= _itemModes.Count)
            _itemMode = 0;

        Main.NewText($"mode changed to {_itemModes[_itemMode]}", Color.Yellow);

        return true;
    }

    public override bool? UseItem(Player player) {
        if (Main.mouseRight || Main.dedServ)
            return true;
        
        // structure select mode
        if (_itemMode == 0) {
            Point16 worldMousePos = (Main.MouseWorld / 16).ToPoint16();

            // if multiple structures are within bounds, select the next structure index after the current one
            List<int> selectedStructureIndexes = [];
            int curSelectedStructureIndex = -1;
            for (int i = 0; i < StructureManager.StructureList.Count; i++) {
                AdvStructure structure = StructureManager.StructureList[i];
                if (structure == SelectedStructure)
                    curSelectedStructureIndex = i;

                if (structure.Tilemap.InBounds(structure.Tilemap.ConvertToRelative(worldMousePos)))
                    selectedStructureIndexes.Add(i);
            }

            if (selectedStructureIndexes.Count == 0) {
                Main.NewText(SelectedStructure != null ? "deselected current structure" : "no structure found on cursor", Color.Yellow);
                SelectedStructure = null;
                return true;
            }

            if (curSelectedStructureIndex == -1) {
                SelectedStructure = StructureManager.StructureList[selectedStructureIndexes[0]];
            }
            else {
                // if the last selected structure was at/past the end of the selection candidates, wrap around to the front
                if (curSelectedStructureIndex >= selectedStructureIndexes[^1])
                    SelectedStructure = StructureManager.StructureList[0];
                else
                    SelectedStructure = StructureManager.StructureList[selectedStructureIndexes.First(index => index > curSelectedStructureIndex)];
            }

            Main.NewText($"selected structure (layout) id {SelectedStructure.StructureLayout.Id}, name: {SelectedStructure.Name}", Color.Yellow);
        }

        // cycle structure debug info mode
        else if (_itemMode == 1) {
            if (SelectedStructure == null) {
                StructureManager.DefaultDebugInfoLevel.EnableNext();

                Main.NewText($"set default DebugInfoVisibility to {StructureManager.DefaultDebugInfoLevel.GetDetailedString()}", Color.Yellow);
                return true;
            }

            SelectedStructure.DebugInfoVisibility.EnableNext();
            SelectedStructure.UpdateDebugVisibility();
            Main.NewText($"set structure (layout) id {SelectedStructure.StructureLayout.Id}'s DebugInfoVisibility to {SelectedStructure.DebugInfoVisibility.GetDetailedString()}", Color.Yellow);
        }

        return true;
    }
}