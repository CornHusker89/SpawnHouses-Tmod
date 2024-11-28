#nullable enable
using System;

namespace SpawnHouses.Structures;

public class SubstructureSlot
{
    public readonly ushort XOffset, YOffset;
    public readonly SubstructureSlot? ParentX, ParentY;
    public CustomStructure? Substructure;
    public CustomMultiStructure ParentMultiStructure;
    
    public SubstructureSlot(CustomMultiStructure parentMultiStructure, ushort xOffset, ushort yOffset, SubstructureSlot? parentX, SubstructureSlot? parentY) 
    {
        ParentMultiStructure = parentMultiStructure;
        XOffset = xOffset;
        YOffset = yOffset;
        ParentX = parentX;
        ParentY = parentY;
    }
    
    /// <summary>
    /// Returns resulting customStructure, and sets own property to value
    /// </summary>
    /// <param name="structureID"></param>
    /// <returns></returns>
    public CustomStructure Evaluate(ushort structureID)
    {
        ushort x, y;
        if (ParentX is not null)
        {
            if (ParentX.Substructure is null)
                throw new Exception("Parent (X) Substructure is not Evaluated");
            x = (ushort)(ParentX.Substructure.X + ParentX.Substructure.StructureXSize + XOffset);
        }
        else
            x = (ushort)(ParentMultiStructure.X + XOffset);
        
        if (ParentY is not null)
        {
            if (ParentY.Substructure is null)
                throw new Exception("Parent (Y) Substructure is not Evaluated");
            y = (ushort)(ParentY.Substructure.Y + ParentY.Substructure.StructureYSize + YOffset);
        }
        else
            y = (ushort)(ParentMultiStructure.Y + YOffset);
        
        CustomStructure structure = StructureIDUtils.CreateStructure(structureID, x, y, StructureStatus.NotGenerated);
        Substructure = structure;
        return structure;
    }
    
    
    public bool IsEvaluated()
    {
        return Substructure is not null;
    }
}