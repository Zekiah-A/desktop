using SFML.Graphics;
using SFML.Window;

namespace OpenMc2D.Gui;

public class Page : Control
{
    public List<Control> Children;

    public Page(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
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

    public override void Render(RenderWindow window)
    {
        window.Clear(Color.Black);

        foreach (var child in Children)
        {
            child.Render(window);
        }
    }
}