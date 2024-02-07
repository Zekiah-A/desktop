using OpenMcDesktop.Game.Definitions;
using Blocks = OpenMcDesktop.Game.Definitions.Blocks;
using Items = OpenMcDesktop.Game.Definitions.Items;
using Entities = OpenMcDesktop.Game.Definitions.Entities;

namespace OpenMcDesktop.Definitions;

public static class Definitions
{
    public static IReadOnlyDictionary<string, Block> BlockDefinitions { get; set; } // All blocks present in the vanilla game pack
    public static IReadOnlyDictionary<string, Item> ItemDefinitions { get; set; } // All items present in the vanilla game pack
    public static IReadOnlyDictionary<string, Entity> EntityDefinitions { get; set; } // All entities present in the vanilla game pack

    static Definitions()
    {
        // TODO: Implement  missing blocks definitions
        // https://github.com/open-mc/client/blob/main/vanilla/blocks.js
        BlockDefinitions = new Dictionary<string, Block>()
        {
            // General
            { "air", new Blocks.Air() },
            { "grass", new Blocks.Grass() },
            { "stone", new Blocks.Stone() },
            /*{ "cobblestone", new Blocks.Cobblestone() },*/
            { "obsidian", new Blocks.Obsidian() },
            /*{ "glowing_obsidian", new Blocks.GlowingObsidian() },
            { "dirt", new Blocks.GlowingObsidian() },*/
            { "dirt", new Blocks.Dirt() },
            /*{ "farmland", new Blocks.Farmland() },
            { "hydrated_farmmland", new Blocks.HydratedFarmland() },*/
            { "bedrock", new Blocks.Bedrock() },
            { "oak_log", new Blocks.OakLog() },
            // Wood
            /*{ "birch_log", new Blocks.BirchLog() },
            { "spruce_log", new Blocks.SpruceLog() },
            { "dark_oak_log", new Blocks.DarkOakLog() },
            { "acacia_log", new Blocks.AcaciaLog() },
            { "jungle_log", new Blocks.JungleLog() },
            { "acacia_log", new Blocks.Acacia() },*/
            // Planks
            { "oak_planks", new Blocks.OakPlanks() },
            { "birch_planks", new Blocks.BirchPlanks() },
            { "spruce_planks", new Blocks.SprucePlanks() },
            { "dark_oak_planks", new Blocks.DarkOakPlanks() },
            { "acacia_planks", new Blocks.AcaciaPlanks() },
            { "jungle_planks", new Blocks.JunglePlanks() },
            // Slabs
            /*{ "oak_planks_slab", new Blocks.OakPlanksSlab() },
            { "oak_planks_upper_slab", new Blocks.OakPlanksUpperSlab() },
            { "birch_planks_slab", new Blocks.BirchPlanksSlab() },
            { "birch_planks_upper_slab", new Blocks.BirchPlanksUpperSlab() },
            { "spruce_planks_slab", new Blocks.SprucePlanksUpperSlab() },
            { "spruce_planks_upper_slab", new Blocks.SprucePlanksLowerSlab() },
            { "dark_oak_planks_slab", new Blocks.DarkOakPlanksSlab() },
            { "dark_oak_planks_upper_slab", new Blocks.DarkOakPlanksUpperSlab() },
            { "acacia_planks_slab", new Blocks.AcaciaPlanksSlab() },
            { "acacia_planks_upper_slab", new Blocks.AcaciaPlanksUpperSlab() },
            { "jungle_planks_slab", new Blocks.JunglePlanksSlab() },
            { "jungle_planks_upper_slab", new Blocks.JunglePlanksUpperSlab() },*/
            // General
            { "sand", new Blocks.Sand() },
            /*{ "glass", new Blocks.Glass() },*/
            { "cut_sandstone", new Blocks.CutSandstone() },
            { "smooth_sandstone", new Blocks.SmoothSandstone() },
            { "chiseled_sandstone", new Blocks.ChiseledSandstone() },
            { "red_sandstone", new Blocks.RedSandstone() },
            { "cut_red_sandstone", new Blocks.CutRedSandstone() },
            { "chiseled_red_sandstone", new Blocks.ChiseledRedSandstone() },
            { "smooth_red_sandstone", new Blocks.SmoothRedSandstone() },
            /*{ "snow_block", new Blocks.Snow() },*/
            { "snowy_grass", new Blocks.SnowyGrass() },
            { "coal_ore", new Blocks.CoalOre() },
            { "iron_ore", new Blocks.IronOre() },
            { "netherrack", new Blocks.Netherrack() },
            { "quartz_ore", new Blocks.QuartzOre() },
            { "tnt", new Blocks.Tnt() },
            /*{ "endstone", new Blocks.Endstone() },*/
            { "chest", new Blocks.Chest() },
            // Wool
            { "white_wool", new Blocks.WhiteWool() },
            { "light_grey_wool", new Blocks.LightGreyWool() },
            { "grey_wool", new Blocks.GreyWool() },
            { "black_wool", new Blocks.BlackWool() },
            { "red_wool", new Blocks.RedWool() },
            { "orange_wool", new Blocks.OrangeWool() },
            { "yellow_wool", new Blocks.YellowWool() },
            { "lime_wool", new Blocks.LimeWool() },
            { "green_wool", new Blocks.GreenWool() },
            { "cyan_wool", new Blocks.CyanWool() },
            { "light_blue_wool", new Blocks.LightBlueWool() },
            { "blue_wool", new Blocks.BlueWool() },
            { "purple_wool", new Blocks.PurpleWool() },
            { "magenta_wool", new Blocks.MagentaWool() },
            { "pink_wool", new Blocks.PinkWool() },
            { "brown_wool", new Blocks.BrownWool() },
            // General
            { "dragon_egg", new Blocks.DragonEgg() },
            { "ice", new Blocks.Ice() },
            { "packed_ice", new Blocks.PackedIce() },
            // Mineral
            /*{ "lapis_block", new Blocks.Lapis() },
            { "coal_block", new Blocks.Coal() },
            { "iron_block", new Blocks.Iron() },
            { "gold_block", new Blocks.Gold() },
            { "emerald_block", new Blocks.Emerald() },
            { "diamond_block", new Blocks.Diamond() },*/
            // General
            /*{ "fire", new Blocks.Fire() },
            { "portal", new Blocks.Portal() },
            { "end_portal", new Blocks.EndPortal() },
            { "end_portal_frame", new Blocks.EndPortalFrame() },
            { "filled_end_portal_frame", new Blocks.FilledEndPortalFrame() },
            { "sugar_cane", new Blocks.SugarCane() },
            { "pumpkin_leaf", new Blocks.PumkinLeaf() },
            { "pumpkin_leaf1", new Blocks.PumkinLeaf1() },
            { "pumpkin_leaf2", new Blocks.PumkinLeaf2() },
            { "pumpkin_leaf3", new Blocks.PumkinLeaf3() },*/
            // Sapling
            /*{ "oak_sapling", new Blocks.OakSapling() },
            { "birch_sapling", new Blocks.BirchSapling() },
            { "spruce_sapling", new Blocks.SpruceSapling() },
            { "dark_oak_sapling", new Blocks.DarkOakSapling() },
            { "acacia_sapling", new Blocks.AcaciaSapling() },
            { "jungle_sapling", new Blocks.JungleSapling() },*/
            // Leaves
            /*{ "oak_leaves", new Blocks.OakLeaves() },
            { "birch_leaves", new Blocks.BirchLeaves() },
            { "spruce_leaves", new Blocks.SpruceLeaves() },
            { "dark_oak_leaves", new Blocks.DarkOakLeaves() },
            { "acacia_leaves", new Blocks.AcaciaLeaves() },
            { "jungle_leaves", new Blocks.JungleLeaves() },
            { "oak_log_leaves", new Blocks.OakLogLeaves() },
            { "birch_log_leaves", new Blocks.BirchLogLeaves() },
            { "spruce_log_leaves", new Blocks.SpruceLogLeaves() },
            { "dark_oak_log_leaves", new Blocks.DarkOakLogLeaves() },
            { "acacia_log_leaves", new Blocks.AcaciaLogLeaves() },
            { "jungle_log_leaves", new Blocks.JungleLogLeaves() },*/
            // General
            /*{ "furnace", new Blocks.Furnace() },
            { "command_block", new Blocks.Command() }*/
        };

        // TODO: Implement  missing items definitions
        // https://github.com/open-mc/client/blob/main/vanilla/items.js
        ItemDefinitions = new Dictionary<string, Item>()
        {
            { "oak_log", new Items.OakLog() },
            /*{ "birch_log", new Items.BirchLog() },
            { "spruce_log", new Items.SpruceLog() },
            { "dark_oak_log", new Items.DarkOakLog() },
            { "acacia_log", new Items.AcaciaLog() },
            { "jungle_log", new Items.JungleLog() },*/
            { "oak_planks", new Items.OakPlanks() },
            /*{ "birch_planks", new Items.BirchPlanks() },
            { "spruce_planks", new Items.SprucePlanks() },
            { "dark_oak_planks", new Items.DarkOakPlanks() },
            { "acacia_planks", new Items.AcaciaPlanks() },
            { "jungle_planks", new Items.JunglePlanks() },
            { "sand", new Items.Sand() },*/
            { "sandstone", new Items.Sandstone() },
            { "stone", new Items.Stone() },
            /*{ "cobblestone", new Items.Cobblestone() },
            { "glass", new Items.Glass() },
            { "bedrock", new Items.Bedrock() },*/
            { "obsidian", new Items.Obsidian() },
            /*{ "glowing_obsidian", new Items.GlowingObsidian() },*/
            { "netherrack", new Items.Netherrack() },
            /*{ "grass", new Items.Grass() },*/
            { "dirt", new Items.Dirt() },
            /*{ "endstone", new Items.Endstone() },
            { "white_wool", new Items.WhiteWool() },
            { "light_grey_wool", new Items.LightGreyWool() },
            { "grey_wool", new Items.GreyWool() },
            { "black_wool", new Items.BlackWool() },
            { "red_wool", new Items.RedWool() },
            { "orange_wool", new Items.OrangeWool() },
            { "yellow_wool", new Items.YellowWool() },
            { "lime_wool", new Items.LimeWool() },
            { "green_wool", new Items.GreenWool() },
            { "cyan_wool", new Items.CyanWool() },
            { "light_blue_wool", new Items.LightBlueWool() },
            { "blue_wool", new Items.BlueWool() },
            { "purple_wool", new Items.PurpleWool() },
            { "magenta_wool", new Items.MagentaWool() },
            { "pink_wool", new Items.PinkWool() },
            { "brown_wool", new Items.BrownWool() },
            { "crafting_table", new Items.CraftingTable() },
            { "furnace", new Items.Furnace() },
            { "chest", new Items.Chest() }*/
        };

        // TODO: Implement  missing entities definitions
        // https://github.com/open-mc/client/blob/main/vanilla/entities.js
        EntityDefinitions = new Dictionary<string, Entity>()
        {
            { "player", new Entities.Player() },
            { "item", new Entities.Item() },
            /*{ "falling_block", new Entities.FallingBlock() },*/
            { "tnt", new Entities.Tnt() },
            { "end_crystal", new Entities.EndCrystal() },
            /*{ "lightning_bolt", new Entities.LightningBolt() },*/
        };
    }
}
