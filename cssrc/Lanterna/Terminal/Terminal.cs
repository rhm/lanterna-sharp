using System;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Low level terminal interface similar to the Java version.
    /// </summary>
    public interface Terminal : IInputProvider, IDisposable
    {
        void EnterPrivateMode();
        void ExitPrivateMode();
        void ClearScreen();
        void SetCursorPosition(int x, int y);
        void SetCursorPosition(TerminalPosition position);
        TerminalPosition GetCursorPosition();
        void SetCursorVisible(bool visible);
        void PutCharacter(char c);
        void PutString(string text);
        Lanterna.Graphics.ITextGraphics NewTextGraphics();
        void EnableSGR(SGR sgr);
        void DisableSGR(SGR sgr);
        void ResetColorAndSGR();
        void SetForegroundColor(ITextColor color);
        void SetBackgroundColor(ITextColor color);
        void AddResizeListener(TerminalResizeListener listener);
        void RemoveResizeListener(TerminalResizeListener listener);
        TerminalSize GetTerminalSize();
        byte[] EnquireTerminal(int timeoutMilliseconds);
        void Bell();
        void Flush();
    }
}
