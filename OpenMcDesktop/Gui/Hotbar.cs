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
    private const int BackgroundInnerWidth = 176;
    private const int SelectInnerWidth = 16;

    public Hotbar(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        backgroundTexture = new Texture("Resources/Textures/gui.png", new IntRect(0, 0, 182, 22));
        selectionTexture = new Texture("Resources/Textures/gui.png", new IntRect(0, 22, 24, 24));
        Items = new Item?[9];
    }

    public int ScrollSelection(int by)
    {
        var selected = (Selected + by) % SlotCount;
        if (selected < 0)
        {
            selected = SlotCount + selected;
        }
        Selected = selected;
        return selected;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var startX = Bounds.StartX();
        var startY = Bounds.StartY();
        var width = Bounds.EndX() - startX;
        var height = Bounds.EndY() - startY;

        var backgroundRect = new RectangleShape
        {
            Texture = backgroundTexture,
            Position = new Vector2f(startX, startY),
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
                Position = new Vector2f(startX + i * height, startY),
                Size = new Vector2f(height, height),
                Texture = Items[i]!.InstanceTexture
            };

            window.Draw(itemRect);
        }

        var selectSize = selectionTexture.Size.X / (float) backgroundTexture.Size.X * width;
        var selectInnerWidth = (SelectInnerWidth / (float) selectionTexture.Size.X) * selectSize;
        var selectBorderWidth = (selectSize - selectInnerWidth) / 2.0f;
        var texturePixelSize = (1.0f / selectionTexture.Size.X) * selectSize;
        var selectRect = new RectangleShape
        {
            Texture = selectionTexture,
            Position = new Vector2f(
                startX + Selected * (selectSize - selectBorderWidth) - texturePixelSize,
                startY + (height / 2.0f - selectSize / 2.0f)),
            Size = new Vector2f(selectSize, selectSize)
        };
        window.Draw(selectRect);
    }
}
