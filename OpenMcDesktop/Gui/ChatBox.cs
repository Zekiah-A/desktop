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
        foreach (var message in Messages.ToList())
        {
            var lineText = new Text(message.Text, font)
            {
                Position = new Vector2f(Bounds.StartX(), Bounds.StartY() + offsetY),
                FillColor = message.Colour,
                Style = message.Decoration
            };

            // Shortcut + also addresses issues with handling first line wrapping
            var messageEndX = lineText.FindCharacterPos((uint) message.Text.Length - 1).X;
            // Simple text wrapping algorithm, aims to break on spaces, otherwise, will break on a character
            var lastCharX = 0f;
            var lastWhiteSpace = -1;
            var lastBreak = 0;
            var lastBreakX = 0f;
            for (var i = 0; i < message.Text.Length; i++)
            {
                if (message.Text[i] == ' ')
                {
                    lastWhiteSpace = i;
                }

                var charX = lineText.FindCharacterPos((uint) i).X;
                if (charX - lastCharX > Bounds.EndX())
                {
                    var breakAt = lastWhiteSpace == -1 ? Math.Max(i - 1, 0) : Math.Min(lastWhiteSpace + 1, message.Text.Length);
                    var messageLine = message.Text[lastBreak..breakAt];
                    var text = new Text(messageLine, font)
                    {
                        Position = new Vector2f(Bounds.StartX(), Bounds.StartY() + offsetY),
                        FillColor = message.Colour,
                        Style = message.Decoration
                    };
                    var shadow = new Text(messageLine, font)
                    {
                        Position = new Vector2f(Bounds.StartX() + 2, Bounds.StartY() + offsetY + 2),
                        FillColor = message.Shadow,
                        Style = message.Decoration
                    };
                    window.Draw(shadow);
                    window.Draw(text);
                    offsetY += text.GetLocalBounds().Height + 8;
                    lastBreak = breakAt;
                    lastBreakX = lineText.FindCharacterPos((uint) breakAt).X;
                    lastCharX = charX;
                }
                // We predict from here to end won't be long enough to overflow, hence it is the last line that must be drawn
                else if (messageEndX - lastBreakX < Bounds.EndX())
                {
                    var messageLine = message.Text[lastBreak..Math.Max(message.Text.Length, 0)];
                    var text = new Text(messageLine, font)
                    {
                        Position = new Vector2f(Bounds.StartX(), Bounds.StartY() + offsetY),
                        FillColor = message.Colour,
                        Style = message.Decoration
                    };
                    var shadow = new Text(messageLine, font)
                    {
                        Position = new Vector2f(Bounds.StartX() + 2, Bounds.StartY() + offsetY + 2),
                        FillColor = message.Shadow,
                        Style = message.Decoration
                    };
                    window.Draw(shadow);
                    window.Draw(text);
                    offsetY += text.GetLocalBounds().Height + 8;
                    break;
                }
            }
        }
    }
}
