using Terraria.DataStructures;

namespace SpawnHouses.Content.Types.DataStructures;

// not a struct because strings aren't primitives
public class AnnotatedPoint16 {
    public readonly Point16 Point;
    public readonly string Name;

    public AnnotatedPoint16(Point16 point, string name) {
        Point = point;
        Name = name;
    }

    public AnnotatedPoint16(int x, int y, string name) {
        Point = new Point16(x, y);
        Name = name;
    }

    public AnnotatedPoint16(int size, string name) {
        Point = new Point16(size);
        Name = name;
    }
}