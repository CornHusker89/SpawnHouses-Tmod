using System;
using Terraria;
using Terraria.ID;

namespace SpawnHouses.Types.Palette;

public class TilePaintedType : PaintedType {
    public TilePaintedType(ushort wallType, byte paintType = PaintID.None, short style = -1) : base(wallType, paintType, style) {
    }

    /// <summary>
    /// </summary>
    /// <param name="wallTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="wallTypes" /></param>
    /// <param name="style"></param>
    public TilePaintedType(ushort[] wallTypes, byte[] paintTypes = null, short style = -1) : base(wallTypes, paintTypes, style) {
    }
}

public class WallPaintedType : PaintedType {
    public WallPaintedType(ushort wallType, byte paintType = PaintID.None, short style = -1) : base(wallType, paintType, style) {
    }

    /// <summary>
    /// </summary>
    /// <param name="wallTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="wallTypes" /></param>
    /// <param name="style"></param>
    public WallPaintedType(ushort[] wallTypes, byte[] paintTypes = null, short style = -1) : base(wallTypes, paintTypes, style) {
    }
}

public abstract class PaintedType {
    private readonly ushort _wallId;
    private readonly ushort[] _wallIds;

    private readonly byte _paintId;
    private readonly byte[] _paintIds;

    /// if the PaintedType contains multiple values
    public readonly bool IsSeries;

    public readonly short Style;

    /// the evaluated tile and paint ids
    public (ushort tileId, byte paintId) Ids => IsSeries ? (Terraria.WorldGen.genRand.NextFromList(_wallIds), Terraria.WorldGen.genRand.NextFromList(_paintIds)) : (_wallId, _paintId);

    public PaintedType(ushort wallType, byte paintType = PaintID.None, short style = -1) {
        _wallId = wallType;
        _paintId = paintType;
        Style = style;
    }

    /// <summary>
    /// </summary>
    /// <param name="wallTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="wallTypes" /></param>
    /// <param name="style"></param>
    public PaintedType(ushort[] wallTypes, byte[] paintTypes = null, short style = -1) {
        IsSeries = true;
        _wallIds = wallTypes;
        Style = style;

        if (paintTypes == null)
            // creates array with default value, which happens to be the same as PaintID.None
            _paintIds = new byte[_wallIds.Length];
        else if (paintTypes.Length != _wallIds.Length)
            throw new ArgumentException("paintTypes and tileTypes must have the same length", nameof(paintTypes));
        else
            _paintIds = paintTypes;
    }
}