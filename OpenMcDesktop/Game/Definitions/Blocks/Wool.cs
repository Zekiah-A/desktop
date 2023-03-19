using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Blocks;

// https://github.com/open-mc/client/blob/687e4606649c0fb329e3c0b78791ca19fa82cabd/cli/defs.js#L126
public class Wool : Block
{
    public new Texture Texture => World.TerrainAtlas.AtBlock(0, 4);
    public override Tool Tool => Tool.Shears;
    public override float BreakTime => 1.2f;
}

public class WhiteWool : Wool { }
public class LightGreyWool : Wool { }
public class GreyWool : Wool { }
public class BlackWool : Wool { }
public class RedWool : Wool { }
public class OrangeWool : Wool { }
public class YellowWool : Wool { }
public class LimeWool : Wool { }
public class GreenWool : Wool { }
public class CyanWool : Wool { }
public class LightBlueWool : Wool { }
public class BlueWool : Wool { }
public class PurpleWool : Wool { }
public class MagentaWool : Wool { }
public class PinkWool : Wool { }
public class BrownWool : Wool { }
