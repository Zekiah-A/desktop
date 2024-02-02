using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class Label : Control
{
    public string Text
    {
        get => text.DisplayedString;
        set => text.DisplayedString = value;
    }

    public int FontSize
    {
        get => (int) text.CharacterSize;
        set => text.CharacterSize = (uint) value;
    }

    public Color Colour
    {
        get => text.FillColor;
        set => text.FillColor = value;
    }

    private readonly Text text;
    private static readonly Font font;

    static Label()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public Label(string content, int fontSize, Color colour)
    {
        text = new Text(content, font);
        FontSize = fontSize;
        Colour = colour;
    }

    public Label(string content, int fontSize, Color colour, Func<int> x, Func<int> y)
    {
        text = new Text(content, font);
        FontSize = fontSize;
        Colour = colour;
        Bounds.StartX = x;
        Bounds.StartY = y;
    }

    public float GetWidth()
    {
        return text.GetGlobalBounds().Width;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        text.Position = new Vector2f(Bounds.StartX(), Bounds.StartY());
        window.Draw(text);
    }
}
