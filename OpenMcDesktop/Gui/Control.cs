using SFML.Graphics;
using SFML.Window;

namespace OpenMcDesktop.Gui;

public abstract class Control
{
    public State State = State.Default;
    public Bounds Bounds = Bounds.Default;
    public bool Focused = false;
    public event EventHandler<EventArgs>? OnHover;
    public event EventHandler<EventArgs>? OnLeave;
    public event EventHandler<EventArgs>? OnMouseDown;
    public event EventHandler<EventArgs>? OnMouseUp;
    public event EventHandler<EventArgs>? OnFocus;
    public event EventHandler<EventArgs>? OnBlur;
    public event EventHandler<ScrollEventArgs>? OnScroll;

    public static int BoundsZero() => 0;
    private int Width => Bounds.EndX() - Bounds.StartX();
    private int Height => Bounds.EndY() - Bounds.StartY();

    protected void InvokeHover(object? sender, EventArgs args) => OnHover?.Invoke(sender, args);
    protected void InvokeLeave(object? sender, EventArgs args) => OnLeave?.Invoke(sender, args);
    protected void InvokeMouseDown(object? sender, EventArgs args) => OnMouseDown?.Invoke(sender, args);
    protected void InvokeMouseUp(object? sender, EventArgs args) => OnMouseUp?.Invoke(sender, args);
    protected void InvokeFocus(object? sender, EventArgs args) => OnFocus?.Invoke(sender, args);
    protected void InvokeBlur(object? sender, EventArgs args) => OnBlur?.Invoke(sender, args);
    protected void InvokeScroll(object? sender, ScrollEventArgs args) => OnScroll?.Invoke(sender, args);

    protected Control(Func<int> x, Func<int> y, Func<int>? width = null, Func<int>? height = null)
    {
        Bounds.StartX = x;
        Bounds.StartY = y;
        Bounds.EndX = () => x() + (width?.Invoke() ?? 0);
        Bounds.EndY = () => y() + (height?.Invoke() ?? 0);
    }

    protected Control(Bounds bounds)
    {
        Bounds = bounds;
    }

    protected Control()
    {
        Bounds.StartX = BoundsZero;
        Bounds.StartY = BoundsZero;
        Bounds.EndX = BoundsZero;
        Bounds.EndY = BoundsZero;
    }

    public virtual bool HitTest(int x, int y, TestType type)
    {
        if (type == TestType.MouseUp && State == State.Pressed)
        {
            State = State.Default;
            OnMouseUp?.Invoke(this, EventArgs.Empty);
            return true;
        }

        if (x > Bounds.StartX() && x < Bounds.EndX() && y > Bounds.StartY() && y < Bounds.EndY())
        {
            if (State == State.Pressed)
            {
                return false;
            }

            if (type == TestType.MouseHover && State != State.Hover)
            {
                State = State.Hover;
                OnHover?.Invoke(this, EventArgs.Empty);
            }
            else if (type == TestType.MouseDown)
            {
                State = State.Pressed;
                OnMouseDown?.Invoke(this, EventArgs.Empty);

                Focused = true;
                OnFocus?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }

        if (type == TestType.MouseDown)
        {
            Focused = false;
            OnBlur?.Invoke(this, EventArgs.Empty);
        }

        if (type == TestType.MouseHover && State == State.Hover || State == State.Pressed)
        {
            State = State.Default;
            OnLeave?.Invoke(this, EventArgs.Empty);
            return false;
        }

        return false;
    }

    public virtual bool WheelTest(int x, int y, float delta, TestType type)
    {
        if (x > Bounds.StartX() && x < Bounds.EndX() && y > Bounds.StartY() && y < Bounds.EndY())
        {
            OnScroll?.Invoke(this, new ScrollEventArgs(delta));
            return true;
        }

        return false;
    }

    public virtual bool KeyboardTest(Keyboard.Key key, int modifiers, TestType type)
    {
        return false;
    }

    public virtual bool TextTest(string unicode)
    {
        return false;
    }

    public virtual void Render(RenderWindow window, View view, float deltaTime) { }
}
