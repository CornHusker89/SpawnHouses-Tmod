using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

using SpawnHouses.Structures.StructureParts;

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
                floor.SetPosition(mainStructureX: X, mainStructureY: Y);

            foreach (ConnectPoint connectPoint in ConnectPoints)
                connectPoint.SetPosition(mainStructureX: X, mainStructureY: Y);
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

    public static class CustomStructureID
    {
        public static readonly byte TestHouse = 1;
        public static readonly byte MainHouse = 2;
        public static readonly byte BeachHouse = 3;
        public static readonly byte BridgeTest = 4;

        public static List<short> MakeCostList(short testHouse = -1, short mainHouse = -1, short beachHouse = -1,
            short bridgeTest = -1)
        {
            return [testHouse, mainHouse, beachHouse, bridgeTest];
        }
    }
}