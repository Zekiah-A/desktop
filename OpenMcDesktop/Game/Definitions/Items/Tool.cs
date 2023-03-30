using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class Tool : Item
{
    public static int MaxStack { get; set; } = 1;
    public static int Model { get; set; } = 1;
    public override int Speed { get; } = 10;
}

public class DiamondPickaxe : Tool
{
    public override Definitions.Tool Tool => Definitions.Tool.Pickaxe;
    public override int Speed => 25;
}

public class DiamondShovel : Tool
{
    public override Definitions.Tool Tool => Definitions.Tool.Shovel;
    public override int Speed => 25;
}

public class DiamondAxe : Tool
{
    public override Definitions.Tool Tool => Definitions.Tool.Axe;
    public override int Speed => 25;
}

public class FlintAndSteel : Tool
{
    public static int Model { get; set; } = 2;
}