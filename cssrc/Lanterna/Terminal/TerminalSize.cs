using System;

namespace Lanterna.Terminal
{
    /// <summary>
    /// Represents the dimensions of the terminal in rows and columns.
    /// </summary>
    public struct TerminalSize
    {
        public static readonly TerminalSize Zero = new TerminalSize(0, 0);
        public static readonly TerminalSize One = new TerminalSize(1, 1);

        public int Columns { get; }
        public int Rows { get; }

        public TerminalSize(int columns, int rows)
        {
            if (columns < 0 || rows < 0)
                throw new ArgumentException("Dimensions cannot be negative");
            Columns = columns;
            Rows = rows;
        }

        public TerminalSize WithColumns(int columns) => new TerminalSize(columns, Rows);
        public TerminalSize WithRows(int rows) => new TerminalSize(Columns, rows);
    }
}
