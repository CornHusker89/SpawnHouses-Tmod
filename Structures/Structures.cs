using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures
{
    public class CustomStructure
    {
        private Mod _mod = ModContent.GetInstance<SpawnHouses>();

        public virtual bool Debug { get; set; } = false;
        public virtual string FilePath { get; set; } = "Structures/_";
        public virtual int StructureFloorLength { get; set; } = 1;
        public virtual int StructureFloorYOffset { get; set; } = 0;
        public virtual bool HasBeams { get; set; } = false;
        public virtual int BeamInterval { get; set; } = 1;
        public virtual int BeamsXOffset { get; set; } = 0;
        public virtual int BeamsCount { get; set; } = 0;
        public virtual int MaxBeamSize { get; set; } = 50;
        public virtual ushort BeamTileID { get; set; } = TileID.WoodenBeam;
        
        public bool GenerateStructure(int x, int y)
        {
            bool result = StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:x, Y:y), _mod);
            return result;
        }
        
        public bool GenerateBeams(int x, int y)
        {
            if (!HasBeams)
            {
                throw new Exception("structure does not have beams");
            }
            
            for (int i = 0; i < BeamsCount; i++)
            {
                bool validBeamLocation = true;
                int y2 = StructureFloorYOffset + 1; //put us 1 below the floor
                int x2 = x + BeamsXOffset + (i * BeamInterval);

                //if there's a tile there already, dont place a beam
                if (Terraria.WorldGen.SolidTile(x2, y + y2))
                {
                    continue;
                }
				
                while (!Terraria.WorldGen.SolidTile(x2, y + y2))
                {
                    if (y2 >= MaxBeamSize + StructureFloorYOffset)
                    {
                        validBeamLocation = false;
                        break;
                    }
                    y2++;
                }

                if (Debug)
                {
                    Main.NewText($"X:{x2}, Y:{y}, X2:{x2}, Y2:{y2}, i:{i}, interval:{BeamInterval}");
                }

                if (validBeamLocation)
                {
                    for (int j = 0; j < y2 - StructureFloorYOffset; j++)
                    {
                        Tile tile = Main.tile[x2, y + j + StructureFloorYOffset];
                        tile.HasTile = true;
                        tile.TileType = BeamTileID;

                    }

                    //make the tile on the bottom of the beam a full block
                    Tile bottomTile = Main.tile[x2, y + y2];
                    bottomTile.Slope = SlopeType.Solid;
                }
            }
            return true;
        }
    }
    
    public class SurfaceHouseStructure : CustomStructure
    {
        public override string FilePath => "Structures/testHouse";
        public override int StructureFloorLength => 19;
        public override int StructureFloorYOffset => 12;
        public override bool HasBeams => true;
        public override int BeamInterval => 4;
        public override int BeamsXOffset => 3;
        public override int BeamsCount => 5;
        public override int MaxBeamSize => 50;
    }
}
