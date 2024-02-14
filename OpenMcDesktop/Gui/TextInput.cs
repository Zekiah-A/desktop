using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace OpenMcDesktop.Gui;

public class TextInput : Control
{
    public virtual string Text { get; set; } = "";
    public string Placeholder { get; set; }
    public Color BorderColour { get; set; } = new(255, 255, 255, 128);
    public TextSelection? Selection;
    public int Cursor = 0;

    public EventHandler<EventArgs>? OnSubmit;

    private Texture guiTexture;
    private IntRect normalCrop;
    protected static Font font;

    static TextInput()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public TextInput(string placeholder, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Placeholder = placeholder;
        guiTexture = new Texture(@"Resources/Textures/gui.png");
        normalCrop = new IntRect(0, 66, 200, 20);
    }

    public override bool KeyboardTest(Keyboard.Key key, KeyModifiers modifiers, TestType type)
    {
        if (type != TestType.KeyDown)
        {
            return false;
        }

        switch (key)
        {
            case Keyboard.Key.Enter:
                OnSubmit?.Invoke(this, EventArgs.Empty);
                return true;
            case Keyboard.Key.Backspace when Text.Length > 0:
                if (Selection != null)
                {
                    Text = Text.Remove(Selection.Start, Selection.End - Selection.Start);
                    Selection = null;
                }
                else
                {
                    Text = Text[..^1];
                }
                return true;
            case Keyboard.Key.A when modifiers.HasFlag(KeyModifiers.Control):
                Selection = new TextSelection(0, Text.Length);
                return true;
            default:
                return false;
        }
    }

    public override bool TextTest(string unicode)
    {
        // Ignore all control characters, will be handled by keyboard test instead
        if (!Focused || unicode[0] < 32 || (int) unicode[0] is > 126 and < 160)
        {
            return false;
        }

        Selection = null;
        Text += unicode;
        return true;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
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

        var textEndX = 0f;
        if (Selection == null)
        {
            var text = new Text(string.IsNullOrEmpty(Text) && !Focused ? Placeholder : Text, font)
            {
                CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f),
                Position = new Vector2f(Bounds.StartX() + 8, Bounds.StartY() + 2),
                FillColor = string.IsNullOrEmpty(Text) ? BorderColour : Color.White
            };
            window.Draw(text);
            textEndX = text.GetGlobalBounds().Width;
        }
        else
        {
            var preText = new Text(Text[..Selection.Start], font)
            {
                CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f),
                Position = new Vector2f(Bounds.StartX() + 8, Bounds.StartY() + 2),
                FillColor = Color.White
            };
            window.Draw(preText);
            textEndX += preText.GetGlobalBounds().Width;

            var selectionText = new Text(Text[Selection.Start..Selection.End], font)
            {
                CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f),
                Position = new Vector2f(Bounds.StartX() + textEndX + 8, Bounds.StartY() + 2),
                FillColor = TextHelpers.TextColours[4]
            };
            textEndX += selectionText.GetGlobalBounds().Width;

            var selectionRect = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX() + 8, Bounds.StartY() + 12),
                Size = new Vector2f(selectionText.GetGlobalBounds().Width, Bounds.EndY() - Bounds.StartY() - 24),
                FillColor = new Color(255, 255, 255, 221),
            };
            window.Draw(selectionRect);
            window.Draw(selectionText);

            var postText = new Text(Text[Selection.End..], font)
            {
                CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f),
                Position = new Vector2f(Bounds.StartX() + textEndX + 8, Bounds.StartY() + 2),
                FillColor = Color.White
            };
            window.Draw(postText);
            textEndX += postText.GetGlobalBounds().Width;
        }


        window.SetMouseCursor(State is State.Hover or State.Pressed ? SfmlHelpers.TextCursor : SfmlHelpers.DefaultCursor);

        if (Focused)
        {
            var cursorRect = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX() + textEndX + 12, Bounds.EndY() - 12),
                Size = new Vector2f(24, 4),
                OutlineColor = BorderColour,
                FillColor = BorderColour
            };
            window.Draw(cursorRect);
        }
    }
}
