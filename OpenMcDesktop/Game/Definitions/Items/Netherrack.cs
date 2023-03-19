using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class Netherrack : Item
{
    public new static Texture Texture => Blocks.Netherrack.Texture;
    public override Type Places => typeof(Blocks.Netherrack);
}