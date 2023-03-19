using OpenMcDesktop.Game.Definitions.Blocks;
using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

public class Item
{
    public static Texture Texture => new Texture(0, 0);
    public virtual Type Places => typeof(Block);
    public virtual  Tool Tool => Tool.None;
    public virtual  int Speed => 0;
}