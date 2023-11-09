using System.Numerics;
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
    private float mouseX;
    private float mouseY;
    private float headRotation;

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

    public override bool HitTest(int x, int y, TestType type)
    {
        mouseX = x;
        mouseY = y;
        return base.HitTest(x, y, type);;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        const float virtualWidth = 16.0f;
        const float virtualHeight = 32.0f;
        var topLeft = new Vector2f(Bounds.StartX(), Bounds.StartY());
        var limbSize = new Vector2f(4 / virtualWidth * (Bounds.EndX() - Bounds.StartX()),
            12 / virtualHeight * (Bounds.EndY() - Bounds.StartY()));
        var frontRotation = (float) Math.Sin(frame) * 30f;
        var backRotation = (float) Math.Sin(frame + (float) Math.PI) * 30f;
        
        var bodyPosition = topLeft + new Vector2f(
            6 / virtualWidth * (Bounds.EndX() - Bounds.StartX()) + (limbSize.X / 2),
            8 / virtualHeight * (Bounds.EndY() - Bounds.StartY()));
        var legsPosition = topLeft + new Vector2f(6 / virtualWidth * (Bounds.EndX() - Bounds.StartX()) + (limbSize.X / 2),
            20 / virtualHeight * (Bounds.EndY() - Bounds.StartY()));
        var armsBodyPosition = topLeft + new Vector2f(
            6 / virtualWidth * (Bounds.EndX() - Bounds.StartX()) + (limbSize.X / 2),
            8 / virtualHeight * (Bounds.EndY() - Bounds.StartY()) + (limbSize.Y / 6));
        
        var headSize = new Vector2f(8 / virtualWidth * (Bounds.EndX() - Bounds.StartX()),
            8  / virtualHeight * (Bounds.EndY() - Bounds.StartY()));
        var headCentrePosition =
            topLeft + new Vector2f(4 / virtualWidth * (Bounds.EndX() - Bounds.StartX()), 0) + headSize / 2;
        var flip = mouseX < headCentrePosition.X;
        var flipVector = new Vector2f(flip ? -1 : 1, 1);
        var flipTransform = Transform.Identity;
        flipTransform.Scale(flip ? new Vector2f(-1, 1) : new Vector2f(1, 1));
        var flipStates = new RenderStates(flipTransform);
         
        if ((Layer & Layers.LegBack) == Layers.LegBack)
        {
            var legsBackRect = new RectangleShape
            {
                Position = legsPosition.Multiply(flipVector),
                Size = limbSize,
                Texture = new Texture(Skin.LegBack)
            }; 
            legsBackRect.Origin = new Vector2f(limbSize.X / 2, 0);
            legsBackRect.Rotation = backRotation;
            window.Draw(legsBackRect, flipStates);
        }
        if ((Layer & Layers.ArmBack) == Layers.ArmBack)
        {
            var armBackRect = new RectangleShape
            {
                Position = armsBodyPosition.Multiply(flipVector),
                Size = limbSize,
                Texture = new Texture(Skin.ArmBack)
            };
            armBackRect.Origin = new Vector2f(limbSize.X / 2, limbSize.Y / 6);
            armBackRect.Rotation = backRotation;
            window.Draw(armBackRect, flipStates);
        }
        if ((Layer & Layers.LegFront) == Layers.LegFront)
        {
            var legsFrontRect = new RectangleShape
            {
                Position = legsPosition.Multiply(flipVector),
                Size = limbSize,
                Texture = new Texture(Skin.LegFront)
            };
            legsFrontRect.Origin = new Vector2f(limbSize.X / 2, 0);
            legsFrontRect.Rotation = frontRotation;
            window.Draw(legsFrontRect, flipStates);
        }
        if ((Layer & Layers.Body) == Layers.Body)
        {
            var bodyRect = new RectangleShape
            {
                Position = bodyPosition.Multiply(flipVector),
                Size = limbSize,
                Texture = new Texture(Skin.Body)
            };
            bodyRect.Origin = new Vector2f(limbSize.X / 2, 0);
            window.Draw(bodyRect, flipStates);
        }
        if ((Layer & Layers.ArmFront) == Layers.ArmFront)
        {
            var armFrontRect = new RectangleShape
            {
                Position = armsBodyPosition.Multiply(flipVector),
                Size = limbSize,
                Texture = new Texture(Skin.ArmFront)
            };
            armFrontRect.Origin = new Vector2f(limbSize.X / 2, limbSize.Y / 6);
            armFrontRect.Rotation = frontRotation;
            window.Draw(armFrontRect, flipStates);
        }
        
        var headRect = new RectangleShape
        {
            Position = bodyPosition.Multiply(flipVector),
            Origin = new Vector2f(headSize.X / 2, headSize.Y),
            Size = headSize,
            Texture = new Texture(Skin.Head)
        };
        var headAngle = float.Atan2( (mouseY - headCentrePosition.Y), Math.Abs(mouseX - headCentrePosition.X));
        headAngle = MathHelpers.RadToDeg(headAngle);
        headRect.Rotation = MathHelpers.Lerp(headRotation, headAngle, 0.1f);
        headRotation = headRect.Rotation;
        window.Draw(headRect, flipStates);
        
        if (State == State.Hover)
        {
            var border = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
                Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
                OutlineColor = BorderColour,
                OutlineThickness = -4,
                FillColor = Color.Transparent
            };
            window.Draw(border);
        }

        frame = Animate ? frame += 0.05f : 0;
    }
}