using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

// https://github.com/open-mc/client/blob/687e4606649c0fb329e3c0b78791ca19fa82cabd/cli/defs.js#L16
public class Stone : Block
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(1, 0);
    public override float BreakTime => 7.5f;
    public override Tool Tool => Tool.Pickaxe;
}

public class Obsidian : Stone
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(5, 2);
    public override float BreakTime => 250.0f;
}

public class Bedrock : Stone
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(1, 1);
    public override float BreakTime => float.PositiveInfinity;
}

public class CoalOre : Stone
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(2, 2);
}

public class IronOre : Stone
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(1, 2);
}

public class Sandstone : Stone
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(0, 12);
    public override float BreakTime => 4;
}

public class CutSandstone : Sandstone { }
public class SmoothSandstone : Sandstone { }
public class ChiseledSandstone : Sandstone { }
public class RedSandstone : Sandstone { }
public class CutRedSandstone : Sandstone { }
public class ChiseledRedSandstone : Sandstone { }
public class SmoothRedSandstone : Sandstone { }