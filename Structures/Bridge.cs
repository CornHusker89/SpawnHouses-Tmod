namespace SpawnHouses.Structures;

public class Bridge
{
    public readonly string StructureFilePath;
    public readonly ushort StructureLength;
    public readonly short StructureYOffset;
    public readonly double AttemptSlope;
    
    public Bridge(string structureFilePath, ushort structureLength,
        short structureYOffset, double attemptSlope)
    {
        StructureFilePath = structureFilePath;
        StructureLength = structureLength;
        StructureYOffset = structureYOffset;
        AttemptSlope = attemptSlope;
    }
}