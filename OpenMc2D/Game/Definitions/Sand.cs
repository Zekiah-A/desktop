using SFML.Graphics;

namespace OpenMc2D.Game.Definitions;

// https://github.com/open-mc/client/blob/687e4606649c0fb329e3c0b78791ca19fa82cabd/cli/defs.js#L61
public class Sand : Block
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(2, 1);
}