using System;
using System.Collections;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

using SpawnHouses.Structures.Substructures;

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
        
        public virtual ArrayList Floor { get; set; } = new ();
        public virtual ArrayList ConnectPoint { get; set; } = new ();

        public bool FrameTiles()    
        {
            int centerX = X + (StructureXSize / 2);
            int centerY = Y + (StructureXSize / 2);
            
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(Convert.ToInt32((StructureXSize + StructureYSize) * 1.5) ), new Actions.SetFrames());

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
}