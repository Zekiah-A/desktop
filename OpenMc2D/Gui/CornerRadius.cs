namespace OpenMc2D.Gui;

public struct CornerRadius
{
    public int TopLeft;
    public int TopRight;
    public int BottomLeft;
    public int BottomRight;
    
    public CornerRadius(int topLeft, int topRight, int bottomLeft, int bottomRight)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }
    
    public CornerRadius(int radius)
    {
        TopLeft = radius;
        TopRight = radius;
        BottomLeft = radius;
        BottomRight = radius;
    }
}