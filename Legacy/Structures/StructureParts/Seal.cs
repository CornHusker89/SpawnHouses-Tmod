namespace SpawnHouses.Legacy.Structures.StructureParts;

public class Seal {
    public Seal(string filePath, short xOffset, short yOffset) {
        FilePath = filePath;
        XOffset = xOffset;
        YOffset = yOffset;
    }

    public string FilePath { get; set; }
    public short XOffset { get; set; }
    public short YOffset { get; set; }


    // -- Seal Presets --

    public class MainBasement_SealWall : Seal {
        public MainBasement_SealWall() : base("Content/Assets/StructureFiles/mainBasement/mainBasement_SealWall.shstruct", 0, -4) {
        }
    }

    public class MainBasement_SealFloor : Seal {
        public MainBasement_SealFloor() : base("Content/Assets/StructureFiles/mainBasement/mainBasement_SealFloor.shstruct", 0, 0) {
        }
    }

    public class MainBasement_SealRoof : Seal {
        public MainBasement_SealRoof() : base("Content/Assets/StructureFiles/mainBasement/mainBasement_SealRoof.shstruct", 0, 0) {
        }
    }
}