using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

// TODO: Annoying inconsistency in the naming scheme, must wait for the server and main client to update their
// TODO: naming before we can change this to something better.
public class SnowBlock : Block
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(2, 4);
    public override float BreakTime => 0.75f;
}