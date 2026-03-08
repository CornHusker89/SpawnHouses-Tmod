using Terraria;

namespace SpawnHouses.Common.Debug;

public interface IDebugDraw {
    public string Name { get; }
    public DebugInfoLevel DebugInfoVisibility { get; set; }

    /// <summary>
    ///     draws debug information, using <see cref="DebugInfoVisibility" /> and <see cref="Name" />, recursively to all drawable objects contained
    /// </summary>
    /// <remarks>assumes that a world-relative batch has begun in <see cref="Main.spriteBatch" />. does not end sprite batch</remarks>
    public void DrawDebugInfo();
}