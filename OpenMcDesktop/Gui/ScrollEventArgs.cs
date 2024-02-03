namespace OpenMcDesktop.Gui;

public class ScrollEventArgs : EventArgs
{
    public float Delta { get; set; }

    public ScrollEventArgs(float delta)
    {
        Delta = delta;
    }
}
