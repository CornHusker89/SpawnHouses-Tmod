namespace SpawnHouses.AdvStructures.AdvStructureParts;

public interface IComponent {
    public ushort Id { get; set; }
    public Shape Volume { get; set; }
}