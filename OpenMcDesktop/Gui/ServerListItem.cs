using OpenMcDesktop.Networking;
using SFML.Graphics;
using SFML.System;
using SharpWebview;
using SharpWebview.Content;

namespace OpenMcDesktop.Gui;

public class ServerListItem : DisplayListItem
{
    public event EventHandler<EventArgs>? OnDoubleClick;
    public string WebviewWindowName;
    public string WebviewUri;
    public PreConnectData? ConnectData;

    private Button openPageButton;
    private long lastClick;

    private void InvokeDoubleClick(object? sender, EventArgs args) => OnDoubleClick?.Invoke(sender, args);

    public ServerListItem(Texture texture, string name, string description, string pageUri) : base(texture, name, description)
    {
        openPageButton = new Button("?", () => Bounds.EndX() - 64, () => Bounds.StartY(), () => 64, () => 64);
        WebviewWindowName = name;
        WebviewUri = pageUri;

        openPageButton.OnMouseUp += (_, _) =>
        {
            using var webview = new Webview();
            webview.SetTitle($"{WebviewWindowName} - Server statistics")
                .SetSize(1024, 768, WebviewHint.None)
                .SetSize(800, 600, WebviewHint.Min)
                .Navigate(new UrlContent("https://" + WebviewUri + ":27277/"))
                .Run();
        };
    }

    public override bool HitTest(int x, int y, TestType type)
    {
        if (openPageButton.HitTest(x, y, type))
        {
            return true;
        }

        // Self hit test
        if (type == TestType.MouseUp && State == State.Pressed)
        {
            State = State.Default;

            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now - lastClick < 500)
            {
                InvokeDoubleClick(this, EventArgs.Empty);
            }
            else
            {
                InvokeMouseUp(this, EventArgs.Empty);
            }
            lastClick = now;

            return true;
        }

        if (x > Bounds.StartX() && x < Bounds.EndX() && y > Bounds.StartY() && y < Bounds.EndY())
        {
            if (State == State.Pressed)
            {
                return false;
            }

            if (type == TestType.MouseHover && State != State.Hover)
            {
                State = State.Hover;
                InvokeHover(this, EventArgs.Empty);
            }
            else if (type == TestType.MouseDown)
            {
                State = State.Pressed;
                InvokeMouseDown(this, EventArgs.Empty);

                Focused = true;
                InvokeFocus(this, EventArgs.Empty);
            }

            return true;
        }

        if (type == TestType.MouseDown)
        {
            Focused = false;
            InvokeBlur(this, EventArgs.Empty);
        }

        if (type == TestType.MouseHover && State == State.Hover || State == State.Pressed)
        {
            State = State.Default;
            InvokeLeave(this, EventArgs.Empty);
            return false;
        }

        return false;
    }


    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        if (State == State.Hover || Selected)
        {
            var border = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
                Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Height),
                OutlineColor = BorderColour,
                OutlineThickness = 4,
                FillColor = Color.Transparent
            };
            window.Draw(border);
        }

        var image = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX() + 8, Bounds.StartY() + 8),
            Size = new Vector2f(Bounds.EndY() - Bounds.StartY() - 16, Bounds.EndY() - Bounds.StartY() - 16),
            Texture = Texture
        };
        window.Draw(image);

        var font = new Font(@"Resources/Fonts/mojangles.ttf");
        var nameText = new Text(Name, font)
        {
            CharacterSize = 32,
            FillColor = Color.White,
            Position = new Vector2f(Bounds.StartX() + image.Size.X + 24, Bounds.StartY() + 8)
        };
        window.Draw(nameText);

        var descriptionText = new Text(Description, font)
        {
            CharacterSize = 24,
            FillColor = DescriptionColour,
            Position = new Vector2f(Bounds.StartX() + image.Size.X + 24, Bounds.StartY() + Height / 2)
        };
        window.Draw(descriptionText);

        openPageButton.Render(window, view, deltaTime);
    }
}