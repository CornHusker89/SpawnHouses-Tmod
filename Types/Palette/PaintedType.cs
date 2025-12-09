using System;
using Terraria;
using Terraria.ID;

namespace SpawnHouses.Types.Palette;

public class TilePaintedType : PaintedType {
    public TilePaintedType(ushort tileType, byte paintType = PaintID.None, short style = -1) : base(tileType, paintType, style) {
    }

    /// <summary>
    /// </summary>
    /// <param name="tileTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="tileTypes" /></param>
    /// <param name="style"></param>
    public TilePaintedType(ushort[] tileTypes, byte[] paintTypes = null, short style = -1) : base(tileTypes, paintTypes, style) {
    }
}

public class WallPaintedType : PaintedType {
    public WallPaintedType(ushort tileType, byte paintType = PaintID.None, short style = -1) : base(tileType, paintType, style) {
    }

    /// <summary>
    /// </summary>
    /// <param name="tileTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="tileTypes" /></param>
    /// <param name="style"></param>
    public WallPaintedType(ushort[] tileTypes, byte[] paintTypes = null, short style = -1) : base(tileTypes, paintTypes, style) {
    }
}

public abstract class PaintedType {
    private readonly ushort _tileId;
    private readonly ushort[] _tileIds;

    private readonly byte _paintId;
    private readonly byte[] _paintIds;

    /// if the PaintedType contains multiple values
    public readonly bool IsSeries;

    public readonly short Style;

    /// the evaluated tile and paint ids
    public (ushort tileId, byte paintId) Ids => IsSeries ? (Terraria.WorldGen.genRand.NextFromList(_tileIds), Terraria.WorldGen.genRand.NextFromList(_paintIds)) : (_tileId, _paintId);

    public PaintedType(ushort tileType, byte paintType = PaintID.None, short style = -1) {
        _tileId = tileType;
        _paintId = paintType;
        Style = style;
    }

    /// <summary>
    /// </summary>
    /// <param name="tileTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="tileTypes" /></param>
    /// <param name="style"></param>
    public PaintedType(ushort[] tileTypes, byte[] paintTypes = null, short style = -1) {
        IsSeries = true;
        _tileIds = tileTypes;
        Style = style;

        if (paintTypes == null)
            // creates array with default value, which happens to be the same as PaintID.None
            _paintIds = new byte[_tileIds.Length];
        else if (paintTypes.Length != _tileIds.Length)
            throw new ArgumentException("paintTypes and tileTypes must have the same length", nameof(paintTypes));
        else
            _paintIds = paintTypes;
    }
}