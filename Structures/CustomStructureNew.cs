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
        public virtual ushort beamTileID { get; set; } = TileID.WoodenBeam;
        public virtual byte beamTilePaintID { get; set; } = PaintID.None;
        public virtual bool canHaveFoundation { get; set; } = false;
        public virtual bool canSampleFoundation { get; set; } = false;
        public virtual ushort foundationTileID { get; set; } = TileID.Dirt;
        public virtual int foudationRadius { get; set; } = 0;
        public virtual int foudationXOffset { get; set; } = 0;
        public virtual int foudationYOffset { get; set; } = 0;
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