using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Blocks;

public class Tnt : Block
{
    public override float BreakTime => 0.0f;
    public new static Texture Texture => World.TerrainAtlas.AtBlock(8, 0);
    public override Texture InstanceTexture { get; }
    public Tnt() { InstanceTexture = Texture; }
}