using System;

namespace SpawnHouses-Tmod.Structures;

public class CustomChainStructure : CustomStructure
{
    public virtual BoundingBox BoundingBox { get; set; }

    public sbyte Cost { get; set; } = -1;
}
