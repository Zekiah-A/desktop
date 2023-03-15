using SFML.Graphics;
using SFML.Window;

namespace OpenMc2D.Gui;

/// <summary>
/// A generic control that has the ability to group a number of controls via it's Children.
/// </summary>
public class Page : Control
{
    public List<Control> Children;
    
    public Page()
    {
        Children = new List<Control>();
    }

    public override bool HitTest(int x, int y, TestType type)
    {
        for (var i = Children.Count - 1; i >= 0; i--) {
            if (Children[i].HitTest(x, y, type))
            {
                return true;
            }
        }

        return false;
    }

    public override void Render(RenderWindow window, View view)
    {
        window.Clear(Color.Black);

        foreach (var child in Children)
        {
            child.Render(window, view);
        }
    }
}