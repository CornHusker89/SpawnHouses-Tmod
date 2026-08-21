using System;

namespace SpawnHouses.Core;

public class ValidStructureNotFoundException : Exception {
    public ValidStructureNotFoundException() {
    }

    public ValidStructureNotFoundException(string message) : base(message) {
    }
}

public class StructureFailedGeneration : Exception {
    public StructureFailedGeneration() {
    }

    public StructureFailedGeneration(string message) : base(message) {
    }
}