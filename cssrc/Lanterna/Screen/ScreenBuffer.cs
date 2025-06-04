using Lanterna.Graphics;
using Lanterna.Terminal;

namespace Lanterna.Screen
{
    public class ScreenBuffer : TextImage
    {
        private readonly BasicTextImage _backend;

        public ScreenBuffer(TerminalSize size, TextCharacter filler)
        {
            _backend = new BasicTextImage(size, filler);
        }

        private ScreenBuffer(BasicTextImage backend)
        {
            _backend = backend;
        }

        public ScreenBuffer Resize(TerminalSize newSize, TextCharacter filler)
        {
            BasicTextImage resizedBackend = _backend.Resize(newSize, filler);
            return new ScreenBuffer(resizedBackend);
        }

        internal bool IsVeryDifferent(ScreenBuffer other, int threshold)
        {
            if (!_backend.GetSize().Equals(other._backend.GetSize()))
                throw new System.ArgumentException("Can only call isVeryDifferent comparing two ScreenBuffers of the same size! This is probably a bug in Lanterna.");
            int differences = 0;
            for (int y = 0; y < GetSize().Rows; y++)
            {
                for (int x = 0; x < GetSize().Columns; x++)
                {
                    if (!_backend.GetCharacterAt(x, y).Equals(other._backend.GetCharacterAt(x, y)))
                    {
                        if (++differences >= threshold)
                            return true;
                    }
                }
            }
            return false;
        }

        public TerminalSize GetSize() => _backend.GetSize();
        public TextCharacter GetCharacterAt(TerminalPosition position) => _backend.GetCharacterAt(position);
        public TextCharacter GetCharacterAt(int column, int row) => _backend.GetCharacterAt(column, row);
        public void SetCharacterAt(TerminalPosition position, TextCharacter character) => _backend.SetCharacterAt(position, character);
        public void SetCharacterAt(int column, int row, TextCharacter character) => _backend.SetCharacterAt(column, row, character);
        public void SetAll(TextCharacter character) => _backend.SetAll(character);
        public TextGraphics NewTextGraphics() => _backend.NewTextGraphics();
        public void CopyTo(TextImage destination)
        {
            if (destination is ScreenBuffer sb)
                destination = sb._backend;
            _backend.CopyTo(destination);
        }
        public void CopyTo(TextImage destination, int startRowIndex, int rows, int startColumnIndex, int columns, int destinationRowOffset, int destinationColumnOffset)
        {
            if (destination is ScreenBuffer sb)
                destination = sb._backend;
            _backend.CopyTo(destination, startRowIndex, rows, startColumnIndex, columns, destinationRowOffset, destinationColumnOffset);
        }
        public void CopyFrom(TextImage source, int startRowIndex, int rows, int startColumnIndex, int columns, int destinationRowOffset, int destinationColumnOffset)
        {
            source.CopyTo(_backend, startRowIndex, rows, startColumnIndex, columns, destinationRowOffset, destinationColumnOffset);
        }
        public void ScrollLines(int firstLine, int lastLine, int distance) => _backend.ScrollLines(firstLine, lastLine, distance);
        public override string ToString() => _backend.ToString();
    }
}
