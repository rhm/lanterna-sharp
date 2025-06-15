using System;
using System.Runtime.InteropServices;

namespace Lanterna.Terminal.Win
{
    /// <summary>
    /// Windows definitions and structures for console operations
    /// </summary>
    public static class WinDef
    {
        /// <summary>
        /// COORD structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short x, short y)
            {
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                return $"COORD({X},{Y})";
            }
        }

        /// <summary>
        /// SMALL_RECT structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;

            public override string ToString()
            {
                return $"SMALL_RECT({Left},{Top})({Right},{Bottom})";
            }
        }

        /// <summary>
        /// CONSOLE_SCREEN_BUFFER_INFO structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;

            public override string ToString()
            {
                return $"CONSOLE_SCREEN_BUFFER_INFO({dwSize},{dwCursorPosition},{wAttributes},{srWindow},{dwMaximumWindowSize})";
            }
        }

        /// <summary>
        /// INPUT_RECORD structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT_RECORD
        {
            public const short KEY_EVENT = 0x01;
            public const short MOUSE_EVENT = 0x02;
            public const short WINDOW_BUFFER_SIZE_EVENT = 0x04;

            public short EventType;
            public EventUnion Event;

            public override string ToString()
            {
                return $"INPUT_RECORD({EventType})";
            }
        }

        /// <summary>
        /// Union for different event types in INPUT_RECORD
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct EventUnion
        {
            [FieldOffset(0)]
            public KEY_EVENT_RECORD KeyEvent;
            [FieldOffset(0)]
            public MOUSE_EVENT_RECORD MouseEvent;
            [FieldOffset(0)]
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
        }

        /// <summary>
        /// KEY_EVENT_RECORD structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_EVENT_RECORD
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool bKeyDown;
            public short wRepeatCount;
            public short wVirtualKeyCode;
            public short wVirtualScanCode;
            public char uChar;
            public uint dwControlKeyState;

            public override string ToString()
            {
                return $"KEY_EVENT_RECORD({bKeyDown},{wRepeatCount},{wVirtualKeyCode},{wVirtualScanCode},{uChar},{dwControlKeyState})";
            }
        }

        /// <summary>
        /// MOUSE_EVENT_RECORD structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSE_EVENT_RECORD
        {
            public COORD dwMousePosition;
            public uint dwButtonState;
            public uint dwControlKeyState;
            public uint dwEventFlags;

            public override string ToString()
            {
                return $"MOUSE_EVENT_RECORD({dwMousePosition},{dwButtonState},{dwControlKeyState},{dwEventFlags})";
            }
        }

        /// <summary>
        /// WINDOW_BUFFER_SIZE_RECORD structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOW_BUFFER_SIZE_RECORD
        {
            public COORD dwSize;

            public override string ToString()
            {
                return $"WINDOW_BUFFER_SIZE_RECORD({dwSize})";
            }
        }
    }
}