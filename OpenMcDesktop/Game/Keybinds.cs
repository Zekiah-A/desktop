using OpenMcDesktop.Gui;
using SFML.Window;

namespace OpenMcDesktop.Game;

// TODO: Remove this
/// <summary>
/// Provides key events to be used by the game that have been propagated from the UI. Not to be used for real-time input,
/// but instead, for one time keybinding, such as opening up a certain panel or menu, or performing an action that does not
/// require per-frame continuous input checking.
/// </summary>
public static class Keybinds
{
    public delegate void KeyEventHandler(int modifiers, TestType type);
    public static Dictionary<Keyboard.Key, KeyEventHandler> KeySubscribers;

    public delegate void MouseEventHandler(int x, int y, TestType type);
    public static List<MouseEventHandler> MouseSubscribers;

    static Keybinds()
    {
        KeySubscribers = new Dictionary<Keyboard.Key, KeyEventHandler>();
        MouseSubscribers = new List<MouseEventHandler>();
    }

    public static void MouseEvent(int x, int y, TestType type)
    {
        foreach (var subscriber in MouseSubscribers)
        {
            subscriber.Invoke(x, y, type);
        }
    }

    public static void KeyEvent(Keyboard.Key key, int modifiers, TestType type)
    {
        if (KeySubscribers.TryGetValue(key, out var subscriber))
        {
            subscriber.Invoke(modifiers, type);
        }
    }

    public static void SubscribeToKey(Keyboard.Key key, KeyEventHandler handler)
    {
        KeySubscribers.Add(key, handler);
    }
}
