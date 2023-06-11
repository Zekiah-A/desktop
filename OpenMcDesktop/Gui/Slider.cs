namespace OpenMcDesktop.Gui;

public class Slider : Control
{
    public string Text;
    public int Value;
    public int MaxValue = 10;
    public event EventHandler ValueChanged;

    public Slider(int value, string text)
    {
        Value = value;
        Text = text;
    }
}