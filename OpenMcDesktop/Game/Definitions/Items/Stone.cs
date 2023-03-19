using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class Stone : Item
{
    public new static Texture Texture => Blocks.Stone.Texture;
    public override Type Places => typeof(Blocks.Stone);
}