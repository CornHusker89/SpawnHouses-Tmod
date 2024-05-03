using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures
{
    public class CutomStructure {
        private Mod _mod = ModContent.GetInstance<SpawnHouses>();
        
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual bool Debug { get; set; } = false;
        public virtual string FilePath { get; set; } = "Structures/_";
        public virtual int StructureXSize { get; set; } = 1;
        public virtual int StructureYSize { get; set; } = 1;
        
        public virtual ArrayList floors { get; set; } = new ArrayList();
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

        public bool GenerateStructure()
        {
            bool result = StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), _mod);

            FrameTiles();
            
            return result;
        }
    }

    public class Floor {
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual ushort floorLength { get; set; } = 1;
        public virtual bool canHaveBeams { get; set; } = false;
        public virtual ushort beamInterval { get; set; } = 1;
        public virtual ushort beamsXOffset { get; set; } = 0;
        public virtual ushort beamsCount { get; set; } = 0;
        public virtual ushort maxBeamSize { get; set; } = 50;
        public virtual Tile beamTile { get; set; } = None;
        public virtual bool canHaveFoundation { get; set; } = false;
        public virtual bool canSampleFoundation { get; set; } = false;
        public virtual Tile foundationTile { get; set; } = None;
        public virtual int foudationRadius { get; set; } = 0;
        public virtual int foudationXOffset { get; set; } = 0;
        public virtual int foudationYOffset { get; set; } = 0;

        public bool GenerateBeams()
        {
            if (!canHaveBeams)
            {
                throw new Exception("this floor cannot have beams");
            }
            
            for (int currentBeamNum = 0; currentBeamNum < beamsCount; currentBeamNum++)
            {
                bool validBeamLocation = true;
                int y2 = Y + 1; //put us 1 below the floor
                int x2 = X + beamsXOffset + (currentBeamNum * BeamInterval);

                //if there's a tile there already, dont place a beam
                if (Terraria.WorldGen.SolidTile(x2, Y + y2))
                {
                    continue;
                }
				
                while (!Terraria.WorldGen.SolidTile(x2, Y + y2))
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
                    Main.NewText($"X:{x2}, Y:{Y}, X2:{x2}, Y2:{y2}, currentBeamNum:{currentBeamNum}, interval:{BeamInterval}");
                }

                if (validBeamLocation)
                {
                    for (int j = 0; j < y2 - StructureFloorYOffset; j++)
                    {
                        Main.tile[x2, Y + j + StructureFloorYOffset] = beamTile;
                    }

                    //make the tile beneath the beam (and the one just above) a full block
                    Tile bottomTile = Main.tile[x2, Y + y2 - 1];
                    bottomTile.Slope = SlopeType.Solid;
                    bottomTile = Main.tile[x2, Y + y2];
                    bottomTile.Slope = SlopeType.Solid;
                }
            }
            FrameTiles();
            
            return true;
        }
    }

    public class ConnectPoint {
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual string orientation { get; set; } = "right";
        public virtual bool canBlend { get; set; } = false;
        public virtual int blendDistance { get; set; } = 10;
        public virtual ushort blendTileID { get; set; } = 0;
        public virtual Bridge bridgeInfo { get; set; } = None;

        public bool GenerateBridge(other: ConnectPoint) {

            return true;
        }
    }

    public class Bridge {
        public virtual bool useStructure { get; set; } = false;
        public virtual ushort bridgeTileID { get; set; } = false;
        public virtual string structureFilePath { get; set; } = "Structures/_";
        public virtual ushort structureLength { get; set; } = 0;
        public virtual ushort structureHeight { get; set; } = 0;
        public virtual double maxSlope { get; set; } = 1.0;

    }
}