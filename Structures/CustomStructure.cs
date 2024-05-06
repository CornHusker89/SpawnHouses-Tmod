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
    public class CustomStructure {
        private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();
        
        public virtual string FilePath { get; set; } = "Structures/_";
        public virtual ushort StructureXSize { get; set; } = 1;
        public virtual ushort StructureYSize { get; set; } = 1;
        
        public ushort X { get; set; }
        public ushort Y { get; set; }
        
        public Floor[] Floors { get; set; }
        public ConnectPoint[] ConnectPoints { get; set; }

        public void SetSubstructurePositions()
        {
            foreach (Floor floor in Floors)
            {
                floor.SetPosition(mainStructureX: X, mainStructureY: Y);
            }
            foreach (ConnectPoint connectPoint in ConnectPoints)
            {
                connectPoint.SetPosition(mainStructureX: X, mainStructureY: Y);
            }
        }

        public void FrameTiles()    
        {
            int centerX = X + (StructureXSize / 2);
            int centerY = Y + (StructureXSize / 2);
            
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(Convert.ToInt32(StructureXSize + StructureYSize) ), new Actions.SetFrames());
        }
        
        public void FrameTiles(int centerX, int centerY, int radius)    
        {
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
        }

        public void GenerateStructure() 
        {
            StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), _mod);
            FrameTiles();
        }
    }
}