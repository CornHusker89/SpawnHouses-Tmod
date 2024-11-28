using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures;

public abstract class CustomMultiStructure : CustomStructure
{
    ushort[] SubstructureIDs;
    
    protected CustomMultiStructure(string filePath, ushort[] substructureIDs, ushort x = 1000, ushort y = 1000, 
        byte status = StructureStatus.NotGenerated) :
        base(filePath, 10, 10, [], status, x, y)
    {
        SubstructureIDs = substructureIDs;
    }
}