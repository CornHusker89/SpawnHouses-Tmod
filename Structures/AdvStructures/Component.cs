using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;
using static SpawnHouses.Structures.AdvStructures.Data;

namespace SpawnHouses.Structures.AdvStructures;

public class Component
{
    public AdvStructure ParentStructure = null;
    public readonly List<ComponentTag> Tags;
    public readonly List<Shape> ConnectingVolumes = [];
    public readonly Dictionary<DataType, object> Data = new Dictionary<DataType, object>();
    public Shape BoundingShape;
    public int X, Y, XSize, YSize;
    
    
    public Component(ComponentParams componentParams)
    {
    }
}