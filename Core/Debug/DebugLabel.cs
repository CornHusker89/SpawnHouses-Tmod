#nullable enable

using Microsoft.Xna.Framework;
using SpawnHouses.Core.AdvGeneratables;
using SpawnHouses.Core.Geometry;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace SpawnHouses.Core.Debug;

public class DebugLabel {
    private readonly bool _hasParentObj;

    private readonly string _name;

    private readonly string? _objType;

    private readonly ushort? _objId;

    private readonly string? _genName;

    private Vector2? _textDimensions;
    
    /// the origin of the label in global tile coordinates
    public Point16 Root;

    public DebugInfoLevel InfoLevel;

    public Color DrawColor;

    
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
    public DebugLabel(Point16 root, IDebugDraw parentObj) {
        Root = root;
        _hasParentObj = true;
        _name = parentObj.InternalName;
        _objType = parentObj.GetType().Name;
        _objId = parentObj is IAdvGeneratable generatable ? generatable.Id : (ushort)0;
        _genName = parentObj is IAdvGeneratable generatable2 ? generatable2.InternalName : null;
        InfoLevel = parentObj.DebugInfoVisibility;
        DrawColor = parentObj.GetDrawColor();
    }

    /// <summary>
    /// </summary>
    /// <param name="root">the origin of the label in global tile coordinates</param>
    /// <param name="infoLevel"></param>
    /// <param name="color"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    public DebugLabel(Point16 root, DebugInfoLevel infoLevel, Color color, string name, string? type = null) {
        Root = root;
        _hasParentObj = false;
        _name = name;
        _objType = type;
        InfoLevel = infoLevel;
        DrawColor = color;
    }

    public string GetDisplayString() {
        string str = "";
        if (InfoLevel.DisplayName)
            str += $"{_name}\n";
        if (InfoLevel.DisplayType && _objType != null)
            str += $"{_objType}\n";
        if (InfoLevel.DisplayId && _objId != 0)
            str += $"Id2: {_objId}\n";
        if (InfoLevel.DisplayGenerator && _genName != null)
            str += $"{_genName}\n";

        // prune trailing newline
        if (str.Length >= 2)
            str = str[..^1];
        return str;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    private Vector2 GetTextDimensions() => FontAssets.MouseText.Value.MeasureString(GetDisplayString());

    public Rectangle GetTextBoundingBox(Point pos) => new(pos.X, pos.Y, (int)TextDimensions.X, (int)TextDimensions.Y);

    public bool IsVisible(TileBox boundingBox) {
        if (!InfoLevel.IsDisplayingText)
            return false;
        
        Rectangle screenRect = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        return screenRect.Intersects(boundingBox.Scale(16));
    }
}