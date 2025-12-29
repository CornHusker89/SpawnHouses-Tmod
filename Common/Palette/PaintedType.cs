using System;
using Terraria.ID;
using Terraria.Utilities;

namespace SpawnHouses.Types.Palette;

public class TilePaintedType : PaintedType {
    public TilePaintedType(ushort type, byte paintType = PaintID.None, short style = 0) : base(type, paintType, style) {
    }

    /// <summary>
    /// </summary>
    /// <param name="wallTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="wallTypes" /></param>
    /// <param name="styles">must have the same length as <paramref name="wallTypes" /></param>
    public TilePaintedType(ushort[] wallTypes, byte[] paintTypes = null, int[] styles = null) : base(wallTypes, paintTypes, styles) {
    }
}

public class WallPaintedType : PaintedType {
    public WallPaintedType(ushort type, byte paintType = PaintID.None, short style = 0) : base(type, paintType, style) {
    }

    /// <summary>
    /// </summary>
    /// <param name="wallTypes"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="wallTypes" /></param>
    /// <param name="styles">must have the same length as <paramref name="wallTypes" /></param>
    public WallPaintedType(ushort[] wallTypes, byte[] paintTypes = null, int[] styles = null) : base(wallTypes, paintTypes, styles) {
    }
}

public abstract class PaintedType {
    private readonly byte _paintId;
    private readonly byte[] _paintIds;

    private readonly int _style;
    private readonly int[] _styles;
    private readonly ushort _typeId;
    private readonly ushort[] _typeIds;

    /// if the PaintedType contains multiple values
    public readonly bool IsSeries;

    public PaintedType(ushort type, byte paintType = PaintID.None, int style = 0) {
        _typeId = type;
        _paintId = paintType;
        _style = style;
    }

    /// <summary>
    /// </summary>
    /// <param name="types"></param>
    /// <param name="paintTypes">must have the same length as <paramref name="types" /></param>
    /// <param name="styles">must have the same length as <paramref name="types" /></param>
    public PaintedType(ushort[] types, byte[] paintTypes = null, int[] styles = null) {
        IsSeries = true;
        _typeIds = types;

        if (paintTypes == null)
            // creates array with default value, which happens to be the same as PaintID.None
            _paintIds = new byte[_typeIds.Length];
        else if (paintTypes.Length != _typeIds.Length)
            throw new ArgumentException("paintTypes and tileTypes must have the same length", nameof(paintTypes));
        else
            _paintIds = paintTypes;

        if (styles == null)
            // creates array with default value, which happens to be the same as the default style
            _styles = new int[_typeIds.Length];
        else if (styles.Length != _typeIds.Length)
            throw new ArgumentException("paintTypes and tileTypes must have the same length", nameof(styles));
        else
            _styles = styles;
    }

    /// the evaluated tile and paint ids
    public (ushort typeId, byte paintId, int style) GetIds(UnifiedRandom random = null) {
        random ??= Terraria.WorldGen.genRand;
        if (IsSeries) {
            int index = random.Next(_typeIds.Length);
            return (_typeIds[index], _paintIds[index], _styles[index]);
        }

        return (_typeId, _paintId, _style);
    }
}