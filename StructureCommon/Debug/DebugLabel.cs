using Microsoft.Xna.Framework;
using SpawnHouses.StructureCommon.Types.DataStructures;
using SpawnHouses.StructureCommon.Types.Interfaces;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace SpawnHouses.StructureCommon.Debug;

public class DebugLabel {
    /// the origin of the label in global tile coordinates
    public Point16 Root;

    public readonly ICanDebugDraw ParentObj;

    private Vector2? _textDimensions;

    public Vector2 TextDimensions {
        get {
            _textDimensions ??= GetTextDimensions();
            return _textDimensions.Value;
        }
    }

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
        if (debugLevel.DisplayId && ParentObj is IAdvGeneratable generatable)
            str += $"Id2: {generatable.Id}\n";
        if (debugLevel.DisplayGenerator && ParentObj is IAdvGeneratable generatable2)
            str += $"{generatable2.GetGeneratorName()}\n";

        // prune trailing newline
        if (str.Length >= 2)
            str = str[..^2];
        return str;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    private Vector2 GetTextDimensions() => FontAssets.MouseText.Value.MeasureString(GetDisplayString());

    public Rectangle GetTextBoundingBox(Point pos) => new(pos.X, pos.Y, (int)TextDimensions.X, (int)TextDimensions.Y);

    public bool IsVisible(TileBox boundingBox) {
        if (!ParentObj.DebugInfoVisibility.IsDisplayingText)
            return false;
        
        Rectangle screenRect = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        return screenRect.Intersects(boundingBox.Scale(16));
    }
}