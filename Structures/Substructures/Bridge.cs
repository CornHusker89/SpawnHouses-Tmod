using Terraria;

namespace SpawnHouses.Structures.Substructures;

public class Bridge {
    public short Orientation { get; set; } = 0; //-1 is left, 1 is right
    public bool UseStructure { get; set; } = false;
    public Tile? BridgeTile { get; set; } = null;
    public string StructureFilePath { get; set; } = "Structures/_";
    public ushort StructureLength { get; set; } = 0;
    public ushort StructureHeight { get; set; } = 0;
    public double MaxSlope { get; set; } = 1.0;

}