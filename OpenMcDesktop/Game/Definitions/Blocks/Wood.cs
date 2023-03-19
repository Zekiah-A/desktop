using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Blocks;

// https://github.com/open-mc/client/blob/687e4606649c0fb329e3c0b78791ca19fa82cabd/cli/defs.js#L40
public abstract class Wood : Block
{
    public override Tool Tool => Tool.Axe;
    public override float BreakTime => 3.0f;
}

public class OakLog : Wood
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(4, 0);
}

public abstract class Planks : Wood
{
    public override float BreakTime { get; }
}

public class OakPlanks : Planks
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(4, 0);
}

public class BirchPlanks : Planks
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(4, 0);
}

public class SprucePlanks : Planks
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(4, 0);
}

public class DarkOakPlanks : Planks
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(4, 0);
}

public class AcaciaPlanks : Planks
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(4, 0);
}

public class JunglePlanks : Planks
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(4, 0);
}