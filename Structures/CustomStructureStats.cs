
using Terraria.ID;

namespace SpawnHouses.Structures
{
    // This is the testing house!!!11!!1!!1!
    public class TestHouseStructure : CustomStructure
    {
        public override string FilePath => "Structures/testHouse";
        public override int StructureXSize => 24;
        public override int StructureFloorLength => 19;
        public override int StructureFloorYOffset => 12;
        public override int StructureFloorXOffset => 2;
        public override bool CanHaveBeams => true;
        public override int BeamInterval => 4;
        public override int BeamsXOffset => 3;
        public override int BeamsCount => 5;
        public override bool CanHaveFoundation => true; 
        public override ushort FoundationTileID => TileID.Dirt;
    }
    
    public class MainHouseStructure : CustomStructure
    {
        public override bool Debug => true;
        public override string FilePath => "Structures/mainHouse";
        public override int StructureXSize => 63;
        public override int StructureFloorLength => 63; //41
        public override int StructureFloorYOffset => 26;
        public override int StructureFloorXOffset => 0;
        public override bool CanHaveFoundation => true; 
        public override ushort FoundationTileID => TileID.Dirt;
        public override int FoudationRadius => 31;
        public override int FoudationYOffset => 36;
        public override int FoudationXOffset => 31;
        public override bool CanBlendLeft => true;
        public override bool CanBlendRight => true;
        public override int BlendDistance => 20;
        public override ushort BlendTileID => TileID.Dirt;
    }
    
    public class BeachHouseStructure : CustomStructure
    {
        public override string FilePath => "Structures/beachHouse";
        public override int StructureXSize => 35;
        public override int StructureFloorLength => 23;
        public override int StructureFloorYOffset => 26;
        public override int StructureFloorXOffset => 11;
        public override bool CanHaveBeams => true;
        public override int BeamInterval => 4;
        public override int BeamsXOffset => 1;
        public override int BeamsCount => 3;
        public override ushort BeamTileID => TileID.RichMahoganyBeam;
        public override byte BeamTilePaintID => PaintID.BrownPaint;
        public override bool CanHaveFoundation => true; 
        public override ushort FoundationTileID => TileID.Sand;
        public override int FoudationXOffset => 25;
        public override int FoudationYOffset => 39;
        public override int FoudationRadius => 14;
        public override bool CanBlendRight => true;
        public override int BlendYOffset => 2;
        public override ushort BlendTileID => TileID.Adamantite; //TileID.Sand?
    }
}