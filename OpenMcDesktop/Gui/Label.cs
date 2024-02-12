using SFML.Graphics;
using SFML.System;
using System.Globalization;

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

    private readonly Text text;
    private readonly Text shadow;
    private static readonly Font font;

    static Label()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public Label(string content, int fontSize, LabelAccent accent)
    {
        text = new Text(content, font)
        {
            FillColor = Color.White
        };
        shadow = new Text(content, font)
        {
            FillColor = Color.Transparent
        };
        FontSize = fontSize;
        SetAccent(accent);
    }

    public Label(string content, int fontSize, Func<int> x, Func<int> y)
    {
        text = new Text(content, font)
        {
            FillColor = Color.White
        };
        shadow = new Text(content, font)
        {
            FillColor = Color.Transparent
        };
        FontSize = fontSize;
        Bounds.StartX = x;
        Bounds.StartY = y;
    }

    public float GetWidth()
    {
        return text.GetGlobalBounds().Width;
    }

    /// <summary>
    /// Use a pre-defined standard theme label type
    /// </summary
    public void SetAccent(LabelAccent accent)
    {
        text.FillColor = accent switch
        {
            LabelAccent.Default => Color.White,
            LabelAccent.Warn => Color.Yellow,
            LabelAccent.Error => Color.Red,
            LabelAccent.Notice => new Color(229, 255, 126),
            _ => Color.Black
        };
        shadow.FillColor = accent switch
        {
            LabelAccent.Default => new Color(108, 108, 108),
            LabelAccent.Warn => new Color(136, 136, 136),
            LabelAccent.Error => new Color(195, 142, 126),
            LabelAccent.Notice => new Color(189, 185, 167),
            _ => Color.Black
        };
        var style = accent switch
        {
            LabelAccent.Notice => SFML.Graphics.Text.Styles.Italic,
            _ => SFML.Graphics.Text.Styles.Regular
        };
        shadow.Style = style;
        text.Style = style;
    }

    /// <summary>
    /// Use an open-mc format string, such as "1fHello World" to automatically set the correct label colours and styles
    /// </summary
    public void UseFormatString(string format)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(format.Length, 3, "Format string can not have a length of less than 3");
        var negative = format[0] == '-';
        var stringCode = negative ? format[1].ToString() : format[..2];

        if (!int.TryParse(stringCode, NumberStyles.AllowHexSpecifier, null, out var style))
        {
            throw new InvalidOperationException("Specified format string had invalid format code " + format[..2]);
        }
        if (negative)
        {
            // TODO: Normal strings shouldn't have a negative format, handle anyway
            style = -style;
        }

        text.DisplayedString = format[2..];
        shadow.DisplayedString = format[2..];
        text.FillColor = TextHelpers.TextColours[style & 15];
        shadow.FillColor = TextHelpers.TextShadows[style & 15];
        var decoration = TextHelpers.TextDecorations[style >> 4];
        text.Style = decoration;
        shadow.Style = decoration;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var position = new Vector2f(Bounds.StartX(), Bounds.StartY());
        shadow.Position = position + new Vector2f(2, 2);
        window.Draw(shadow);
        text.Position = position;
        window.Draw(text);
    }
}
