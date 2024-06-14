namespace SpawnHouses.Structures;

public class Directions {
    public static byte Up = 0;
    public static byte Down = 1;
    public static byte Left = 2;
    public static byte Right = 3;

    public static byte flipDirection(byte direction)
    {
        if (direction == 1 || direction == 3)
            return (byte)(direction - 1);
        else
            return (byte)(direction + 1);
    }
}