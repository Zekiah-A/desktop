using SFML.Graphics;
using SFML.System;

namespace OpenMc2D.Gui;

public class Label : Control
{
    public string Text;
    public int FontSize;
    public Color Colour;

    public Label(string text, int fontSize, Color colour)
    {
        Text = text;
        FontSize = fontSize;
        Colour = colour;
    }

    public Label(string text, int fontSize, Color colour, Func<int> x, Func<int> y)
    {
        Text = text;
        FontSize = fontSize;
        Colour = colour;
        Bounds.StartX = x;
        Bounds.StartY = y;
    }

    public override void Render(RenderWindow window)
    {
        var font = new Font(@"Resources/Fonts/mojangles.ttf");
        var text = new Text(Text, font)
        {
            CharacterSize = (uint) FontSize,
            FillColor = Colour,
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY())
        };
        window.Draw(text);
    }
}