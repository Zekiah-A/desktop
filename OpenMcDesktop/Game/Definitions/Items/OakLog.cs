using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class OakLog : Item
{
    public new static Texture Texture => Blocks.OakLog.Texture;
    public override Type Places => typeof(Blocks.OakLog);
}