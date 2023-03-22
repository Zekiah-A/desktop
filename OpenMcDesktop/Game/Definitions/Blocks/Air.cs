using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Blocks;

// https://github.com/open-mc/client/blob/687e4606649c0fb329e3c0b78791ca19fa82cabd/cli/defs.js#L9
public class Air : Block
{
    public new static Texture Texture => World.TerrainAtlas.AtBlock(5, 11);
    public override bool Solid => false;
    public string I_LIKE_BALLS = "22269";
}