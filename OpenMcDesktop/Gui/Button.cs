using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class Button : Control
{
    public string Text;

    private readonly Texture guiTexture;
    private readonly IntRect normalCrop;
    private readonly IntRect hoveredCrop;
    private readonly IntRect pressedCrop;
    private readonly Font font;

    public Button(string text) : this(text, BoundsZero, BoundsZero, BoundsZero, BoundsZero)
    {

    }

    public Button(string text, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Text = text;
        guiTexture = new Texture(@"Resources/Textures/gui.png");
        normalCrop = new IntRect(0, 66, 200, 20);
        hoveredCrop = new IntRect(0, 86, 200, 20);
        pressedCrop = new IntRect(0, 46, 200, 20);

        font = new Font(@"Resources/Fonts/mojangles.ttf");
        var buffer = new SoundBuffer(@"Resources/Audio/click.ogg");
        var clickSound = new Sound(buffer);

        OnMouseUp += (sender, args) =>
        {
            clickSound.Play();
        };
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var background = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
            TextureRect = State switch
            {
                State.Default => normalCrop with { Left = 2, Width = 196 },
                State.Hover => hoveredCrop with { Left = 2, Width = 196 },
                _ => pressedCrop with { Left = 2, Width = 196 }
            },
            Texture = guiTexture
        };
        window.Draw(background);

        // We include backgroundLeft/right to cover up the "squished" side borders when the button has a non-default width,
        // like NinePatching, except for only two sides.
        var backgroundLeft = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            Size = new Vector2f(0.1f * (Bounds.EndY() - Bounds.StartY()), Bounds.EndY() - Bounds.StartY()),
            TextureRect = State switch
            {
                State.Default => normalCrop with { Width = 2 },
                State.Hover => hoveredCrop with { Width = 2 },
                _ => pressedCrop with { Width = 2 }
            },
            Texture = guiTexture
        };
        window.Draw(backgroundLeft);

        var backgroundRight = new RectangleShape
        {
            Position = new Vector2f(Bounds.EndX() - 0.1f * (Bounds.EndY() - Bounds.StartY()), Bounds.StartY()),
            Size = new Vector2f(0.1f * (Bounds.EndY() - Bounds.StartY()), Bounds.EndY() - Bounds.StartY()),
            TextureRect = State switch
            {
                State.Default => normalCrop with { Width = 2, Left = 198 },
                State.Hover => hoveredCrop with { Width = 2, Left = 198 },
                _ => pressedCrop with { Width = 2, Left = 198 }
            },
            Texture = guiTexture
        };
        window.Draw(backgroundRight);

        var text = new Text(Text, font);
        text.CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f);
        text.Position = new Vector2f(Bounds.StartX() + (Bounds.EndX() - Bounds.StartX()) / 2 - text.GetLocalBounds().Width / 2, Bounds.StartY() + 2);
        window.Draw(text);
    }
}