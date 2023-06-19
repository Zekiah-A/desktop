using System.Collections.Concurrent;
using SFML.Graphics;
using SFML.Window;

namespace OpenMcDesktop.Gui;

/// <summary>
/// A generic control that has the ability to group a number of controls via it's Children.
/// </summary>
public class Page : Control
{
    public List<Control> Children;

    public Page()
    {
        Children = new List<Control>();
    }

    public override bool HitTest(int x, int y, TestType type)
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i].HitTest(x, y, type))
            {
                return true;
            }
        }

        return false;
    }

    public override bool KeyboardTest(Keyboard.Key key, int modifiers, TestType type)
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i].KeyboardTest(key, modifiers, type))
            {
                return true;
            }
        }

        return false;
    }
    
    public override bool TextTest(string unicode)
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i].TextTest(unicode))
            {
                return true;
            }
        }

        return false;
    }

    public override void Render(RenderWindow window, View view, float deltaTime)
    {
        // Unfortunately we must copy the children every single frame otherwise enumeration modification exceptions may occur
        // if a child in this collection is added or modified during rendering.
        foreach (var child in Children.ToList())
        {
            child.Render(window, view, deltaTime);
        }
    }
}