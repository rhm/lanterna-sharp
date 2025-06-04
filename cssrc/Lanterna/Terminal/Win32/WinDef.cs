namespace Lanterna.Terminal.Win32
{
    /// <summary>
    /// Placeholder structures for Windows console interop.
    /// They only contain the fields needed by the stub classes.
    /// </summary>
    public static class WinDef
    {
        public struct COORD
        {
            public short X;
            public short Y;
        }

        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        public struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        public struct INPUT_RECORD { }
        public struct KEY_EVENT_RECORD { }
        public struct MOUSE_EVENT_RECORD { }
        public struct WINDOW_BUFFER_SIZE_RECORD { public COORD dwSize; }
    }
}
