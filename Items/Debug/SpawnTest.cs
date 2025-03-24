using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.Xna.Framework;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Items.Debug
{
	public class SpawnTest : ModItem
	{

		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.rare = ItemRarityID.Blue;
		}

		public override bool AltFunctionUse(Terraria.Player player)
		{
			return true;
		}



		private static void FillComponents(List<Shape> volumes, StructureTag[] tagsRequired, StructureTag[] tagsBlacklist, TilePalette palette)
		{
			if (volumes.Count == 0)
				return;

			ComponentParams componentParams = new ComponentParams(
				tagsRequired,
				tagsBlacklist,
				volumes[0],
				palette
			);
			var genMethod = ComponentGen.GetRandomMethod(componentParams);

			foreach (var volume in volumes)
			{
				componentParams.MainVolume = volume;
				genMethod(componentParams);
			}
		}



		public override bool? UseItem(Terraria.Player player)
		{
			int x = (Main.MouseWorld / 16).ToPoint16().X;
			int y = (Main.MouseWorld / 16).ToPoint16().Y;

			Console.WriteLine(x + ", " + y);

			int length = 30;
			var palette = TilePalette.Palette1;

			var result = RoomLayoutGen.RoomLayout1(new RoomLayoutParams([], [], new Shape(new Point16(x, y), new Point16(x + length, y - length)),
				palette, 11, new Range(4, 18), new Range(7, 24), new Range(1, 2), new Range(1, 2), 0.3f, 1));

			FillComponents(result.FloorVolumes, [StructureTag.HasFloor], [], palette);
			FillComponents(result.WallVolumes, [StructureTag.HasWall], [], palette);
			FillComponents(result.Rooms.Select(room => room.Volume).ToList(), [StructureTag.HasBackground], [], palette);

			WorldUtils.Gen(new Point(x + length / 2, y - length), new Shapes.Circle((length + 50) / 2), Actions.Chain(
				new Actions.SetFrames(),
				new Actions.Custom((i, j, args) =>
				{
					Framing.WallFrame(i, j);
					return true;
				})
			));

			return true;
		}

	}
}