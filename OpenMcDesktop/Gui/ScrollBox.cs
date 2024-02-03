using OpenMcDesktop;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace OpenMcDesktop.Gui;

/// <summary>
/// Scroll box doesn't affect the positioning of it's children by default, it only crops children who exceed it's bounds,
/// and provides scrolling functionality for overflowing children. Positioned elements should be careful when using
/// window.__GetView()__.Size in position properties, as the view dimensions used by the scrollbox may differ from that of
/// the outer window. In such cases using window.Size may be preferable
/// </summary>
public class ScrollBox : Page
{
    private float Delta = 0.0f;
    public float Scroll = 0.0f;

    public ScrollBox(Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base()
    {
        Bounds.StartX = x;
        Bounds.StartY = y;
        Bounds.EndX = () => x() + (width?.Invoke() ?? 0);
        Bounds.EndY = () => y() + (height?.Invoke() ?? 0);
    }

    public ScrollBox() : base()
    {
    }

    public override bool HitTest(int x, int y, TestType type)
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i].HitTest(x, y + (int) Scroll, type))
            {
                return true;
            }
        }

        return false;
    }

    // We consume scroll (sometimes) unlike page which passes it
    public override bool WheelTest(int x, int y, float delta, TestType type)
    {
        if (x > Bounds.StartX() && x < Bounds.EndX() && y > Bounds.StartY() && y < Bounds.EndY())
        {
            Delta = Delta + delta * 6;
            return true;
        }

        return false;
    }


    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        var scrollView = new View(new FloatRect(
            Bounds.StartX(),
            Bounds.StartY(),
            (Bounds.EndX() - Bounds.StartX()),
            (Bounds.EndY() - Bounds.StartY())));
        scrollView.Viewport = new FloatRect(
            Bounds.StartX() / (float)window.Size.X,
            Bounds.StartY() / (float)window.Size.Y,
            (Bounds.EndX() - Bounds.StartX()) / (float)window.Size.X,
            (Bounds.EndY() - Bounds.StartY()) / (float)window.Size.Y);

        // Simulate kinetic scrolling
        Delta = MathHelpers.Lerp(Delta, 0, 0.1f);
        Scroll -= Delta;
        Scroll = Math.Clamp(Scroll, 0, Bounds.EndY()); // TODO: Find greatest extent bounds of children
        scrollView.Move(new Vector2f(0, Scroll));

        window.SetView(scrollView);
        foreach (var child in Children.ToList())
        {
            child.Render(window, scrollView, deltaTime);
        }
        window.SetView(view);
    }
}
