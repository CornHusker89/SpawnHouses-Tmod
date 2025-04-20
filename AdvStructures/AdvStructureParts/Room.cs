using System.Collections.Generic;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Room {
    public Shape Volume;
    public List<Gap> Gaps;

    /// <summary>If the room has important things inside, alters generation such as gaps</summary>
    public bool HasContents;

    public Room(Shape volume, List<Gap> gaps = null) {
        Volume = volume;
        Gaps = gaps ?? [];
    }
}