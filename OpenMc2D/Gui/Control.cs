using SFML.Graphics;

namespace OpenMc2D.Gui;

public abstract class Control
{
    public State State = State.Default;
    public Bounds Bounds = Bounds.Default;
    public event EventHandler<EventArgs>? OnHover;
    public event EventHandler<EventArgs>? OnLeave;
    public event EventHandler<EventArgs>? OnMouseDown;
    public event EventHandler<EventArgs>? OnMouseUp;

    protected Control(Func<int> x, Func<int> y, Func<int>? width = null, Func<int>? height = null)
    {
        Bounds.StartX = x;
        Bounds.StartY = y;
        Bounds.EndX = () => x() + (width?.Invoke() ?? 0);
        Bounds.EndY = () => y() + (height?.Invoke() ?? 0);
    }

    protected Control()
    {
        Bounds.StartX = () => 0;
        Bounds.StartY = () => 0;
        Bounds.EndX = () => 0;
        Bounds.EndY = () => 0;
    }

    public virtual bool HitTest(int x, int y, TestType type)
    {
        if (type == TestType.Up && State == State.Pressed)
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
            
            if (type == TestType.Hover && State != State.Hover)
            {
                State = State.Hover;
                OnHover?.Invoke(this, EventArgs.Empty);
            }
            else if (type == TestType.Down)
            {
                State = State.Pressed;
                OnMouseDown?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }
        
        if (type == TestType.Hover && State == State.Hover || State == State.Pressed)
        {
            State = State.Default;
            OnLeave?.Invoke(this, EventArgs.Empty);
            return false;
        }

        return false;
    }

    public virtual void Render(RenderWindow window, View view) { }
}