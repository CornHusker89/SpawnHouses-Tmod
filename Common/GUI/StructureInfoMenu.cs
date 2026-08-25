using System.Collections.Generic;
using StructureHelper.Core.Loaders.UILoading;
using Terraria.UI;

namespace SpawnHouses.Common.GUI;

public class StructureInfoMenu : SmartUIState {
    public override int InsertionIndex(List<GameInterfaceLayer> layers) {
        return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
    }

    // public override void SafeUpdate(GameTime gameTime)
    // {
    //     Recalculate();
    //
    //     if (Main.playerInventory)
    //         TestWand.UIVisible = false;
    //
    //     if (ignoreButton.IsMouseHovering)
    //     {
    //         Tooltip.SetName($"Place with null tiles: {ignoreNulls}");
    //         Tooltip.SetTooltip("If the structure placed manually should have it's null tiles placed or not. Turn this off to get a realistic generation, or on if you want to edit the structure.");
    //         Main.LocalPlayer.mouseInterface = true;
    //     }
    //
    //     if (refreshButton.IsMouseHovering)
    //     {
    //         Tooltip.SetName("Reload");
    //         Tooltip.SetTooltip("Reload structures from the folder, use this if you change the folders contents externally and want to see it reflected here.");
    //         Main.LocalPlayer.mouseInterface = true;
    //     }
    //
    //     if (closeButton.IsMouseHovering)
    //     {
    //         Tooltip.SetName("Close");
    //         Tooltip.SetTooltip("Close this menu");
    //         Main.LocalPlayer.mouseInterface = true;
    //     }
    // }
}