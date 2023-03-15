using OpenMc2D.Gui;
using SFML.Graphics;
using SFML.System;

namespace OpenMc2D.Networking;

public class DisplayListItem : Control
{
    public Texture Texture { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Height { get; set; } = 128;
    public Color BorderColour { get; set; } = new(255, 255, 255, 128);

    public DisplayListItem(Texture texture, string name, string description)
    {
        Texture = texture;
        Name = name;
        Description = description;
    }

    public override void Render(RenderWindow window, View view)
    {
        if (State == State.Hover)
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
    }
}