using SFML.Graphics;
using SFML.System;
using OpenMcDesktop.Gui;

namespace OpenMcDesktop.Gui;

public class ChatBox : Control
{
    public readonly List<ChatBoxItem> Messages;
    private static readonly Font font;

    static ChatBox()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public ChatBox(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Messages = new List<ChatBoxItem>();
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var background = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            FillColor = new Color(0, 0, 0, 96),
            Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
        };
        window.Draw(background);

        var offsetY = 0f;
        foreach (var message in Messages)
        {
            var shadow = new Text(message.Text, font)
            {
                Position = new Vector2f(Bounds.StartX() + 2, Bounds.StartY() + offsetY + 2),
                FillColor = message.Shadow,
                Style = message.Decoration
            };
            window.Draw(shadow);
            var text = new Text(message.Text, font)
            {
                Position = new Vector2f(Bounds.StartX(), Bounds.StartY() + offsetY),
                FillColor = message.Colour,
                Style = message.Decoration
            };
            window.Draw(text);
            offsetY += text.GetLocalBounds().Height;
        }
    }
}
