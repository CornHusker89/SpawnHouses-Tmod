
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Generation;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.IO;
using Terraria.Localization;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace SpawnHouses.Structures
{
    public class SurfaceHouse : ModSystem
    {
        // 3. These lines setup the localization for the message shown during world generation. Update your localization files after building and reloading the mod to provide values for this.
        public static LocalizedText WorldGenCustomHousesPassMessage { get; private set; }

        public override void SetStaticDefaults() {
	        WorldGenCustomHousesPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(WorldGenCustomHousesPassMessage)}"));
        }
        
        // 4. We use the ModifyWorldGenTasks method to tell the game the order that our world generation code should run
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
            // 5. We use FindIndex to locate the index of the vanilla world generation task called "Sunflowers". This ensures our code runs at the correct step.
            int SunflowersIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
            if (SunflowersIndex != -1) {
                // 6. We register our world generation pass by passing in an instance of our custom GenPass class below. The GenPass class will execute our world generation code.
                tasks.Insert(SunflowersIndex + 1, new WorldGenCustomHousesPass("World Gen Custom Houses", 100f));
            }
        }
    }
    
	// 7. Make sure to inherit from the GenPass class.
	public class WorldGenCustomHousesPass : GenPass
	{
		Mod mod = ModContent.GetInstance<SpawnHouses>();
		public WorldGenCustomHousesPass(string name, float loadWeight) : base(name, loadWeight) {
		}

		// 8. The ApplyPass method is where the actual world generation code is placed.
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			
			// 9. Finally, we do the actual world generation code.
			string structurePath = "Structures/testHouse";
			int structureFloorLength = 19;
			int beamInterval = 4;
			
			bool foundLocation = false;
			int x = 0;
			int y = 0;
			while (!foundLocation)
			{
				x = Terraria.WorldGen.genRand.Next(0, Main.maxTilesX);
				y = 1;
				while (y < Main.worldSurface) {
					if (Terraria.WorldGen.SolidTile(x, y)) {
						break;
					}
					y++;
				}
				
				Tile tile = Main.tile[x, y];
				if (tile.HasTile && tile.TileType == TileID.JungleGrass)
				{
					foundLocation = true;
				}
			}

			y -= 15; //the structure spawning has an offset

			int beamsXOffset = (structureFloorLength % beamInterval) / 2;
			for (int i = beamsXOffset; i < structureFloorLength / beamInterval; i += beamInterval)
			{
				bool validBeamLocation = true;
				int y2 = 0;
				while (!Terraria.WorldGen.SolidTile(x, y + y2))
				{
					if (y2 >= 50)
					{
						validBeamLocation = false;
						break;
					}
					y2++;
				}

				y2 += 10; //remove

				if (validBeamLocation)
				{
					for (int j = 0; j < y2; j++)
					{
						Tile tile = Main.tile[x, y + y2];
						tile.TileType = TileID.WoodenBeam;
					}
				}
			}
			
			
			
			bool result = StructureHelper.Generator.GenerateStructure(structurePath, new Point16(X:x, Y:y), mod);
		}
	}
}