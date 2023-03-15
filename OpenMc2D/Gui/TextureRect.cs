using SFML.Graphics;
using SFML.System;

namespace OpenMc2D.Gui;

public class TextureRect : Control
{
    public Texture Texture;
    public Bounds SubRect;

    public TextureRect(Texture texture, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Texture = texture;
        SubRect = Bounds.Default;
    }

    public override void Render(RenderWindow window, View view)
    {
        var rect = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
            Texture = Texture
        };

        if (SubRect != Bounds.Default)
        {
            rect.TextureRect = new IntRect(SubRect.StartX(), SubRect.StartY(), SubRect.EndX(), SubRect.EndY());
        }

        window.Draw(rect);
    }
}