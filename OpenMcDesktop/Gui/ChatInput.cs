using System.Numerics;
using System.Text.RegularExpressions;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace OpenMcDesktop.Gui;

public class ChatInput : TextInput
{
    private string rawText;
    public override string Text
    {
        get => rawText;
        set
        {
            rawText = value;
            UpdateStyledText();
        }
    }
    private List<StyledTextNode> styledTextNodes;

    public ChatInput(string placeholder, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(placeholder, x, y, width, height)
    {
        styledTextNodes = new List<StyledTextNode>();
    }

    // Run tokenisers on text
    private void UpdateStyledText()
    {
        styledTextNodes.Clear();
        var lineIndex = 0;
        while (lineIndex < rawText.Length - 1)
        {
            var nodeCreated = false;

            foreach (var pair in TextHelpers.CommandStyleMap)
            {
                var endIndex = rawText.Length - 1;
                var matchFound = false;
                Match? match = null;
                while (endIndex >= lineIndex)
                {
                    match = pair.Key.Match(rawText[lineIndex..endIndex]);
                    if (match.Success && match.Index == lineIndex)
                    {
                        matchFound  = true;
                        break;
                    }

                    endIndex--;
                }
                if (!matchFound || match == null)
                {
                    continue;
                }

                styledTextNodes.Add(new StyledTextNode(match.Value,
                    TextHelpers.TextColours[pair.Value],
                    TextHelpers.TextShadows[pair.Value],
                    TextHelpers.TextDecorations[0]));
                lineIndex += match.Length;
                nodeCreated = true;
                break;
            }

            if (!nodeCreated)
            {
                styledTextNodes.Add(new StyledTextNode(rawText[lineIndex].ToString(),
                    TextHelpers.TextColours[15],
                    TextHelpers.TextShadows[15],
                    TextHelpers.TextDecorations[0]));
                lineIndex++;
            }
        }
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
            var placeholder = new Text(Placeholder, font)
            {
                CharacterSize = characterSize,
                Position = textPosition,
                FillColor = BorderColour
            };
            window.Draw(placeholder);
        }

        var lastNodeEnd = 0f;
        foreach (var node in styledTextNodes)
        {
            var shadow = new Text(node.Text, font)
            {
                CharacterSize = characterSize,
                Position = textPosition + new Vector2f(lastNodeEnd + 2, 2),
                FillColor = node.Shadow
            };
            window.Draw(shadow);

            var text = new Text(node.Text, font)
            {
                CharacterSize = characterSize,
                Position = textPosition + new Vector2f(lastNodeEnd, 0),
                FillColor = node.Colour
            };
            window.Draw(text);

            lastNodeEnd += text.GetGlobalBounds().Width + 3;
        }

        window.SetMouseCursor(State is State.Hover or State.Pressed ? SfmlHelpers.TextCursor : SfmlHelpers.DefaultCursor);
        if (Focused)
        {
            var cursorRect = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX() + lastNodeEnd + 12, Bounds.EndY() - 12),
                Size = new Vector2f(24, 4),
                OutlineColor = BorderColour,
                FillColor = BorderColour
            };
            window.Draw(cursorRect);
        }
    }
}
