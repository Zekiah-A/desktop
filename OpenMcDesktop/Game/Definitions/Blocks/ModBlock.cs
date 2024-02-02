// Generic block definition used by blocks that have come in from javascript 
using SFML.Graphics;
using System.Collections.Generic;

namespace OpenMcDesktop.Game.Definitions.Blocks;

public class ModBlock : Block
{
    // Texture can not be static because each mod block acts like a "fake type" created ar runtime.
    public new Texture Texture;
    public override Texture InstanceTexture { get; }
    public Dictionary<string, object> Data { get; } // Additional properties that can be attatched to mod block.

    public ModBlock(Texture texture)
    {
        Texture = texture;
        InstanceTexture = Texture;
        Data = new Dictionary<string, object>();
    }
}