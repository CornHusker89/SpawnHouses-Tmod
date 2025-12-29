namespace SpawnHouses.Structures.StructureParts;

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
        public MainBasement_SealWall() : base("Structures/StructureFiles/mainBasement/mainBasement_SealWall", 0, -4) {
        }
    }

    public class MainBasement_SealFloor : Seal {
        public MainBasement_SealFloor() : base("Structures/StructureFiles/mainBasement/mainBasement_SealFloor", 0, 0) {
        }
    }

    public class MainBasement_SealRoof : Seal {
        public MainBasement_SealRoof() : base("Structures/StructureFiles/mainBasement/mainBasement_SealRoof", 0, 0) {
        }
    }
}