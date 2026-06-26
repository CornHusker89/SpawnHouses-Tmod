using System.Collections.Generic;
using Terraria;

namespace SpawnHouses.Common.Debug;

public interface ICanDebugDraw {
    public string Name { get; }
    public DebugInfoLevel DebugInfoVisibility { get; set; }

    /// <summary>
    ///     draws debug geometry, using <see cref="DebugInfoVisibility" /> and <see cref="Name" />, recursively to all drawable objects contained. does not do any text-related drawing
    /// </summary>
    /// <remarks>assumes that a world-relative batch has begun in <see cref="Main.spriteBatch" />. does not end sprite batch</remarks>
    /// <returns>list of labels to be drawn. will not include labels that should be hidden for any reason</returns>
    public List<DebugLabel> DrawDebugGeometry();
}