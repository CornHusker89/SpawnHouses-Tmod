using Terraria;
using Terraria.ModLoader.IO;

namespace SpawnHouses.StructureHelper.ChestHelper;

internal class ChestRuleChance : ChestRule {
    /// <summary>
    ///     the chance for any individual item from this pool to generate, from 0 to 1. (0 = 0%, 1 = 100%) If you want to
    ///     generate X items from the pool, use ChestRulePool instead. If you want multiple different chances, add another rule
    ///     of this type.
    /// </summary>
    public float chance;

    public override string Name => "Chance Rule";

    public override string Tooltip =>
        "Attempts to generate all items in the rule, \nwith a configurable chance to generate each.\nItems are attempted in the order they appear here";

    public override void PlaceItems(Chest chest, ref int nextIndex) {
        if (nextIndex >= 40)
            return;

        for (int k = 0; k < pool.Count; k++)
            if (Terraria.WorldGen.genRand.NextFloat(1) <= chance) {
                chest.item[nextIndex] = pool[k].GetLoot();
                nextIndex++;
            }
    }

    public override TagCompound Serizlize() {
        TagCompound tag = new TagCompound {
            { "Type", "Chance" },
            { "Chance", chance },
            { "Pool", SerializePool() }
        };

        return tag;
    }

    public new static ChestRule Deserialize(TagCompound tag) {
        ChestRuleChance rule = new ChestRuleChance {
            chance = tag.GetFloat("Chance"),
            pool = DeserializePool(tag.GetCompound("Pool"))
        };

        return rule;
    }

    public override ChestRule Clone() {
        ChestRuleChance clone = new ChestRuleChance();

        for (int k = 0; k < pool.Count; k++) clone.pool.Add(pool[k].Clone());

        clone.chance = chance;

        return clone;
    }
}