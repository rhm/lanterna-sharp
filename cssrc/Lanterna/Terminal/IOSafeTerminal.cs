using Lanterna.Input;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Variant of Terminal that doesn't throw IOExceptions.
    /// </summary>
    public interface IOSafeTerminal : Terminal
    {
        new void EnterPrivateMode();
        new void ExitPrivateMode();
        new void ClearScreen();
        new void SetCursorPosition(int x, int y);
        new void SetCursorPosition(TerminalPosition position);
        new TerminalPosition GetCursorPosition();
        new void SetCursorVisible(bool visible);
        new void PutCharacter(char c);
        new void PutString(string text);
        new Lanterna.Graphics.ITextGraphics NewTextGraphics();
        new void EnableSGR(SGR sgr);
        new void DisableSGR(SGR sgr);
        new void ResetColorAndSGR();
        new void SetForegroundColor(ITextColor color);
        new void SetBackgroundColor(ITextColor color);
        new TerminalSize GetTerminalSize();
        new byte[] EnquireTerminal(int timeoutMilliseconds);
        new void Bell();
        new void Flush();
        new KeyStroke PollInput();
        new KeyStroke ReadInput();
    }
}
