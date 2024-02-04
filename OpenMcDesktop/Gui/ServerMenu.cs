using SFML.Graphics;
using SFML.System;
namespace OpenMcDesktop.Gui;

public class ServerMenu : Control
{
    public string Title
    {
        get => title;
        set
        {
            title = value;
            titleLabel.UseFormatString(value);
        }
    }
    public string Description
    {
        get => description;
        set
        {
            description = value;
            descriptionLabel.UseFormatString(value);
        }
    }
    public List<PlayerInfo> Players { get; set; }

    private string title;
    private string description;
    private readonly Label titleLabel;
    private readonly Label descriptionLabel;

    private static readonly Font font;

    static ServerMenu()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public ServerMenu(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        titleLabel = new Label("...", 24, Control.BoundsZero, () => Bounds.StartY() + 32);
        titleLabel.Bounds.StartX = () => (int) ((Bounds.EndX() - Bounds.StartX()) / 2 - titleLabel.GetWidth() / 4);

        descriptionLabel = new Label("...", 24, Control.BoundsZero, () => Bounds.StartY() + 96);
        descriptionLabel.Bounds.StartX = () => (int) ((Bounds.EndX() - Bounds.StartX()) / 2 - descriptionLabel.GetWidth() / 4);
        Players = new List<PlayerInfo>();
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

        titleLabel.Render(window, view, deltaTime);
        descriptionLabel.Render(window, view, deltaTime);
    }
}
