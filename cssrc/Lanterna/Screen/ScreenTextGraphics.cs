using Lanterna.Graphics;
using Lanterna.Terminal;

namespace Lanterna.Screen
{
    class ScreenTextGraphics : AbstractTextGraphics
    {
        private readonly Screen _screen;

        public ScreenTextGraphics(Screen screen)
            : base()
        {
            _screen = screen;
        }

        public override TextGraphics SetCharacter(int columnIndex, int rowIndex, TextCharacter textCharacter)
        {
            _screen.SetCharacter(columnIndex, rowIndex, textCharacter);
            return this;
        }

        public override TextCharacter GetCharacter(int column, int row)
        {
            return _screen.GetBackCharacter(column, row);
        }

        public override TerminalSize GetSize()
        {
            return _screen.GetTerminalSize();
        }
    }
}
