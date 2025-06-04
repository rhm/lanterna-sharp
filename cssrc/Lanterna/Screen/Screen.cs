using System;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Terminal;

namespace Lanterna.Screen
{
    public interface Screen : IInputProvider, Scrollable, IDisposable
    {
        void StartScreen();
        void StopScreen();
        void Clear();

        TerminalPosition? GetCursorPosition();
        void SetCursorPosition(TerminalPosition? position);

        TabBehaviour GetTabBehaviour();
        void SetTabBehaviour(TabBehaviour tabBehaviour);

        TerminalSize GetTerminalSize();

        void SetCharacter(int column, int row, TextCharacter screenCharacter);
        void SetCharacter(TerminalPosition position, TextCharacter screenCharacter);

        TextGraphics NewTextGraphics();

        TextCharacter GetFrontCharacter(int column, int row);
        TextCharacter GetFrontCharacter(TerminalPosition position);
        TextCharacter GetBackCharacter(int column, int row);
        TextCharacter GetBackCharacter(TerminalPosition position);

        void Refresh();
        void Refresh(RefreshType refreshType);

        TerminalSize? DoResizeIfNecessary();

        new void ScrollLines(int firstLine, int lastLine, int distance);

        public enum RefreshType
        {
            Automatic,
            Delta,
            Complete
        }
    }
}
