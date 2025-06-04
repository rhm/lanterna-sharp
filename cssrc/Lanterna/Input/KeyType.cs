namespace Lanterna.Input
{
    /// <summary>
    /// Enumeration describing the kind of key pressed.
    /// This mirrors the KeyType enum from the Java implementation.
    /// </summary>
    public enum KeyType
    {
        Character,
        Escape,
        Backspace,
        ArrowLeft,
        ArrowRight,
        ArrowUp,
        ArrowDown,
        Insert,
        Delete,
        Home,
        End,
        PageUp,
        PageDown,
        Tab,
        ReverseTab,
        Enter,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        F13,
        F14,
        F15,
        F16,
        F17,
        F18,
        F19,
        KeyTypeMarker,
        // Virtual key types
        CursorLocation,
        MouseEvent,
        Eof
    }
}
