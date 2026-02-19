#nullable enable

using Terraria.ID;

namespace SpawnHouses.Common.Palette;

public class TilePalette {
    public required PaintedTypeDecorSet BedroomDecor;
    public required PaintedTypeRoomSet BedroomRoom;

    // ----- One-off Tiles -----

    /// <summary>
    ///     tile, 1-wide, 1-tall that can be placed standalone to represent a junk-y, debris area
    /// </summary>
    public required TilePaintedType? Debris1X1;

    // ----- Sets -----
    public required PaintedTypeFloorSet ExternalFloor;

    public required PaintedTypeWallSet ExternalWall;
    public required PaintedTypeFloorSet InternalFloor;
    public required PaintedTypeWallSet InternalWall;
    public required PaintedTypeDecorSet LivingDecor;

    public required PaintedTypeRoomSet LivingRoom;

    public required PaintedTypeRoofSet Roof;
    public required PaintedTypeDecorSet StorageDecor;
    public required PaintedTypeRoomSet StorageRoom;
    
    public required PaintedTypeDecorSet WorkshopDecor;
    public required PaintedTypeRoomSet WorkshopRoom;
}