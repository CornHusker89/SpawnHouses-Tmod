#nullable enable
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;

namespace SpawnHouses.Common.Modules.Components;

public class Roof : PathComponent {
    public Roof(PathComponentParams p) : base(p) {
        TagsCurrent.Add(Tags.External);
    }
}