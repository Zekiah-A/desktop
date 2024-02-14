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

        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        var index = 0;
        var insideCommand = Text[0] == '/';

        if (insideCommand)
        {
            styledTextNodes.Add(new StyledTextNode(
                Text[index++].ToString(),
                TextHelpers.TextColours[7],
                Color.Black,
                SFML.Graphics.Text.Styles.Bold));

            while (true)
            {
                if (Text.Length > index)
                {
                    if (char.IsWhiteSpace(Text[index]))
                    {
                        break;
                    }

                    styledTextNodes.Add(new StyledTextNode(
                        Text[index].ToString(),
                        TextHelpers.TextColours[7],
                        Color.Black,
                        SFML.Graphics.Text.Styles.Bold));
                }
                else
                {
                    break;
                }

                index++;
            }
        }
        else
        {
            styledTextNodes.Add(new StyledTextNode(
                Text,
                Color.White,
                Color.Black,
                SFML.Graphics.Text.Styles.Regular));

            return;
        }

        var defaultColor = Color.White;
        var unclosedQuote = false;

        while (Text.Length > index)
        {
            switch (Text[index])
            {
                case '@':
                    if (unclosedQuote)
                    {
                        styledTextNodes.Add(
                            new StyledTextNode(Text[index++].ToString(),
                                unclosedQuote 
                                    ? TextHelpers.TextColours[9] 
                                    : TextHelpers.TextColours[10],
                                Color.Black,
                                SFML.Graphics.Text.Styles.Regular));

                        break;
                    }
                    
                    if (Text.Length > index + 1)
                    {
                        index++;

                        styledTextNodes.Add(
                            new StyledTextNode($"@{Text[index++]}",
                                Color.Green,
                                Color.Black,
                                SFML.Graphics.Text.Styles.Bold));
                    }
                    else
                    {
                        styledTextNodes.Add(
                            new StyledTextNode(Text[index++].ToString(),
                                defaultColor,
                                Color.Black,
                                SFML.Graphics.Text.Styles.Regular));
                    }

                    break;

                case '"':
                {
                    var start = index;

                    index++;

                    var successful = false;

                    while (true)
                    {
                        if (Text.Length > index)
                        {
                            if (Text[index] == '"')
                            {
                                unclosedQuote = false;
                                successful = true;
                                break;
                            }
                        }
                        else
                        {

                            index = start;
                            unclosedQuote = true;

                            styledTextNodes.Add(
                                new StyledTextNode(Text[index++].ToString(),
                                    TextHelpers.TextColours[9],
                                    Color.Black,
                                    SFML.Graphics.Text.Styles.Regular));

                            break;
                        }

                        index++;
                    }

                    if (successful)
                    {
                        var content = Text[start..++index];

                        styledTextNodes.Add(
                            new StyledTextNode(content,
                                TextHelpers.TextColours[13],
                                Color.Black,
                                SFML.Graphics.Text.Styles.Regular));
                    }

                    break;
                }

                case '~':
                {
                    if (unclosedQuote)
                    {
                        styledTextNodes.Add(
                            new StyledTextNode(Text[index++].ToString(),
                                unclosedQuote 
                                    ? TextHelpers.TextColours[9] 
                                    : TextHelpers.TextColours[10],
                                Color.Black,
                                SFML.Graphics.Text.Styles.Regular));

                        break;
                    }

                    styledTextNodes.Add(new StyledTextNode(
                        Text[index++].ToString(),
                        TextHelpers.TextColours[11],
                        Color.Black,
                        SFML.Graphics.Text.Styles.Bold));

                    while (true)
                    {
                        if (Text.Length > index)
                        {
                            if (char.IsDigit(Text[index]))
                            {
                                styledTextNodes.Add(new StyledTextNode(
                                    Text[index++].ToString(),
                                    Color.Yellow,
                                    Color.Black,
                                    SFML.Graphics.Text.Styles.Bold));
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    break;
                }

                case >= '0' and <= '9':
                {
                    if (unclosedQuote)
                    {
                        styledTextNodes.Add(
                            new StyledTextNode(Text[index++].ToString(),
                                unclosedQuote 
                                    ? TextHelpers.TextColours[9] 
                                    : TextHelpers.TextColours[10],
                                Color.Black,
                                SFML.Graphics.Text.Styles.Regular));

                        break;
                    }

                    styledTextNodes.Add(new StyledTextNode(
                        Text[index++].ToString(),
                        Color.Blue,
                        Color.Black,
                        SFML.Graphics.Text.Styles.Bold));

                    while (true)
                    {
                        if (Text.Length > index)
                        {
                            if (char.IsWhiteSpace(Text[index]))
                            {
                                break;
                            }

                            styledTextNodes.Add(new StyledTextNode(
                                Text[index].ToString(),
                                Color.Blue,
                                Color.Black,
                                SFML.Graphics.Text.Styles.Bold));
                        }
                        else
                        {
                            break;
                        }

                        index++;
                    }

                    break;
                }

                default:
                    styledTextNodes.Add(
                        new StyledTextNode(Text[index++].ToString(),
                            unclosedQuote 
                                ? TextHelpers.TextColours[9] 
                                : TextHelpers.TextColours[10],
                            Color.Black,
                            SFML.Graphics.Text.Styles.Regular));

                    break;
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
