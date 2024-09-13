using Terraria;
using Terraria.ID;

namespace SpawnHouses.Structures;

public static class GenHelper
{
    /// <summary>
    /// Places a bush (walls) with many variants, from 1x1 to 2x3 at the coordinates given
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tileID"></param>
    public static void PlaceBush(int x, int y, ushort tileID = WallID.LivingLeaf)
        {
        void PlaceWall(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            tile.WallType = tileID;
        }
        
        switch (Terraria.WorldGen.genRand.Next(0, 6))
        {
            case 0:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x - 1, y + 1);
                PlaceWall(x, y + 2);
                PlaceWall(x - 1, y + 2);
                break;
            
            case 1:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x + 1, y);
                PlaceWall(x + 1, y + 1);
                PlaceWall(x, y + 2);
                PlaceWall(x + 1, y + 2);
                break;
            
            case 2:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x + 1, y);
                PlaceWall(x + 1, y + 1);
                PlaceWall(x + 1, y - 1);
                PlaceWall(x + 1, y + 2);
                break;
            
            case 3:
                PlaceWall(x, y);
                PlaceWall(x, y - 1);
                PlaceWall(x, y + 1);
                PlaceWall(x, y + 2);
                break;
            
            case 4:
                PlaceWall(x, y);
                PlaceWall(x, y - 1);
                PlaceWall(x - 1, y - 1);
                PlaceWall(x - 2, y);
                PlaceWall(x, y + 1);
                PlaceWall(x - 2, y + 1);
                PlaceWall(x - 1, y + 2);
                break;
            
            case 5:
                PlaceWall(x - 1, y);
                PlaceWall(x + 1, y);
                PlaceWall(x, y - 1);
                PlaceWall(x + 1, y - 1);
                break;
        }
    }
}