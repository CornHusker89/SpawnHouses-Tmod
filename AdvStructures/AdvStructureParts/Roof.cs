namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Roof : IComponent {
    public ushort Id { get; set; }
    public Shape Volume { get; set; }

    public Roof(Shape volume) {
        Volume = volume;
    }
}