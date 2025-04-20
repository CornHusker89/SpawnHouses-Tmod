namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Wall {
    public Shape Volume;
    public bool IsExterior;

    public Wall(Shape volume, bool isExterior = false) {
        Volume = volume;
        IsExterior = isExterior;
    }
}