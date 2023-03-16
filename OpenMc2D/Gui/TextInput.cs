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

    public override bool KeyboardTest(Keyboard.Key key, int modifiers, TestType type)
    {
        if (!Focused || type != TestType.KeyDown)
        {
            return false;
        }
        
        switch (key)
        {
            case Keyboard.Key.Backspace when Text.Length > 0:
                Text = Text[..^1];
                break;
            case Keyboard.Key.Backspace or Keyboard.Key.LShift or Keyboard.Key.RShift or Keyboard.Key.Enter
                or Keyboard.Key.LControl or Keyboard.Key.RControl or Keyboard.Key.LSystem or Keyboard.Key.RSystem
                or Keyboard.Key.Delete or Keyboard.Key.Menu or Keyboard.Key.Home:
                break;
            default:
                Text += key switch
                {
                    Keyboard.Key.Semicolon => ";",
                    Keyboard.Key.Backslash => "\\",
                    Keyboard.Key.Comma => ",",
                    Keyboard.Key.Divide => "/",
                    Keyboard.Key.Equal => "=",
                    Keyboard.Key.Hyphen => "-",
                    Keyboard.Key.Quote => "\"",
                    Keyboard.Key.RBracket => ")",
                    Keyboard.Key.LBracket => "(",
                    Keyboard.Key.Space => " ",
                    _ => (modifiers & (int) ModifierFlags.Shift) == (int) ModifierFlags.Shift
                        ? key.ToString()
                        : key.ToString().ToLower()
                };
                break;
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

        var text = new Text(string.IsNullOrEmpty(Text) ? Placeholder : Text, font);
        text.CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f);
        text.Position = new Vector2f(Bounds.StartX() + 8, Bounds.StartY() + 2);
        text.FillColor = string.IsNullOrEmpty(Text) ? BorderColour : Color.White;
        window.Draw(text);
        
        if (Focused)
        {
            var cursorRect = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
                Size = new Vector2f(8, 4),
                OutlineColor = BorderColour
            };
            cursorRect.FillColor = BorderColour;
            window.Draw(cursorRect);
        }
    }
}