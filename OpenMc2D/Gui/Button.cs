using SFML.Graphics;
using SFML.System;

namespace OpenMc2D.Gui;

public class Button : Control
{
    public string Text;

    private Texture guiTexture;
    private IntRect normalCrop;
    private IntRect hoveredCrop;
    private IntRect pressedCrop;
    private Font font;
    
    public Button(string text, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Text = text;
        guiTexture = new Texture(@"Resources/Textures/gui.png");
        normalCrop = new IntRect(0, 66, 200, 20);
        hoveredCrop = new IntRect(0, 86, 200, 20);
        pressedCrop = new IntRect(0, 46, 200, 20);

        font = new Font(@"Resources/Fonts/mojangles.ttf");
    }

    public override void Render(RenderWindow window)
    {
        var background = new RectangleShape
        {
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            Size = new Vector2f(Bounds.EndX() - Bounds.StartX(), Bounds.EndY() - Bounds.StartY()),
            TextureRect = State switch
            {
                State.Default => normalCrop,
                State.Hover => hoveredCrop,
                _ => pressedCrop
            },
            Texture = guiTexture
        };
        window.Draw(background);
        
        var text = new Text(Text, font);
        
        text.CharacterSize = (uint) ((Bounds.EndY() - Bounds.StartY()) / 1.4f);
        text.Position = new Vector2f(Bounds.StartX() +  (Bounds.EndX() - Bounds.StartX()) / 2 - text.GetLocalBounds().Width / 2, Bounds.StartY() + 2);
        window.Draw(text);
    }
}