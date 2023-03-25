using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions.Entities;

public class Player : Entity
{
    public override float Width => 0.3f;
    public override bool Alive => true;
    public new int Health = 20;

    public Item?[] Inventory = new Item[32];
    public Item?[] Items = new Item?[] { null, null, null, null, null, null };
    public int Selected = 0;
    public Texture? Skin = null;
    public Texture[]? Textures = null;

    public Animation WalkAnimation = new Animation()
        .AddKeyFrame
        (
            0,
            new Dictionary<string, float>()
            {
                { "left_arm_rotation", -45 },
                { "right_arm_rotation", 45 },
                { "left_leg_rotation", 45 },
                { "right_leg_rotation", -45 }
            },
            KeyframeEase.SineInOut
        )
        .AddKeyFrame
        (
            500,
            new Dictionary<string, float>()
            {
                { "left_arm_rotation", 45 },
                { "right_arm_rotation", -45 },
                { "left_leg_rotation", -45 },
                { "right_leg_rotation", 45 }
            },
            KeyframeEase.SineInOut
        );
    public Animation CrouchAnimation = new Animation();
    public Animation PlaceAnimation = new Animation();

    public override void Render(RenderWindow window)
    {
        
    }
}