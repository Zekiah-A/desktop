using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class Grass : Item
{
    public new static Texture Texture => Blocks.Grass.Texture;
    public override Type Places => typeof(Blocks.Grass);
}