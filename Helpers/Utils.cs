using System;
using System.ComponentModel;
using System.Linq;
using SpawnHouses.Enums;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Structures.Structures;
using SpawnHouses.Structures.Structures.ChainStructures;

namespace SpawnHouses.Helpers;

// ReSharper disable InconsistentNaming
public static class Directions {
    public const byte Up = 0;
    public const byte Down = 1;
    public const byte Left = 2;
    public const byte Right = 3;
    public const byte None = 4;

    public static byte FlipDirection(byte direction) {
        if (direction is 1 or 3)
            return (byte)(direction - 1);
        return (byte)(direction + 1);
    }
}

public static class StructureStatus {
    public const byte NotGenerated = 0;
    public const byte GeneratedButNotFound = 1;
    public const byte GeneratedAndFound = 2;
}

public static class GenerateChances {
    public const byte Rejected = 0;
    public const byte Neutral = 1;
    public const byte Guaranteed = 2;
}