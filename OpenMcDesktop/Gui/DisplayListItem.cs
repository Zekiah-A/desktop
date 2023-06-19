using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class DisplayListItem : Control
{
    public bool Selected { get; set; }
    public Texture Texture { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Height { get; set; } = 128;
    public Color BorderColour { get; set; } = new(255, 255, 255, 128);
    public Color DescriptionColour { get; set; } = new(255, 255, 255, 200);
    
    public DisplayListItem(Texture texture, string name, string description)
    {
        Texture = texture;
        Name = name;
        Description = description;
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
    }
}