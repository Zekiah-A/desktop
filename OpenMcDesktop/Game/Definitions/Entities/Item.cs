using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Entities;

public class Item : Entity
{
    public override float Width => 0.125f;
    public override float Height => 0.25f;
    public Texture Texture = new Texture(0, 0);
}