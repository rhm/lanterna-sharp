using Lanterna.Graphics;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Simplified TextGraphics implementation that delegates to a Terminal.
    /// This is only a minimal placeholder for the full Java class.
    /// </summary>
    internal class TerminalTextGraphics : ITextGraphics
    {
        private readonly Terminal _terminal;

        public TerminalTextGraphics(Terminal terminal)
        {
            _terminal = terminal;
            Size = terminal.GetTerminalSize();
        }

        public TerminalSize Size { get; }

        public void SetCharacter(int column, int row, char c)
        {
            _terminal.SetCursorPosition(column, row);
            _terminal.PutCharacter(c);
        }
    }
}
