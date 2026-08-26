using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SpawnHouses.Common.NPCs;

[AutoloadHead]
internal class Builder : ModNPC {
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 25;

        // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
        NPCID.Sets.ExtraFramesCount[Type] = 10;
        NPCID.Sets.AttackFrameCount[Type] = 4;
        // The amount of pixels away from the center of the npc that it tries to attack enemies.
        NPCID.Sets.DangerDetectRange[Type] = 4 * 16;
        NPCID.Sets.AttackType[Type] = 3;
        // The amount of time it takes for the NPC's attack animation to be over once it starts.
        NPCID.Sets.AttackTime[Type] = 20;
        NPCID.Sets.AttackAverageChance[Type] = 11;
        // For when a party is active, the party hat spawns at a Y offset.
        NPCID.Sets.HatOffsetY[Type] = -7;

        // Influences how the NPC looks in the Bestiary
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() {
            Velocity = 1f,
            Direction = 1
        };

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        NPC.Happiness
            .SetBiomeAffection<SnowBiome>(AffectionLevel.Like)
            .SetBiomeAffection<ForestBiome>(AffectionLevel.Love)
            .SetBiomeAffection<DesertBiome>(AffectionLevel.Hate)
            .SetBiomeAffection<OceanBiome>(AffectionLevel.Like)
            .SetNPCAffection(NPCID.Painter, AffectionLevel.Love)
            .SetNPCAffection(NPCID.Mechanic, AffectionLevel.Like)
            .SetNPCAffection(NPCID.Cyborg, AffectionLevel.Dislike)
            .SetNPCAffection(NPCID.Wizard, AffectionLevel.Hate);
    }

    public override void SetDefaults() {
        NPC.townNPC = true; // Sets NPC to be a Town NPC
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 24;
        NPC.height = 40;
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit41 with { Pitch = -0.61f, PitchVariance = 0.49f };
        NPC.DeathSound = SoundID.NPCDeath44 with { Pitch = 0.38f };
        NPC.knockBackResist = 0.5f;

        AnimationType = NPCID.Guide;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            // Sets the preferred biomes of this town NPC listed in the bestiary.
            // With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

            // You can add multiple elements if you really wanted to
            // You can also use localization keys (see Localization/en-US.lang)
            new FlavorTextBestiaryInfoElement("Mods.SpawnHouses.Bestiary.Builder")
        });
    }

    public override void HitEffect(NPC.HitInfo hit) {
        int num = NPC.life > 0 ? 1 : 5;

        for (int k = 0; k < num; k++)
            Dust.NewDust(NPC.position, NPC.width, NPC.height, Main.rand.Next([DustID.Stone, DustID.Iron, DustID.WoodFurniture]));
    }

    public override bool CanTownNPCSpawn(int numTownNpcs) => numTownNpcs >= 1;

    public override ITownNPCProfile TownNPCProfile() => new BuilderProfile();

    public override List<string> SetNPCNameList() => ["Bob"];

    public override string GetChat() => Language.GetTextValue("Mods.SpawnHouses.NPCs.Builder.Dialogue.Greeting");

    public override void SetChatButtons(ref string button, ref string button2) {
        button = Language.GetTextValue("Mods.SpawnHouses.NPCs.Builder.Dialogue.HelpButton");
        button2 = Language.GetTextValue("Mods.SpawnHouses.NPCs.Builder.Dialogue.StructuresButton");
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
        if (firstButton)
            Main.npcChatText = Language.GetTextValue("Mods.SpawnHouses.NPCs.Builder.Dialogue.HelpChat");
        //Main.npcChatCornerItem = HelpOptionID.GetHelpItem(option);
    }

    public override bool CanGoToStatue(bool toKingStatue) => true;

    public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
        damage = 20;
        knockback = 4f;
    }

    public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
        cooldown = 30;
        randExtraCooldown = 30;
    }

    public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) {
        itemWidth = 48;
        itemHeight = 48;
    }
}

public class BuilderProfile : ITownNPCProfile {
    public int RollVariation() => 0;
    public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

    public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => ModContent.Request<Texture2D>("SpawnHouses/Common/NPCs/Builder");

    public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("SpawnHouses/Common/NPCs/Builder_Head");
}