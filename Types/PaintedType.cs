using Terraria.ID;

namespace SpawnHouses.Types;

public struct PaintedType(ushort type, byte paintType = PaintID.None, short style = -1) {
    public ushort Type = type;
    public byte PaintType = paintType;
    public short Style = style;

    public static PaintedType PickRandom(PaintedType[] paintedTypes) {
        int index = Terraria.WorldGen.genRand.Next(paintedTypes.Length);
        return paintedTypes[index];
    }
}