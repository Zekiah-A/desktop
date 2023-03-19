using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class Obsidian : Item
{
    public new static Texture Texture => Blocks.Obsidian.Texture;
    public override Type Places => typeof(Blocks.Obsidian);
}