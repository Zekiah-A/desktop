using OpenMcDesktop.Game.Definitions.Blocks;
using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Items;

// TODO: In theory, like how Blocks.Sandstone inherits from Blocks.Stone, Items.Sandstone should inherit from Items.Stone
// TODO: But for some reason, main client/server does not do this, this should probably be fixed.
public class Sandstone : Item
{
    public new static Texture Texture => Blocks.Sandstone.Texture;
    public override Type Places => typeof(Blocks.Sandstone);
}

public class CutSandstone : Sandstone { }
public class SmoothSandstone : Sandstone { }
public class ChiseledSandstone : Sandstone { }
public class RedSandstone : Sandstone { }
public class CutRedSandstone : Sandstone { }
public class ChiseledRedSandstone : Sandstone { }
public class SmoothRedSandstone : Sandstone { }