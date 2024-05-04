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
            bool structureSuccess = StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), _mod);
            bool frameSuccess = FrameTiles();
            
            return (structureSuccess && frameSuccess);
        }
    }

    public class Floor {
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual ushort floorLength { get; set; } = 1;

        public bool GenerateBeams(Tile beamTile, ushort beamInterval, ushort beamsXOffset = 0, ushort beamsCount, ushort maxBeamSize = 50) 
        {
            return true;
        }

        public bool GenerateFoundation(Tile foundationTile = None, ushort foudationRadius, ushort foudationXOffset = 0, ushort foudationYOffset = 0) 
        {
            if (foundationTile == None) {
                
            }

            return true;
        }
    }

    public class ConnectPoint {
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual short orientation { get; set; } = 1; //-1 is left, 0 is None, 1 is right
        public virtual Bridge bridgeInfo { get; set; } = None;

        public bool GenerateBridge(ConnectPoint other)
        {
            if (bridgeInfo == None || other.bridgeInfo == None) {
                return false;
            }

            return true;
        }

        public bool Blend(Tile blendTile, ushort blendDistance, bool canUseSlopes = true)
        {
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