using System;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Represents a coordinate in terminal space.
    /// </summary>
    public struct TerminalPosition : IComparable<TerminalPosition>
    {
        public static readonly TerminalPosition TopLeftCorner = new TerminalPosition(0, 0);
        public static readonly TerminalPosition Offset1x1 = new TerminalPosition(1, 1);

        public int Column { get; }
        public int Row { get; }

        public TerminalPosition(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int CompareTo(TerminalPosition other)
        {
            var cmp = Row.CompareTo(other.Row);
            if (cmp != 0)
                return cmp;
            return Column.CompareTo(other.Column);
        }

        public TerminalPosition WithRow(int row) => new TerminalPosition(Column, row);
        public TerminalPosition WithColumn(int column) => new TerminalPosition(column, Row);
    }
}
