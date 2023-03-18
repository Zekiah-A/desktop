using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

// https://github.com/open-mc/client/blob/687e4606649c0fb329e3c0b78791ca19fa82cabd/cli/defs.js#L10
public class Grass : Block
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(3, 0);
    public override float BreakTime => 1.5f;
}

public class SnowyGrass : Grass
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(4, 4);
}