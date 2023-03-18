using SFML.Graphics;

namespace OpenMc2D.Game.Definitions;

public class Water : Block
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(13, 12);
    public override bool Solid => false;
    public override bool Climbable => true;
    public override float Viscosity => 0.07f;
}

public class Lava : Water
{
    public override Texture Texture => World.TerrainAtlas.AtBlock(14, 12);
    public override float Viscosity => 0.5f;
}