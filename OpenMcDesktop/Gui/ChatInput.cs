using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace OpenMcDesktop.Gui;

public class ChatInput : TextInput
{
    public string Suggestion;

    public ChatInput(string placeholder, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(placeholder, x, y, width, height)
    {
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var backgroundRect = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
            FillColor = new Color(0, 0, 0, 64),
        };
        window.Draw(backgroundRect);

        var characterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f);
        var textPosition = new Vector2f(Bounds.StartX() + 8, Bounds.StartY() + 2);

        if (string.IsNullOrEmpty(Text))
        {
            var suggestion = new Text(Placeholder, font)
            {
                CharacterSize = characterSize,
                Position = textPosition,
                FillColor = BorderColour,
            };
            window.Draw(suggestion);
        }

        var text = new Text(string.IsNullOrEmpty(Text) && !Focused ? Placeholder : Text, font)
        {
            CharacterSize = characterSize,
            Position = textPosition,
            FillColor = string.IsNullOrEmpty(Text) ? BorderColour : Color.White
        };
        window.Draw(text);

        window.SetMouseCursor(State is State.Hover or State.Pressed ? SfmlHelpers.TextCursor : SfmlHelpers.DefaultCursor);

        if (Focused)
        {
            var cursorRect = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX() + text.GetGlobalBounds().Width + 12, Bounds.EndY() - 12),
                Size = new Vector2f(24, 4),
                OutlineColor = BorderColour,
                FillColor = BorderColour
            };
            window.Draw(cursorRect);
        }
    }
}