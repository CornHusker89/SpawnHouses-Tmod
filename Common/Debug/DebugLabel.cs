using Microsoft.Xna.Framework;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace SpawnHouses.Common.Debug;

public class DebugLabel {
    /// the origin of the label in global tile coordinates
    public readonly Point16 Root;

    public readonly ICanDebugDraw ParentObj;

    /// <summary>
    /// </summary>
    /// <param name="root">the origin of the label in global tile coordinates</param>
    /// <param name="parentObj"></param>
    public DebugLabel(Point16 root, ICanDebugDraw parentObj) {
        Root = root;
        ParentObj = parentObj;
    }

    public string GetDisplayString() {
        DebugInfoLevel debugLevel = ParentObj.DebugInfoVisibility;
        string str = "";
        if (debugLevel.DisplayName)
            str += $"{ParentObj.Name}\n";
        if (debugLevel.DisplayType)
            str += $"{ParentObj.GetType().Name}\n";
        if (debugLevel.DisplayId && ParentObj is IGeneratable generatable)
            str += $"Id: {generatable.Id}\n";
        if (debugLevel.DisplayGenerator && ParentObj is IGeneratable generatable2)
            str += $"{generatable2.GetGeneratorName()}\n";

        // prune trailing newline
        if (str.Length >= 2)
            str = str[..^2];
        return str;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public Vector2 GetTextDimensions() => FontAssets.MouseText.Value.MeasureString(GetDisplayString());

    public Rectangle GetRectangle(Point pos) {
        Vector2 dimensions = GetTextDimensions();
        return new Rectangle(pos.X, pos.Y, (int)dimensions.X, (int)dimensions.Y);
    }

    public bool IsVisible(Rectangle boundingBox) {
        if (!ParentObj.DebugInfoVisibility.IsDisplayingText)
            return false;
        
        Rectangle screenRect = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        return screenRect.Intersects(boundingBox.Scale(16));
    }
}