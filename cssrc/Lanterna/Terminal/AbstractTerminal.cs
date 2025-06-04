using System.Collections.Generic;
using Lanterna.Graphics;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Base class with resize listener management and default TextGraphics implementation.
    /// </summary>
    public abstract class AbstractTerminal : Terminal
    {
        private readonly List<TerminalResizeListener> _resizeListeners = new();
        private TerminalSize? _lastKnownSize;

        public virtual void AddResizeListener(TerminalResizeListener listener)
        {
            if (listener != null)
                _resizeListeners.Add(listener);
        }

        public virtual void RemoveResizeListener(TerminalResizeListener listener)
        {
            if (listener != null)
                _resizeListeners.Remove(listener);
        }

        protected void OnResized(TerminalSize size)
        {
            if (_lastKnownSize == null || !_lastKnownSize.Equals(size))
            {
                _lastKnownSize = size;
                foreach (var l in _resizeListeners)
                    l.OnResized(this, size);
            }
        }

        public virtual ITextGraphics NewTextGraphics()
        {
            return new TerminalTextGraphics(this);
        }

        // Other Terminal methods remain abstract
        public abstract void EnterPrivateMode();
        public abstract void ExitPrivateMode();
        public abstract void ClearScreen();
        public abstract void SetCursorPosition(int x, int y);
        public abstract void SetCursorPosition(TerminalPosition position);
        public abstract TerminalPosition GetCursorPosition();
        public abstract void SetCursorVisible(bool visible);
        public abstract void PutCharacter(char c);
        public abstract void PutString(string text);
        public abstract void EnableSGR(SGR sgr);
        public abstract void DisableSGR(SGR sgr);
        public abstract void ResetColorAndSGR();
        public abstract void SetForegroundColor(ITextColor color);
        public abstract void SetBackgroundColor(ITextColor color);
        public abstract TerminalSize GetTerminalSize();
        public abstract byte[] EnquireTerminal(int timeoutMilliseconds);
        public abstract void Bell();
        public abstract void Flush();
        public abstract Lanterna.Input.KeyStroke PollInput();
        public abstract Lanterna.Input.KeyStroke ReadInput();
        public abstract void Dispose();
    }
}
