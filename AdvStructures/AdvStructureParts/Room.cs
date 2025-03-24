using System.Collections.Generic;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Room
{
    public Shape Volume;
    public List<Gap> Gaps;

    public Room(Shape volume, List<Gap> gaps = null)
    {
        Volume = volume;
        Gaps = gaps ?? [];
    }
}