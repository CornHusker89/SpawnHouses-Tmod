namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Floor : IComponent {
    public bool IsExterior;


    public Floor(Shape volume, bool isExterior = false) {
        Volume = volume;
        IsExterior = isExterior;
    }

    public ushort Id { get; set; }
    public Shape Volume { get; set; }
}