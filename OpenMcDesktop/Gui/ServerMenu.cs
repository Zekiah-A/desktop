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

    private const int LabelHeight = 32;
    private const int LabelSpacing = 16;

    private static readonly Font font;
    private static readonly Texture[] pingTextures;

    static ServerMenu()
    {
        font = new Font(@"Resources/Fonts/mojangles.ttf");
        var pingtextureMap = new Texture(@"Resources/Textures/icons.png");
        pingTextures = new Texture[]
        {
            new Texture(pingtextureMap.CopyToImage(), new IntRect(16, 54, 10, 10)), // 5 bars
            new Texture(pingtextureMap.CopyToImage(), new IntRect(27, 54, 10, 10)), // 4 bars
            new Texture(pingtextureMap.CopyToImage(), new IntRect(38, 54, 10, 10)), // 3 bars
            new Texture(pingtextureMap.CopyToImage(), new IntRect(49, 54, 10, 10)), // 2 bars
            new Texture(pingtextureMap.CopyToImage(), new IntRect(60, 54, 10, 10)), // 1 bar
        };
    }

    public ServerMenu(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        title = "";
        description = "";
        titleLabel = new Label("...", 24, Control.BoundsZero, () => Bounds.StartY() + 16);
        titleLabel.Bounds.StartX = () => (int) (Bounds.StartX() + (Bounds.EndX() - Bounds.StartX()) / 2 - titleLabel.GetWidth() / 2);

        descriptionLabel = new Label("...", 24, Control.BoundsZero, () => Bounds.StartX() + 48);
        descriptionLabel.Bounds.StartX = () => (int) (Bounds.StartX() + (Bounds.EndX() - Bounds.StartX()) / 2 - descriptionLabel.GetWidth() / 2);
        Players = new List<PlayerInfo>();
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var background = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            FillColor = new Color(0, 0, 0, 64),
            Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
        };
        window.Draw(background);

        titleLabel.Render(window, view, deltaTime);
        descriptionLabel.Render(window, view, deltaTime);

        var labelColumn = 0;
        for (var i = 0; i < Players.ToList().Count; i++)
        {
            var player = Players[i];
            var columnWidth = Bounds.StartY() + 96 + i * (LabelHeight + LabelSpacing);
            var labelPosition = new Vector2f(labelColumn + Bounds.StartX() + 120, columnWidth - 16);
            var labelWidth = (Bounds.EndX() - Bounds.StartX() - 240) / 2;
            var labelRect = new RectangleShape
            {
                Position = labelPosition,
                FillColor = new Color(195, 195, 195, 96),
                Size = new Vector2f(labelWidth, LabelHeight),
            };
            window.Draw(labelRect);

            var skinRect = new RectangleShape
            {
                Position = labelPosition + new Vector2f(2, 2),
                Size = new Vector2f(LabelHeight - 4, LabelHeight - 4),
                Texture = player.SkinIcon
            };
            window.Draw(skinRect);

            var namePosition = labelPosition + new Vector2f(LabelHeight + 12, -4);
            var nameShadow = new Text(player.Name, font)
            {
                FillColor = new Color(64, 64, 64, 192),
                Position = namePosition + new Vector2f(2, 2),
            };
            window.Draw(nameShadow);
            var nameText = new Text(player.Name, font)
            {
                Position = namePosition,
            };
            window.Draw(nameText);


            var pingIndicator = new RectangleShape
            {
                Position = labelPosition + new Vector2f(labelWidth - LabelHeight - 2, 2),
                Size = new Vector2f(LabelHeight - 4, LabelHeight - 4),
                Texture = player.Ping switch
                {
                    < 25 => pingTextures[0],
                    < 60 => pingTextures[1],
                    < 300 => pingTextures[2],
                    < 1000 => pingTextures[3],
                    _ => pingTextures[4]
                }
            };
            window.Draw(pingIndicator);

            // TODO: Cleanup all the alignment code into something more reasonable
            if (Bounds.EndX() - labelPosition.Y < 64)
            {
                labelColumn += columnWidth + 16;
            }
        }
    }
}
