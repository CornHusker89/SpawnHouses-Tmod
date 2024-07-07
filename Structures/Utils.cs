namespace SpawnHouses.Structures;

public class Directions 
{
    public const byte Up = 0;
    public const byte Down = 1;
    public const byte Left = 2;
    public const byte Right = 3;

    public static byte flipDirection(byte direction)
    {
        if (direction == 1 || direction == 3)
            return (byte)(direction - 1);
        else
            return (byte)(direction + 1);
    }
}

public class StructureStatus
{
    public const byte NotGenerated = 0;
    public const byte GeneratedButNotFound = 1;
    public const byte GeneratedAndFound = 2;
}