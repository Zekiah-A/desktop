using OpenMcDesktop.Networking;
using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class DisplayList : Control
{
    public List<DisplayListItem> Children;
    public int ItemHeight = 128;
    public int ItemSpacing = 16;
    public int Scroll = 0;
    public int SelectedIndex = -1;
    
    public DisplayList(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Children = new List<DisplayListItem>();
    }
    
    public override bool HitTest(int x, int y, TestType type)
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i].HitTest(x, y, type))
            {
                SelectedIndex = i;
                return true;
            }
        }

        return false;
    }

    public override void Render(RenderWindow window, View view)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            var childIndex = i;
            Children[i].Bounds.StartX = Bounds.StartX;
            Children[i].Bounds.EndX = Bounds.EndX;
            Children[i].Bounds.StartY = () => Bounds.StartY() + childIndex * (ItemHeight + ItemSpacing);
            Children[i].Bounds.EndY = () => Children[childIndex].Bounds.StartY() + Children[childIndex].Height;
            Children[i].Render(window, view);
        }
    }
}