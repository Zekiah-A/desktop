using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class Dirt : Item
{
    public new static Texture Texture => Blocks.Dirt.Texture;
    public override Type Places => typeof(Blocks.Dirt);
}