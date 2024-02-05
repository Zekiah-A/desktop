using SFML.System;
using SFML.Graphics;

namespace OpenMcDesktop.Gui;

public class PauseMenu : Control
{
    private static readonly Font font;

    static PauseMenu()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public PauseMenu(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {

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
    }
}
