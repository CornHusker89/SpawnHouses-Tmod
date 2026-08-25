// using System;
// using System.Collections.Generic;
// using MagicStorage;
// using MagicStorage.Common.Systems;
// using MagicStorage.Items;
// using MagicStorage.NPCs;
// using MagicStorage.Stations;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using ReLogic.Content;
// using Terraria;
// using Terraria.GameContent;
// using Terraria.GameContent.Bestiary;
// using Terraria.GameContent.Personalities;
// using Terraria.ID;
// using Terraria.Localization;
// using Terraria.ModLoader;
// using Terraria.Utilities;
//
// namespace SpawnHouses.Common;
//
// [AutoloadHead]
// internal class Builder : ModNPC {
//
// 	public override void SetStaticDefaults() {
// 		Main.npcFrameCount[Type] = 25;
//
// 		// Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
// 		NPCID.Sets.ExtraFramesCount[Type] = 10;
// 		NPCID.Sets.AttackFrameCount[Type] = 4;
// 		// The amount of pixels away from the center of the npc that it tries to attack enemies.
// 		NPCID.Sets.DangerDetectRange[Type] = 4 * 16;
// 		NPCID.Sets.AttackType[Type] = 3;
// 		// The amount of time it takes for the NPC's attack animation to be over once it starts.
// 		NPCID.Sets.AttackTime[Type] = 20;
// 		NPCID.Sets.AttackAverageChance[Type] = 11;
// 		// For when a party is active, the party hat spawns at a Y offset.
// 		NPCID.Sets.HatOffsetY[Type] = -7;
//
// 		// Influences how the NPC looks in the Bestiary
// 		NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() {
// 			Velocity = 1f,
// 			Direction = 1
// 		};
//
// 		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
//
// 		NPC.Happiness
// 			.SetBiomeAffection<SnowBiome>(AffectionLevel.Like)
// 			.SetBiomeAffection<ForestBiome>(AffectionLevel.Love)
// 			.SetBiomeAffection<DesertBiome>(AffectionLevel.Hate)
// 			.SetBiomeAffection<OceanBiome>(AffectionLevel.Like)
// 			.SetNPCAffection(NPCID.Painter, AffectionLevel.Love)
// 			.SetNPCAffection(NPCID.Mechanic, AffectionLevel.Like)
// 			.SetNPCAffection(NPCID.Cyborg, AffectionLevel.Dislike)
// 			.SetNPCAffection(NPCID.Wizard, AffectionLevel.Hate);
// 	}
//
// 	public override void SetDefaults() {
// 		NPC.townNPC = true; // Sets NPC to be a Town NPC
// 		NPC.friendly = true; // NPC Will not attack player
// 		NPC.width = 24;
// 		NPC.height = 40;
// 		NPC.aiStyle = 7;
// 		NPC.damage = 10;
// 		NPC.defense = 15;
// 		NPC.lifeMax = 250;
// 		NPC.HitSound = SoundID.NPCHit41 with { Pitch = -0.61f, PitchVariance = 0.49f };
// 		NPC.DeathSound = SoundID.NPCDeath44 with { Pitch = 0.38f };
// 		NPC.knockBackResist = 0.5f;
//
// 		AnimationType = NPCID.Guide;
// 	}
//
// 	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
// 		// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
// 		bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
// 			// Sets the preferred biomes of this town NPC listed in the bestiary.
// 			// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
// 			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
//
// 			// You can add multiple elements if you really wanted to
// 			// You can also use localization keys (see Localization/en-US.lang)
// 			new FlavorTextBestiaryInfoElement("Mods.SpawnHouses.Bestiary.Builder")
// 		});
// 	}
//
// 	public override void PostAI() {
// 		if (Main.dedServ)
// 			return;
// 	}
//
// 	public override void HitEffect(NPC.HitInfo hit) {
// 		int num = NPC.life > 0 ? 1 : 5;
//
// 		for (int k = 0; k < num; k++)
// 			Dust.NewDust(NPC.position, NPC.width, NPC.height, Main.rand.Next([DustID.Stone, DustID.Iron, DustID.WoodFurniture]));
// 	}
//
// 	public override bool CanTownNPCSpawn(int numTownNPCs) => numTownNPCs >= 1;
//
// 	public override ITownNPCProfile TownNPCProfile() => new BuilderProfile();
//
// 	public override List<string> SetNPCNameList() => ["Bob"];
//
// 	public override string GetChat() => Language.GetTextValue("Mods.SpawnHouses.Dialogue.Builder.Greeting");
//
// 	public override void SetChatButtons(ref string button, ref string button2) {
// 		// Ensure that the current option isn't an unavailable one
// 		if (helpOption > 0)
// 			SkipOverUnavailableTips(forwards: false, ref helpOption);
//
// 		button = helpOption == 0
// 			? Language.GetTextValue("LegacyInterface.51")
// 			: SkipBackwards(helpOption - 1, out int prevOption) && prevOption > 0
// 				? Language.GetTextValue("Mods.MagicStorage.Dialogue.ChatOptions.Golem.PrevHelp")
// 				: "";
//
// 		button2 = helpOption > 0 && SkipForwards(helpOption + 1, out int nextOption) && nextOption > 0
// 			? Language.GetTextValue("Mods.MagicStorage.Dialogue.ChatOptions.Golem.NextHelp")
// 			: "";
// 	}
//
// 	public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
// 		var player = Main.LocalPlayer.GetModPlayer<StoragePlayer>();
//
// 		ref int savedTip = ref player.automatonHelpTip;
//
// 		bool wasHelpOptionUninitialized = false;
// 		if (helpOption == 0) {
// 			if (!MagicStorageConfig.DisplayLastSeenAutomatonTip)
// 				savedTip = 0;
//
// 			helpOption = savedTip;
//
// 			wasHelpOptionUninitialized = true;
// 		}
//
// 		if (!wasHelpOptionUninitialized) {
// 			if (firstButton)
// 				helpOption--;
// 			else
// 				helpOption++;
// 		}
//
// 		SkipOverUnavailableTips(!wasHelpOptionUninitialized && !firstButton, ref helpOption);
//
// 		savedTip = helpOption;
//
// 		int option = GetSelectedHelp();
//
// 		Main.npcChatText = HelpOptionID.GetHelpText(option);
// 		Main.npcChatCornerItem = HelpOptionID.GetHelpItem(option);
// 	}
//
// 	private static void SkipOverUnavailableTips(bool forwards, ref int option) {
// 		// Clamp the option
// 		option = Utils.Clamp(option, 1, HelpOptionID.Count);
//
// 		if (HelpOptionID.IsOptionAvailable(helpOptionsByIndex[option - 1]))
// 			return;  // The current option is available, no need to skip
//
// 		int validIndex;
// 		if (forwards) {
// 			// Scan forwards, then scan backwards if that fails
// 			if (!SkipForwards(option, out validIndex))
// 				SkipBackwards(option,out validIndex);
// 		} else {
// 			// Scan backwards, then scan forwards if that fails
// 			if (!SkipBackwards(option, out validIndex))
// 				SkipForwards(option, out validIndex);
// 		}
//
// 		// There's always going to be at least one option that's available
// 		option = validIndex;
//
// 		if (option < 1)
// 			option = 1;
// 		else if (option > HelpOptionID.Count)
// 			option = HelpOptionID.Count;
// 	}
//
// 	private static bool SkipForwards(int current, out int validOption) {
// 		if (current < 1)
// 			current = 1;
//
// 		for (int i = current; i <= HelpOptionID.Count; i++) {
// 			if (HelpOptionID.IsOptionAvailable(helpOptionsByIndex[i - 1])) {
// 				// Found a valid option
// 				validOption = i;
// 				return true;
// 			}
// 		}
//
// 		// No valid options were found
// 		validOption = 0;
// 		return false;
// 	}
//
// 	private static bool SkipBackwards(int current, out int validOption) {
// 		if (current > HelpOptionID.Count)
// 			current = HelpOptionID.Count;
//
// 		for (int i = current; i >= 1; i--) {
// 			if (HelpOptionID.IsOptionAvailable(helpOptionsByIndex[i - 1])) {
// 				// Found a valid option
// 				validOption = i;
// 				return true;
// 			}
// 		}
//
// 		// No valid options were found
// 		validOption = 0;
// 		return false;
// 	}
//
// 	// Make this Town NPC teleport to the King and/or Queen statue when triggered.
// 	public override bool CanGoToStatue(bool toKingStatue) => true;
//
// 	public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
// 		damage = 20;
// 		knockback = 4f;
// 	}
//
// 	public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
// 		cooldown = 30;
// 		randExtraCooldown = 30;
// 	}
//
// 	public override void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) {
// 		scale = 1f;
//
// 		Main.GetItemDrawFrame(ModContent.ItemType<StorageDeactivator>(), out Texture2D texture, out Rectangle frame);
//
// 		item = texture;
// 		itemFrame = frame;
// 		itemSize = itemFrame.Width;
// 	}
//
// 	public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) {
// 		itemWidth = 48;
// 		itemHeight = 48;
// 	}
//
// 	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
// 		float npcHeight = Main.NPCAddHeight(NPC);
//
// 		Texture2D texture = TextureAssets.Npc[Type].Value;
//
// 		Vector2 halfSize = new(texture.Width / 2, texture.Height / Main.npcFrameCount[Type] / 2);
//
// 		SpriteEffects spriteEffects = SpriteEffects.None;
// 		if (NPC.spriteDirection == 1)
// 			spriteEffects = SpriteEffects.FlipHorizontally;
//
// 		Texture2D glow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
//
// 		spriteBatch.Draw(glow,
// 			new Vector2(NPC.Center.X - glow.Width * NPC.scale / 2f, NPC.Bottom.Y - glow.Height * NPC.scale / Main.npcFrameCount[Type] + 4f + npcHeight + NPC.gfxOffY) - screenPos + halfSize * NPC.scale,
// 			NPC.frame,
// 			Color.White,
// 			NPC.rotation,
// 			halfSize,
// 			NPC.scale,
// 			spriteEffects,
// 			0f);
//
// 		if (newHelpTextAvailable) {
// 			Texture2D exclamation = TextureAssets.Extra[48].Value;
//
// 			Rectangle source = exclamation.Frame(8, 39, newHelpTextAvailableCounter % 60 < 30 ? 6 : 7, 1);
//
// 			Vector2 center = NPC.Top - new Vector2(0, source.Height * 0.75f) - Main.screenPosition;
//
// 			double sin = (Math.Sin(newHelpTextAvailableCounter / 60d * MathHelper.TwoPi * 0.65) + 1) / 2;
//
// 			center.Y += (float)(-5 * Math.Sin(newHelpTextAvailableCounter / 60d * MathHelper.TwoPi * 0.4));
//
// 			float transparency = (float)(0.75 + 0.25 * sin);
//
// 			spriteBatch.Draw(exclamation, center, source, Color.White * transparency, 0, source.Size() / 2f, 1.5f, SpriteEffects.None, 0);
// 		}
// 	}
// }
//
// public class BuilderProfile : ITownNPCProfile {
// 	public int RollVariation() => 0;
// 	public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
//
// 	public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => ModContent.Request<Texture2D>("SpawnHouses/NPCs/Builder");
//
// 	public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("SpawnHouses/NPCs/Builder_Head");
// }
//

