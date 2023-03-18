using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

public class Netherrack : Block
{
    public override float BreakTime => 2.0f;
    public override Texture Texture => World.TerrainAtlas.AtBlock(7, 6);
    public override Tool Tool => Tool.Pickaxe;
}

public class QuartzOre : Block
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(6, 6);
}