namespace OpenMcDesktop.Gui;

public record struct Bounds(Func<int> StartX, Func<int> StartY, Func<int> EndX, Func<int> EndY)
{
    public static Bounds Default => new(() => 0, () => 0, () => 0, () => 0);
}
