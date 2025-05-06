using Terraria;
using Terraria.ModLoader.IO;

namespace SpawnHouses.StructureHelper.ChestHelper;

internal class ChestRuleGuaranteed : ChestRule {
    public override string Name => "Guaranteed Rule";

    public override string Tooltip =>
        "Always generates every item in the rule\nItems are generated in the order they appear here";

    public override void PlaceItems(Chest chest, ref int nextIndex) {
        if (nextIndex >= 40)
            return;

        for (int k = 0; k < pool.Count; k++) {
            if (nextIndex >= 40)
                return;

            chest.item[nextIndex] = pool[k].GetLoot();
            nextIndex++;
        }
    }

    public override TagCompound Serizlize() {
        TagCompound tag = new TagCompound {
            { "Type", "Guaranteed" },
            { "Pool", SerializePool() }
        };

        return tag;
    }

    public new static ChestRule Deserialize(TagCompound tag) {
        ChestRuleGuaranteed rule = new ChestRuleGuaranteed {
            pool = DeserializePool(tag.GetCompound("Pool"))
        };

        return rule;
    }

    public override ChestRule Clone() {
        ChestRuleGuaranteed clone = new ChestRuleGuaranteed();

        for (int k = 0; k < pool.Count; k++) clone.pool.Add(pool[k].Clone());

        return clone;
    }
}