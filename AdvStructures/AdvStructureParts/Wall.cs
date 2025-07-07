namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Wall : IComponent {
    public bool IsExterior;

    public Wall(Shape volume, bool isExterior = false) {
        Volume = volume;
        IsExterior = isExterior;
    }

    public ushort Id { get; set; }
    public Shape Volume { get; set; }
}