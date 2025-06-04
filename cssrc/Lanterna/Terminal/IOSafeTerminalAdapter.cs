using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Utility for exposing a Terminal as IOSafeTerminal by converting IOExceptions to RuntimeExceptions.
    /// This is a partial port of the Java class.
    /// </summary>
    public class IOSafeTerminalAdapter : IOSafeTerminal
    {
        private readonly Terminal _backend;

        public IOSafeTerminalAdapter(Terminal backend)
        {
            _backend = backend;
        }

        public static IOSafeTerminal CreateRuntimeExceptionAdapter(Terminal terminal)
            => new IOSafeTerminalAdapter(terminal);

        public void EnterPrivateMode() => _backend.EnterPrivateMode();
        public void ExitPrivateMode() => _backend.ExitPrivateMode();
        public void ClearScreen() => _backend.ClearScreen();
        public void SetCursorPosition(int x, int y) => _backend.SetCursorPosition(x, y);
        public void SetCursorPosition(TerminalPosition position) => _backend.SetCursorPosition(position);
        public TerminalPosition GetCursorPosition() => _backend.GetCursorPosition();
        public void SetCursorVisible(bool visible) => _backend.SetCursorVisible(visible);
        public void PutCharacter(char c) => _backend.PutCharacter(c);
        public void PutString(string text) => _backend.PutString(text);
        public ITextGraphics NewTextGraphics() => _backend.NewTextGraphics();
        public void EnableSGR(SGR sgr) => _backend.EnableSGR(sgr);
        public void DisableSGR(SGR sgr) => _backend.DisableSGR(sgr);
        public void ResetColorAndSGR() => _backend.ResetColorAndSGR();
        public void SetForegroundColor(ITextColor color) => _backend.SetForegroundColor(color);
        public void SetBackgroundColor(ITextColor color) => _backend.SetBackgroundColor(color);
        public void AddResizeListener(TerminalResizeListener listener) => _backend.AddResizeListener(listener);
        public void RemoveResizeListener(TerminalResizeListener listener) => _backend.RemoveResizeListener(listener);
        public TerminalSize GetTerminalSize() => _backend.GetTerminalSize();
        public byte[] EnquireTerminal(int timeoutMilliseconds) => _backend.EnquireTerminal(timeoutMilliseconds);
        public void Bell() => _backend.Bell();
        public void Flush() => _backend.Flush();
        public KeyStroke PollInput() => _backend.PollInput();
        public KeyStroke ReadInput() => _backend.ReadInput();
        public void Dispose() => _backend.Dispose();
    }
}
