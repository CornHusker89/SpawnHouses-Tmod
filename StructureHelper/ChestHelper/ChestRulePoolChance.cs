﻿using Terraria;
using Terraria.ModLoader.IO;

namespace SpawnHouses.StructureHelper.ChestHelper;

internal class ChestRulePoolChance : ChestRule {
	/// <summary>
	///     the chance for this item pool to generate at all.
	/// </summary>
	public float chance;

	/// <summary>
	///     How many items from the pool, picked at random, should be placed in the chest.
	/// </summary>
	public int itemsToGenerate;

    public override bool UsesWeight => true;

    public override string Name => "Chance + Pool Rule";

    public override string Tooltip =>
        "Has a configurable chance to generate a \nconfigurable amount of items randomly \nselected from the rule. \nCan make use of weight.";

    public override void PlaceItems(Chest chest, ref int nextIndex) {
        if (nextIndex >= 40)
            return;

        if (Terraria.WorldGen.genRand.NextFloat() <= chance) {
            var toLoot = pool;

            for (var k = 0; k < itemsToGenerate; k++) {
                if (nextIndex >= 40)
                    return;

                var maxWeight = 1;

                foreach (var loot in toLoot)
                    maxWeight += loot.weight;

                var selection = Main.rand.Next(maxWeight);
                var weightTotal = 0;
                Loot selectedLoot = null;

                for (var i = 0; i < toLoot.Count; i++) {
                    weightTotal += toLoot[i].weight;

                    if (selection < weightTotal + 1) {
                        selectedLoot = toLoot[i];
                        toLoot.Remove(selectedLoot);
                        break;
                    }
                }

                chest.item[nextIndex] = selectedLoot?.GetLoot();
                nextIndex++;
            }
        }
    }

    public override TagCompound Serizlize() {
        var tag = new TagCompound {
            { "Type", "PoolChance" },
            { "Chance", chance },
            { "ToGenerate", itemsToGenerate },
            { "Pool", SerializePool() }
        };

        return tag;
    }

    public new static ChestRule Deserialize(TagCompound tag) {
        var rule = new ChestRulePoolChance {
            itemsToGenerate = tag.GetInt("ToGenerate"),
            chance = tag.GetFloat("Chance"),
            pool = DeserializePool(tag.GetCompound("Pool"))
        };

        return rule;
    }

    public override ChestRule Clone() {
        var clone = new ChestRulePoolChance();

        for (var k = 0; k < pool.Count; k++)
            clone.pool.Add(pool[k].Clone());

        clone.itemsToGenerate = itemsToGenerate;
        clone.chance = chance;

        return clone;
    }
}