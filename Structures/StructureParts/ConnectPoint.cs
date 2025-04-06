namespace SpawnHouses.Structures.StructureParts;

public class ConnectPoint {
    public byte Direction;
    public ushort X;
    public short XOffset;
    public ushort Y;
    public short YOffset;

    public ConnectPoint(short xOffset, short yOffset, byte direction) {
        XOffset = xOffset;
        YOffset = yOffset;
        Direction = direction;
    }

    // for cloning
    private ConnectPoint(ushort x, ushort y, short xOffset, short yOffset, byte direction) {
        X = x;
        Y = y;
        XOffset = xOffset;
        YOffset = yOffset;
        Direction = direction;
    }

    public void SetPosition(int mainStructureX, int mainStructureY) {
        X = (ushort)(mainStructureX + XOffset);
        Y = (ushort)(mainStructureY + YOffset);
    }

    public ConnectPoint Clone() {
        return new ConnectPoint(X, Y, XOffset, YOffset, Direction);
    }
}