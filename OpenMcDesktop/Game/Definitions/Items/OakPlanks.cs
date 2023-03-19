using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

public class OakPlanks : Item
{
    public new static Texture Texture => Blocks.OakLog.Texture;
    public override Type Places => typeof(Blocks.OakPlanks);
}