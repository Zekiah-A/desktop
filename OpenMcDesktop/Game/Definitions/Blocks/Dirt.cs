using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Blocks;

public class Dirt : Block
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(2, 0);
    public override Texture InstanceTexture { get; }
    public override float BreakTime => 1.0f;
    public override Tool Tool => Tool.Shovel;

    public Dirt() { InstanceTexture = Texture; }
}