using Terraria.DataStructures;

namespace SpawnHouses.Common.Types.Interfaces;

public interface IFindable {
    public bool IsFound(Point16 playerPos);

    public void OnFound();
}