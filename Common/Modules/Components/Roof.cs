#nullable enable
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Common.Types.StructureTypes;

namespace SpawnHouses.Common.Modules.Components;

/// <summary>
///     all roofs' required tags has Tags.External in it on object init
/// </summary>
public class Roof : PathComponent {
    public bool StartExtendable;
    public bool EndExtendable;

    /// <summary>
    ///     if the endpoint with the lower X is extendable, linked to <see cref="StartExtendable" /> and <see cref="EndExtendable" />
    /// </summary>
    public bool LowerXExtendable {
        get => Geometry.Points[0].X <= Geometry.Points[^1].X ? StartExtendable : EndExtendable;
        set {
            if (Geometry.Points[0].X <= Geometry.Points[^1].X)
                StartExtendable = value;
            else
                EndExtendable = value;
        }
    }

    /// <summary>
    ///     if the endpoint with the larger X is extendable, linked to <see cref="StartExtendable" /> and <see cref="EndExtendable" />
    /// </summary>
    public bool HigherXExtendable {
        get => Geometry.Points[0].X > Geometry.Points[^1].X ? StartExtendable : EndExtendable;
        set {
            if (Geometry.Points[0].X > Geometry.Points[^1].X)
                StartExtendable = value;
            else
                EndExtendable = value;
        }
    }
    
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="path"></param>
    /// <param name="startExtendable"></param>
    /// <param name="endExtendable"></param>
    /// <param name="name"></param>
    public Roof(PathComponentParams p, Path path, bool startExtendable, bool endExtendable, string name) : base(p, path, name) {
        StartExtendable = startExtendable;
        EndExtendable = endExtendable;
        p.TagsRequired.Add(Tags.External);
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="path"></param>
    /// <param name="startExtendable"></param>
    /// <param name="endExtendable"></param>
    /// <param name="name"></param>
    public Roof(AdvStructure structure, Path path, bool startExtendable, bool endExtendable, string name) : base(new PathComponentParams(structure), path, name) {
        StartExtendable = startExtendable;
        EndExtendable = endExtendable;
        Params.TagsRequired.Add(Tags.External);
    }
}