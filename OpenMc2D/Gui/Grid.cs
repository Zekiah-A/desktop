using SFML.Graphics;
using SFML.Window;

namespace OpenMc2D.Gui;

public class Grid : Control
{
    public int Columns
    {
        get => Children.GetLength(0);
    }

    public int Rows
    {
        get => Children.GetLength(1);
    }

    public int RowGap { get; set; } = 0;
    public int ColumnGap { get; set; } = 0;
    public Control?[,] Children { get; set; }
    
    public Grid(int columns, int rows, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Children = new Control?[columns, rows];
    }
    
    public override bool HitTest(int x, int y, TestType type)
    {

        foreach (var child in Children)
        {
            if (child != null && child.HitTest(x, y, type))
            {
                return true;
            }
        }
        return false;
    }

    public override bool KeyboardTest(Keyboard.Key key, int modifiers, TestType type)
    {
        foreach (var child in Children)
        {
            if (child != null && child.KeyboardTest(key, modifiers, type))
            {
                return true;
            }
        }
        
        return false;
    }
    
    public override bool TextTest(string unicode)
    {
        foreach (var child in Children)
        {
            if (child != null && child.TextTest(unicode))
            {
                return true;
            }
        }

        return false;
    }

    public override void Render(RenderWindow window, View view)
    {
        var columnWidth = (Bounds.EndX() - Bounds.StartX()) / Columns;
        var rowHeight = (Bounds.EndY() - Bounds.StartY()) / Rows;
        
        for (var column = 0; column < Columns; column++)
        {
            for (var row = 0; row < Rows; row++)
            {
                if (Children[column, row] is null)
                {
                    continue;
                }

                var childColumn = column;
                var childRow = row;
                Children[column, row]!.Bounds.StartX = () => Bounds.StartX() + (columnWidth + ColumnGap / 2) * childColumn;
                Children[column, row]!.Bounds.StartY = () => Bounds.StartY() + (rowHeight + RowGap / 2) * childRow;
                Children[column, row]!.Bounds.EndX = () => Bounds.StartX() + (columnWidth - ColumnGap / 2) * (childColumn + 1);
                Children[column, row]!.Bounds.EndY = () => Bounds.StartY() + (rowHeight - RowGap / 2) * (childRow + 1);
                Children[column, row]!.Render(window, view);
            }
        }
    }
}