using OpenMcDesktop;
using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

/// <summary>
/// Scroll box doesn't affect the positioning of it's children by default, it only crops children who exceed it's bounds,
/// and provides scrolling functionality for overflowing children
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

    // We consume scroll (sometimes) unlike page which passes it
    public override bool WheelTest(int x, int y, float delta, TestType type)
    {
        if (x > Bounds.StartX() && x < Bounds.EndX() && y > Bounds.StartY() && y < Bounds.EndY())
        {
            Delta = delta * 20;
            return true;
        }

        return false;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        /*var scrollView = new View(new FloatRect(0, 0, window.Size.X, window.Size.Y));*/
        var scrollView = new View(new FloatRect(
            Bounds.StartX(),
            Bounds.StartY(),
            (Bounds.EndX() - Bounds.StartX()),
            (Bounds.EndY() - Bounds.StartY())));
        /*var scrollView = new View(new FloatRect(
            Bounds.StartX(),
            Bounds.StartY(),
            Bounds.EndX(),
            Bounds.EndY()));*/
        scrollView.Viewport = new FloatRect(
            Bounds.StartX() / (float)window.Size.X,
            Bounds.StartY() / (float)window.Size.Y,
            (Bounds.EndX() - Bounds.StartX()) / (float)window.Size.X,
            (Bounds.EndY() - Bounds.StartY()) / (float)window.Size.Y);

        // Simulate kinetic sscrolling
        Delta = MathHelpers.Lerp(Delta, 0, 0.1f);
        Scroll -= Delta;
        //scrollView.Move(new Vector2f(Bounds.StartX(), Scroll));
        scrollView.Move(new Vector2f(-64, Scroll));

        window.SetView(scrollView);
        foreach (var child in Children.ToList())
        {
            child.Render(window, scrollView, deltaTime);
        }
        window.SetView(view);
    }
}
