using Terraria;

namespace SpawnHouses.Structures.Substructures;

public class Bridge {
    public virtual short orientation { get; set; } = 1; //-1 is left, 1 is right
    public virtual bool useStructure { get; set; } = false;
    public virtual Tile? bridgeTile { get; set; } = null;
    public virtual string structureFilePath { get; set; } = "Structures/_";
    public virtual ushort structureLength { get; set; } = 0;
    public virtual ushort structureHeight { get; set; } = 0;
    public virtual double maxSlope { get; set; } = 1.0;

}