using System.Collections.Generic;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Room {
    public List<Gap> Gaps;
    public Shape Volume;

    public Room(Shape volume, List<Gap> gaps = null) {
        Volume = volume;
        Gaps = gaps ?? [];
    }
}