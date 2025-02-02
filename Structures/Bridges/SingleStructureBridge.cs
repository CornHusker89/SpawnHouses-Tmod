using System;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using SpawnHouses.Structures.StructureParts;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace SpawnHouses.Structures.Bridges;

public class SingleStructureBridge : Bridge
{
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();
    
    public readonly string StructureFilePath;
    public readonly ushort StructureLength;
    public readonly ushort StructureHeight;
    public readonly short StructureOffsetX;
    public readonly short StructureOffsetY;
    
    private Box[] BoundingBoxOverride;
    private bool HasBoundingBox;
    
    public SingleStructureBridge(string structureFilePath, ushort structureLength, ushort structureHeight,
        short structureOffsetX, short structureOffsetY, short deltaX, short deltaY, 
        byte[] inputDirections, Box[] boundingBoxOverride = null, bool hasBoundingBox = true,
        ConnectPoint point1 = null, ConnectPoint point2 = null) 
        : 
        base(inputDirections, deltaX, deltaX, deltaY, deltaY, 
            (sbyte)deltaX, (sbyte)deltaY, point1, point2)
    {
        StructureFilePath = structureFilePath;
        StructureLength = structureLength;
        StructureHeight = structureHeight;
        StructureOffsetX = structureOffsetX;
        StructureOffsetY = structureOffsetY;
        HasBoundingBox = hasBoundingBox;
        
        // BoundingBoxOverride represents an offset from the normal position, not an absolute value
        if (boundingBoxOverride == null)
            BoundingBoxOverride = [new Box(0, 0, 0, 0)];
        else
            BoundingBoxOverride = boundingBoxOverride;
        
        if (point1 != null && point2 != null)
            SetPoints(point1, point2);
    }
    
    public override void SetPoints(ConnectPoint point1, ConnectPoint point2)
    {
        Point1 = point1;
        Point2 = point2;

        int startX = point1.X + StructureOffsetX + 1;
        int startY = point1.Y + StructureOffsetY + 1;
        
        if (HasBoundingBox)
        {
            BoundingBoxes = new Box[BoundingBoxOverride.Length];
            for (int i = 0; i < BoundingBoxOverride.Length; i++)
            {
                BoundingBoxes[i] = new Box
                (
                    startX + BoundingBoxOverride[i].Point1.X,
                    startY + BoundingBoxOverride[i].Point1.Y,
                    startX + BoundingBoxOverride[i].Point2.X + StructureLength - 1,
                    startY + BoundingBoxOverride[i].Point2.Y + StructureHeight - 1
                );
            }
        }
        else
            BoundingBoxes = [];
    }
    
    
    [NoJIT]
    public override void Generate()
    {
        if (Point1 == null || Point2 == null)
            throw new Exception("bridge point 1 or 2 is null");
        
        if (Point2.X - Point1.X - 1 != DeltaXMultiple)
            throw new Exception($"Bridge length cannot be resolved with the given Bridge's length, p1: {Point1.X}, p2 {Point2.X}, distance: {Point2.X - Point1.X - 1}, required distance: {DeltaXMultiple}");
    
        if (Point2.Y - Point1.Y - 1 != DeltaYMultiple)
            throw new Exception($"Bridge height cannot be resolved with the given Bridge's height, p1: {Point1.Y}, p2 {Point2.Y}, distance: {Point2.Y - Point1.Y - 1}, required distance: {DeltaYMultiple}");
        

        StructureHelper.Generator.GenerateStructure(StructureFilePath, new Point16(Point1.X + StructureOffsetX + 1, Point1.Y + StructureOffsetY + 1), _mod);
        
        ushort centerX = (ushort)((Point1.X + Point2.X) / 2);
        ushort centerY = (ushort)((Point1.Y + Point2.Y) / 2);
        int radius = StructureLength + StructureHeight;
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }

    public override SingleStructureBridge Clone()
    {
        Type type = this.GetType();
        SingleStructureBridge bridge = (SingleStructureBridge)Activator.CreateInstance(type)!;
        if (Point1 is not null && Point2 is not null)
            bridge.SetPoints(Point1, Point2);
        return bridge;
    }
    
    
    
    
    // --- bridge presets ---
    
    // Note that bridge directions have the directions in point order, so point1 of the bridge should be inputDirections.[0]
    // use AltGen designation to allow for flipped point order
    
    
    // empty
    public class EmptyBridgeHorizontal : SingleStructureBridge
    {
        public EmptyBridgeHorizontal() : base("Structures/StructureFiles/empty",
            0, 0, 0, 0, -1, -1, [Directions.Right, Directions.Left], hasBoundingBox: false) {}
    }
    public class EmptyBridgeHorizontalAltGen : SingleStructureBridge
    {
        public EmptyBridgeHorizontalAltGen() : base("Structures/StructureFiles/empty",
            0, 0, 0, 0, -1, -1, [Directions.Left, Directions.Right], hasBoundingBox: false) {}
    }
    
    public class EmptyBridgeVertical : SingleStructureBridge
    {
        public EmptyBridgeVertical() : base("Structures/StructureFiles/empty",
            0, 0, 0, 0, -1, -1, [Directions.Down, Directions.Up], hasBoundingBox: false) {}
    }
    public class EmptyBridgeVerticalAltGen : SingleStructureBridge
    {
        public EmptyBridgeVerticalAltGen() : base("Structures/StructureFiles/empty",
            0, 0, 0, 0, -1, -1, [Directions.Up, Directions.Down], hasBoundingBox: false) {}
    }
    
    
    // straight
    public class MainBasementHallway1 : SingleStructureBridge
    {
        public MainBasementHallway1() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway1",
            8, 7, 0, -7, 8, -1, [Directions.Right, Directions.Left]) {}
    }
    public class MainBasementHallway1AltGen : SingleStructureBridge
    {
        public MainBasementHallway1AltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway1",
            8, 7, -9, -7, -10, -1, [Directions.Left, Directions.Right]) {}
    }
    

    
    // L shaped
    public class MainBasementHallway2 : SingleStructureBridge
    {
        public MainBasementHallway2() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway2",
            6, 11, -3, -1, 3, 9, [Directions.Down, Directions.Left], [new Box(0, -1, 0, 0)]) {}
    }
    public class MainBasementHallway2AltGen : SingleStructureBridge
    {
        public MainBasementHallway2AltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway2",
            6, 11, -7, -11, -5, -11, [Directions.Left, Directions.Down], [new Box(0, -1, 0, 0)]) {}
    }
    
    public class MainBasementHallway2Reversed : SingleStructureBridge
    {
        public MainBasementHallway2Reversed() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway2_r",
            6, 11, -3, -1, -4, 9, [Directions.Down, Directions.Right], [new Box(0, -1, 0, 0)]) {}
    } 
    public class MainBasementHallway2ReversedAltGen : SingleStructureBridge
    {
        public MainBasementHallway2ReversedAltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway2_r",
            6, 11, 0, -11, 2, -11, [Directions.Right, Directions.Down], [new Box(0, -1, 0, 0)]) {}
    } 
    
    
    
    // L shaped
    public class MainBasementHallway3 : SingleStructureBridge
    {
        public MainBasementHallway3() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway3",
            6, 11, -3, -11, 3, -6, [Directions.Up, Directions.Left], [new Box(0, 0, 0, -1)]) {}
    }
    public class MainBasementHallway3AltGen : SingleStructureBridge
    {
        public MainBasementHallway3AltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway3",
            6, 11, -7, -6, -5, 4, [Directions.Left, Directions.Up], [new Box(0, 0, 0, -1)]) {}
    }
    
    public class MainBasementHallway3Reversed : SingleStructureBridge
    {
        public MainBasementHallway3Reversed() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway3_r",
            6, 11, -3, -11, -4, -6, [Directions.Up, Directions.Right], [new Box(0, 0, 0, -1)]) {}
    } 
    public class MainBasementHallway3ReversedAltGen : SingleStructureBridge
    {
        public MainBasementHallway3ReversedAltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway3_r",
            6, 11, 0, -6, 2, 4, [Directions.Right, Directions.Up], [new Box(0, 0, 0, -1)]) {}
    } 
    
    
    
    // just a 1-tile short lil mini lil feller
    public class MainBasementHallway6 : SingleStructureBridge
    {
        public MainBasementHallway6() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway6",
            1, 5, 0, -5, 1, -1, [Directions.Right, Directions.Left]) {}
    }
    public class MainBasementHallway6AltGen : SingleStructureBridge
    {
        public MainBasementHallway6AltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway6",
            1, 5, -1, -5, -1, -1, [Directions.Left, Directions.Right]) {}
    }
    
    
    
    // a 3-tile short mini fella, but a lil bigger
    public class MainBasementHallway7 : SingleStructureBridge
    {
        public MainBasementHallway7() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway7",
            3, 5, 0, -5, 3, -1, [Directions.Right, Directions.Left]) {}
    }
    public class MainBasementHallway7AltGen : SingleStructureBridge
    {
        public MainBasementHallway7AltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway7",
            3, 5, -3, -5, -3, -1, [Directions.Left, Directions.Right]) {}
    }
    
    
    
    // vertical bridge
    public class MainBasementHallway8 : SingleStructureBridge
    {
        public MainBasementHallway8() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway8",
            6, 11, -3, -1, -1, 9, [Directions.Down, Directions.Up]) {}
    }
    public class MainBasementHallway8AltGen : SingleStructureBridge
    {
        public MainBasementHallway8AltGen() : base("Structures/StructureFiles/mainBasement/mainBasement_Hallway8",
            6, 11, -3, -12, -1, -11, [Directions.Up, Directions.Down]) {}
    }
    
}    