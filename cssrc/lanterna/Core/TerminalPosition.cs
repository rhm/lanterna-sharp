/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */

namespace Lanterna.Core;

/// <summary>
/// A 2-d position in 'terminal space'. Please note that the coordinates are 0-indexed, meaning 0x0 is the top left
/// corner of the terminal. This object is immutable so you cannot change it after it has been created. Instead, you
/// can easily create modified 'clones' by using the 'With' methods.
/// </summary>
public readonly struct TerminalPosition : IComparable<TerminalPosition>, IEquatable<TerminalPosition>
{
    /// <summary>
    /// Constant for the top-left corner (0x0)
    /// </summary>
    public static readonly TerminalPosition TopLeftCorner = new(0, 0);
    
    /// <summary>
    /// Constant for the 1x1 position (one offset in both directions from top-left)
    /// </summary>
    public static readonly TerminalPosition Offset1X1 = new(1, 1);

    /// <summary>
    /// Index of the row this position is representing, zero indexed (the first row has index 0)
    /// </summary>
    public int Row { get; }
    
    /// <summary>
    /// Index of the column this position is representing, zero indexed (the first column has index 0).
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Creates a new TerminalPosition object, which represents a location on the screen. There is no check to verify
    /// that the position you specified is within the size of the current terminal and you can specify negative positions
    /// as well.
    /// </summary>
    /// <param name="column">Column of the location, or the "x" coordinate, zero indexed (the first column is 0)</param>
    /// <param name="row">Row of the location, or the "y" coordinate, zero indexed (the first row is 0)</param>
    public TerminalPosition(int column, int row)
    {
        Row = row;
        Column = column;
    }

    /// <summary>
    /// Creates a new TerminalPosition object representing a position with the same column index as this but with a
    /// supplied row index.
    /// </summary>
    /// <param name="row">Index of the row for the new position</param>
    /// <returns>A TerminalPosition object with the same column as this but with a specified row index</returns>
    public TerminalPosition WithRow(int row)
    {
        if (row == 0 && Column == 0)
            return TopLeftCorner;
        return new TerminalPosition(Column, row);
    }

    /// <summary>
    /// Creates a new TerminalPosition object representing a position with the same row index as this but with a
    /// supplied column index.
    /// </summary>
    /// <param name="column">Index of the column for the new position</param>
    /// <returns>A TerminalPosition object with the same row as this but with a specified column index</returns>
    public TerminalPosition WithColumn(int column)
    {
        if (column == 0 && Row == 0)
            return TopLeftCorner;
        return new TerminalPosition(column, Row);
    }

    /// <summary>
    /// Creates a new TerminalPosition object representing a position on the same row, but with a column offset by a
    /// supplied value. Calling this method with delta 0 will return this, calling it with a positive delta will return
    /// a terminal position <i>delta</i> number of columns to the right and for negative numbers the same to the left.
    /// </summary>
    /// <param name="delta">Column offset</param>
    /// <returns>New terminal position based off this one but with an applied offset</returns>
    public TerminalPosition WithRelativeColumn(int delta)
    {
        if (delta == 0)
            return this;
        return WithColumn(Column + delta);
    }

    /// <summary>
    /// Creates a new TerminalPosition object representing a position on the same column, but with a row offset by a
    /// supplied value. Calling this method with delta 0 will return this, calling it with a positive delta will return
    /// a terminal position <i>delta</i> number of rows to the down and for negative numbers the same up.
    /// </summary>
    /// <param name="delta">Row offset</param>
    /// <returns>New terminal position based off this one but with an applied offset</returns>
    public TerminalPosition WithRelativeRow(int delta)
    {
        if (delta == 0)
            return this;
        return WithRow(Row + delta);
    }

    /// <summary>
    /// Creates a new TerminalPosition object that is 'translated' by an amount of rows and columns specified by another
    /// TerminalPosition. Same as calling
    /// <code>WithRelativeRow(translate.Row).WithRelativeColumn(translate.Column)</code>
    /// </summary>
    /// <param name="translate">How many columns and rows to translate</param>
    /// <returns>New TerminalPosition that is the result of the original with added translation</returns>
    public TerminalPosition WithRelative(TerminalPosition translate)
    {
        return WithRelative(translate.Column, translate.Row);
    }

    /// <summary>
    /// Creates a new TerminalPosition object that is 'translated' by an amount of rows and columns specified by the two
    /// parameters. Same as calling
    /// <code>WithRelativeRow(deltaRow).WithRelativeColumn(deltaColumn)</code>
    /// </summary>
    /// <param name="deltaColumn">How many columns to move from the current position in the new TerminalPosition</param>
    /// <param name="deltaRow">How many rows to move from the current position in the new TerminalPosition</param>
    /// <returns>New TerminalPosition that is the result of the original position with added translation</returns>
    public TerminalPosition WithRelative(int deltaColumn, int deltaRow)
    {
        return WithRelativeRow(deltaRow).WithRelativeColumn(deltaColumn);
    }

    /// <summary>
    /// Returns itself if it is equal to the supplied position, otherwise the supplied position. You can use this if you
    /// have a position field which is frequently recalculated but often resolves to the same; it will keep the same
    /// object in memory instead of swapping it out every cycle.
    /// </summary>
    /// <param name="position">Position you want to return</param>
    /// <returns>Itself if this position equals the position passed in, otherwise the position passed in</returns>
    public TerminalPosition With(TerminalPosition position)
    {
        return Equals(position) ? this : position;
    }

    public static TerminalPosition operator +(TerminalPosition a, TerminalPosition b)
    {
        return a.WithRelative(b);
    }

    public static TerminalPosition operator -(TerminalPosition a, TerminalPosition b)
    {
        return a.WithRelative(-b.Column, -b.Row);
    }

    public static TerminalPosition operator *(TerminalPosition a, TerminalPosition b)
    {
        return new TerminalPosition(a.Column * b.Column, a.Row * b.Row);
    }

    public static TerminalPosition operator /(TerminalPosition a, TerminalPosition b)
    {
        return new TerminalPosition(a.Column / b.Column, a.Row / b.Row);
    }

    public TerminalPosition Abs()
    {
        int x = Math.Abs(Column);
        int y = Math.Abs(Row);
        return new TerminalPosition(x, y);
    }

    public TerminalPosition Min(TerminalPosition position)
    {
        int x = Math.Min(Column, position.Column);
        int y = Math.Min(Row, position.Row);
        return new TerminalPosition(x, y);
    }

    public TerminalPosition Max(TerminalPosition position)
    {
        int x = Math.Max(Column, position.Column);
        int y = Math.Max(Row, position.Row);
        return new TerminalPosition(x, y);
    }

    public int CompareTo(TerminalPosition other)
    {
        if (Row < other.Row)
            return -1;
        if (Row == other.Row)
        {
            if (Column < other.Column)
                return -1;
            if (Column == other.Column)
                return 0;
        }
        return 1;
    }

    public override string ToString()
    {
        return $"TerminalPosition{{column={Column}, row={Row}}}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column);
    }

    public bool Equals(int columnIndex, int rowIndex)
    {
        return Column == columnIndex && Row == rowIndex;
    }

    public bool Equals(TerminalPosition other)
    {
        return Row == other.Row && Column == other.Column;
    }

    public override bool Equals(object? obj)
    {
        return obj is TerminalPosition other && Equals(other);
    }

    public static bool operator ==(TerminalPosition left, TerminalPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TerminalPosition left, TerminalPosition right)
    {
        return !left.Equals(right);
    }
}