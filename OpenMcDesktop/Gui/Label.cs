using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class Label : Control
{
    public string Text
    {
        get => text.DisplayedString;
        set
        {
            text.DisplayedString = value;
            shadow.DisplayedString = value;
        }
    }

    public int FontSize
    {
        get => (int) text.CharacterSize;
        set
        {
            text.CharacterSize = (uint) value;
            shadow.CharacterSize = (uint) value;
        }
    }

    public LabelAccent Accent { get; set; }

    private readonly Text text;
    private readonly Text shadow;
    private static readonly Font font;

    static Label()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public Label(string content, int fontSize, LabelAccent accent)
    {
        text = new Text(content, font);
        shadow = new Text(content, font);
        FontSize = fontSize;
        Accent = accent;
    }

    public Label(string content, int fontSize, Color colour, Func<int> x, Func<int> y)
    {
        text = new Text(content, font);
        shadow = new Text(content, font);
        FontSize = fontSize;
        Accent = LabelAccent.Default;
        Bounds.StartX = x;
        Bounds.StartY = y;
    }

    public float GetWidth()
    {
        return text.GetGlobalBounds().Width;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var position = new Vector2f(Bounds.StartX(), Bounds.StartY());
        var style = Accent switch
        {
            LabelAccent.Notice => SFML.Graphics.Text.Styles.Italic,
            _ => SFML.Graphics.Text.Styles.Regular
        };
        shadow.FillColor = Accent switch
        {
            LabelAccent.Default => new Color(108, 108, 108),
            LabelAccent.Warn => new Color(136, 136, 136),
            LabelAccent.Error => new Color(195, 142, 126),
            LabelAccent.Notice => new Color(189, 185, 167),
            _ => Color.Black
        };
        shadow.Style = style;
        shadow.Position = position + new Vector2f(2, 2);
        window.Draw(shadow);
        text.FillColor = Accent switch
        {
            LabelAccent.Default => Color.White,
            LabelAccent.Warn => Color.Yellow,
            LabelAccent.Error => Color.Red,
            LabelAccent.Notice => new Color(229, 255, 126),
            _ => Color.Black
        };
        text.Style = style;
        text.Position = position;
        window.Draw(text);
    }
}
