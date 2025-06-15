using System;
using System.IO;
using System.Text;
using Lanterna.Core;
using Lanterna.Terminal.Ansi;

namespace Lanterna.Terminal.Win
{
    /// <summary>
    /// Windows Terminal implementation that uses Win32 Console APIs
    /// </summary>
    public class WindowsTerminal : UnixLikeTerminal
    {
        private static readonly Encoding CONSOLE_CHARSET = Encoding.UTF8;
        private static readonly WindowsConsoleInputStream CONSOLE_INPUT = new WindowsConsoleInputStream(CONSOLE_CHARSET);
        private static readonly WindowsConsoleOutputStream CONSOLE_OUTPUT = new WindowsConsoleOutputStream(CONSOLE_CHARSET);

        private uint[]? settings;

        public WindowsTerminal() 
            : this(CONSOLE_INPUT, CONSOLE_OUTPUT, CONSOLE_CHARSET, CtrlCBehaviour.CtrlCKillsApplication)
        {
        }

        public WindowsTerminal(Stream terminalInput, Stream terminalOutput, Encoding terminalCharset, CtrlCBehaviour terminalCtrlCBehaviour)
            : base(CONSOLE_INPUT, CONSOLE_OUTPUT, CONSOLE_CHARSET, terminalCtrlCBehaviour)
        {
            // Handle resize events
            CONSOLE_INPUT.WindowBufferSizeEvent += evt =>
            {
                OnResized(evt.dwSize.X, evt.dwSize.Y);
            };
        }

        protected override void Acquire()
        {
            base.Acquire();

            uint terminalOutputMode = GetConsoleOutputMode();
            terminalOutputMode |= Wincon.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            terminalOutputMode |= Wincon.DISABLE_NEWLINE_AUTO_RETURN;
            Wincon.SetConsoleMode(CONSOLE_OUTPUT.Handle, terminalOutputMode);

            uint terminalInputMode = GetConsoleInputMode();
            terminalInputMode |= Wincon.ENABLE_MOUSE_INPUT;
            terminalInputMode |= Wincon.ENABLE_WINDOW_INPUT;
            terminalInputMode |= Wincon.ENABLE_VIRTUAL_TERMINAL_INPUT;
            Wincon.SetConsoleMode(CONSOLE_INPUT.Handle, terminalInputMode);
        }

        protected override void SaveTerminalSettings()
        {
            settings = new uint[] { GetConsoleInputMode(), GetConsoleOutputMode() };
        }

        protected override void RestoreTerminalSettings()
        {
            if (settings != null)
            {
                Wincon.SetConsoleMode(CONSOLE_INPUT.Handle, settings[0]);
                Wincon.SetConsoleMode(CONSOLE_OUTPUT.Handle, settings[1]);
            }
        }

        protected override void KeyEchoEnabled(bool enabled)
        {
            uint mode = GetConsoleInputMode();
            if (enabled)
            {
                mode |= Wincon.ENABLE_ECHO_INPUT;
            }
            else
            {
                mode &= ~Wincon.ENABLE_ECHO_INPUT;
            }
            Wincon.SetConsoleMode(CONSOLE_INPUT.Handle, mode);
        }

        protected override void CanonicalMode(bool enabled)
        {
            uint mode = GetConsoleInputMode();
            if (enabled)
            {
                mode |= Wincon.ENABLE_LINE_INPUT;
            }
            else
            {
                mode &= ~Wincon.ENABLE_LINE_INPUT;
            }
            Wincon.SetConsoleMode(CONSOLE_INPUT.Handle, mode);
        }

        protected override void KeyStrokeSignalsEnabled(bool enabled)
        {
            uint mode = GetConsoleInputMode();
            if (enabled)
            {
                mode |= Wincon.ENABLE_PROCESSED_INPUT;
            }
            else
            {
                mode &= ~Wincon.ENABLE_PROCESSED_INPUT;
            }
            Wincon.SetConsoleMode(CONSOLE_INPUT.Handle, mode);
        }

        public override TerminalSize GetTerminalSize()
        {
            if (Wincon.GetConsoleScreenBufferInfo(CONSOLE_OUTPUT.Handle, out WinDef.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo))
            {
                int columns = screenBufferInfo.srWindow.Right - screenBufferInfo.srWindow.Left + 1;
                int rows = screenBufferInfo.srWindow.Bottom - screenBufferInfo.srWindow.Top + 1;
                return new TerminalSize(columns, rows);
            }
            
            // Fallback to a reasonable default
            return new TerminalSize(80, 24);
        }

        protected override void RegisterTerminalResizeListener(Action onResize)
        {
            // Windows console resize is handled through the WindowBufferSizeEvent
            // which is already wired up in the constructor
        }

        public override TerminalPosition GetCursorPosition()
        {
            if (Wincon.GetConsoleScreenBufferInfo(CONSOLE_OUTPUT.Handle, out WinDef.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo))
            {
                int column = screenBufferInfo.dwCursorPosition.X - screenBufferInfo.srWindow.Left;
                int row = screenBufferInfo.dwCursorPosition.Y - screenBufferInfo.srWindow.Top;
                return new TerminalPosition(column, row);
            }
            
            // Fallback
            return new TerminalPosition(0, 0);
        }

        private uint GetConsoleInputMode()
        {
            if (Wincon.GetConsoleMode(CONSOLE_INPUT.Handle, out uint mode))
            {
                return mode;
            }
            return 0;
        }

        private uint GetConsoleOutputMode()
        {
            if (Wincon.GetConsoleMode(CONSOLE_OUTPUT.Handle, out uint mode))
            {
                return mode;
            }
            return 0;
        }
    }
}