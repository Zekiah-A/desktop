using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Entities;

public class Player : Entity
{
    public override float Width => 0.3f;
    public override bool Alive => true;
    public new int Health = 20;

    public Item?[] Inventory = new Item[32];
    public Item?[] items = new Item?[] { null, null, null, null, null, null };
    public int Selected = 0;
    public byte[]? Skin = null;
    public Texture[]? Textures = null;
}