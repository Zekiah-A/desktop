using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace OpenMc2D.Gui;

public class TextInput : Control
{
    public string Text { get; set; } = "";
    public string Placeholder { get; set; }
    public Color BorderColour { get; set; } = new(255, 255, 255, 128);

    private Texture guiTexture;
    private IntRect normalCrop;
    private Font font;

    public TextInput(string placeholder, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Placeholder = placeholder;
        guiTexture = new Texture(@"Resources/Textures/gui.png");
        normalCrop = new IntRect(0, 66, 200, 20);
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public override bool TextTest(string unicode)
    {
        if (!Focused)
        {
            return false;
        }

        if (Text.Length == 0 && unicode[0] == (char) 8)
        {
            return true;
        }

        if (unicode[0] == (char) 8)
        {
            Text = Text[..^1];
        }
        else
        {
            Text += unicode;
        }

        return true;
    }

    public override void Render(RenderWindow window, View view)
    {
        var backgroundRect = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
            FillColor = Color.Black,
            OutlineThickness = -4,
            OutlineColor = BorderColour
        };
        window.Draw(backgroundRect);

        var text = new Text(string.IsNullOrEmpty(Text) && !Focused ? Placeholder : Text, font);
        text.CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f);
        text.Position = new Vector2f(Bounds.StartX() + 8, Bounds.StartY() + 2);
        text.FillColor = string.IsNullOrEmpty(Text) ? BorderColour : Color.White;
        window.Draw(text);
        
        if (Focused)
        {
            var cursorRect = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX() + text.GetGlobalBounds().Width + 12, Bounds.EndY() - 12),
                Size = new Vector2f(24, 4),
                OutlineColor = BorderColour
            };
            cursorRect.FillColor = BorderColour;
            window.Draw(cursorRect);
        }
    }
}