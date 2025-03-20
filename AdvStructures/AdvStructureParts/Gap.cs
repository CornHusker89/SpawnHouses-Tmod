namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Gap(Shape volume, Room lowerRoom, Room higherRoom)
{
    public Shape Volume = volume;
    public Room LowerRoom = lowerRoom;
    public Room HigherRoom = higherRoom;
}