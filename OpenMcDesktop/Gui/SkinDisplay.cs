using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class SkinDisplay : Control
{
    public DecodedSkin Skin;
    public Color BorderColour { get; set; } = new(255, 255, 255, 128);
    public Layers Layer = Layers.All;
    public bool Animate;
    private float frame;
    
    [Flags]
    public enum Layers
    {
        ArmBack = 1 << 1,
        Body = 1 << 2,
        ArmFront = 1 << 3,
        LegBack = 1 << 4,
        LegFront = 1 << 5,
        All = ArmBack | Body | ArmFront | LegBack | LegFront
    }

    public SkinDisplay(byte[] data, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Skin = SkinHelpers.DecodeSkin(data);
    }

    public override void Render(RenderWindow window, View view)
    {
        if (State == State.Hover)
        {
            var border = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
                Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
                OutlineColor = BorderColour,
                OutlineThickness = 4,
                FillColor = Color.Transparent
            };
            window.Draw(border);
        }

        const float virtualWidth = 16.0f;
        const float virtualHeight = 32.0f;
        var topLeft = new Vector2f(Bounds.StartX(), Bounds.StartY());
        var limbSize = new Vector2f(4 / virtualWidth * (Bounds.EndX() - Bounds.StartX()),
            12 / virtualHeight * (Bounds.EndY() - Bounds.StartY()));
        var frontRotation = (float) Math.Sin(frame) * 30f;
        var backRotation = (float) Math.Sin(frame + (float) Math.PI) * 30f;
        
        var legsPosition = topLeft + new Vector2f(6 / virtualWidth * (Bounds.EndX() - Bounds.StartX()) + (limbSize.X / 2),
            20 / virtualHeight * (Bounds.EndY() - Bounds.StartY()));
        var armsBodyPosition = topLeft + new Vector2f(
            6 / virtualWidth * (Bounds.EndX() - Bounds.StartX()) + (limbSize.X / 2),
            8 / virtualHeight * (Bounds.EndY() - Bounds.StartY()));

        if ((Layer & Layers.LegBack) == Layers.LegBack)
        {
            var legsBackRect = new RectangleShape
            {
                Position = legsPosition,
                Size = limbSize,
                Texture = new Texture(Skin.LegBack)
            }; 
            legsBackRect.Origin = new Vector2f(limbSize.X / 2, 0);
            legsBackRect.Rotation = backRotation;
            window.Draw(legsBackRect);
        }
        if ((Layer & Layers.ArmBack) == Layers.ArmBack)
        {
            var armBackRect = new RectangleShape
            {
                Position = armsBodyPosition,
                Size = limbSize,
                Texture = new Texture(Skin.ArmBack)
            };
            armBackRect.Origin = new Vector2f(limbSize.X / 2, 0);
            armBackRect.Rotation = backRotation;
            window.Draw(armBackRect);
        }
        if ((Layer & Layers.LegFront) == Layers.LegFront)
        {
            var legsFrontRect = new RectangleShape
            {
                Position = legsPosition,
                Size = limbSize,
                Texture = new Texture(Skin.LegFront)
            };
            legsFrontRect.Origin = new Vector2f(limbSize.X / 2, 0);
            legsFrontRect.Rotation = frontRotation;
            window.Draw(legsFrontRect);
        }
        if ((Layer & Layers.Body) == Layers.Body)
        {
            var bodyRect = new RectangleShape
            {
                Position = armsBodyPosition,
                Size = limbSize,
                Texture = new Texture(Skin.Body)
            };
            bodyRect.Origin = new Vector2f(limbSize.X / 2, 0);
            window.Draw(bodyRect);
        }
        if ((Layer & Layers.ArmFront) == Layers.ArmFront)
        {
            var armFrontRect = new RectangleShape
            {
                Position = armsBodyPosition,
                Size = limbSize,
                Texture = new Texture(Skin.ArmFront)
            };
            armFrontRect.Origin = new Vector2f(limbSize.X / 2, 0);
            armFrontRect.Rotation = frontRotation;
            window.Draw(armFrontRect);
        }
        
        var headRect = new RectangleShape
        {
            Position = topLeft + new Vector2f(4 / virtualWidth * (Bounds.EndX() - Bounds.StartX()), 0),
            Size = new Vector2f(8 / virtualWidth * (Bounds.EndX() - Bounds.StartX()),
                8  / virtualHeight * (Bounds.EndY() - Bounds.StartY())),
            Texture = new Texture(Skin.Head)
        };
        window.Draw(headRect);

        frame = Animate ? frame += 0.05f : 0;
    }
}