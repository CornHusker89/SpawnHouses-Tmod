using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures
{
    public class CustomUndergroundStructure : CustomStructure {
        private Mod _mod = ModContent.GetInstance<SpawnHouses>();

        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual bool Debug { get; set; } = false;
        public virtual string FilePath { get; set; } = "Structures/_";
        public virtual int StructureXSize { get; set; } = 1;
        public virtual int StructureYSize { get; set; } = 1;

        public virtual ArrayList dockingPoints { get; set; } = new ArrayList();


        public bool FrameTiles()    
        {
            int centerX = X + (StructureXSize / 2);
            int centerY = Y + (StructureXSize / 2);
            
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle((StructureXSize + StructureYSize) * 1.5), new Actions.SetFrames());

            return true;
        }
        
        public bool FrameTiles(int centerX, int centerY, int radius)    
        {
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());

            return true;
        }
    }

    public class UndergroundHouseStructure1 : CustomStructure
    {
        public override string FilePath => "Structures/_";
        public override int StructureXSize => 0;
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
}