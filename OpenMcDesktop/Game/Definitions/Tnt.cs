using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

public class Tnt : Block
{
    public override float BreakTime => 0.0f;
    public override Texture Texture => World.TerrainAtlas.AtBlock(8, 0);
}