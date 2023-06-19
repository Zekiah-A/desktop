using OpenMcDesktop.Game.Definitions;
using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class Hotbar : Control
{
    public const int SlotCount = 9;
    public int Selected;
    private Texture backgroundTexture;
    private Texture selectionTexture;
    private Item?[] Items;
    
    public Hotbar(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        backgroundTexture = new Texture("Resources/Textures/gui.png", new IntRect(0, 0, 182, 22));
        selectionTexture = new Texture("Resources/Textures/gui.png", new IntRect(0, 22, 24, 24));
        Items = new Item?[9];
    }
    
    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var width = Bounds.EndX() - Bounds.StartX();
        var height = Bounds.EndY() - Bounds.StartY();
        
        var backgroundRect = new RectangleShape
        {
            Texture = backgroundTexture,
            Position = new Vector2f(Bounds.StartX(), Bounds.StartY()),
            Size = new Vector2f(width, height)
        };
        window.Draw(backgroundRect);

        for (var i = 0; i < Items.Length; i++)
        {
            if (Items[i] is null)
            {
                continue;
            }
            
            var itemRect = new RectangleShape
            {
                Position = new Vector2f(Bounds.StartX() + i * height, Bounds.StartY()),
                Size = new Vector2f(height, height),
                Texture = Items[i]!.InstanceTexture
            };
            
            window.Draw(itemRect);
        }
        
        var selectSize = selectionTexture.Size.X / (float)backgroundTexture.Size.X * width;
        var selectRect = new RectangleShape
        {
            Texture = selectionTexture,
            Position = new Vector2f(
                Bounds.StartX() + Selected * selectSize,
                Bounds.StartY() + (height / 2.0f - selectSize / 2.0f)),
            Size = new Vector2f(selectSize, selectSize)
        };
        window.Draw(selectRect);
    }
}