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
/// Terminal dimensions in 2-d space, measured in number of rows and columns. This class is immutable and cannot change
/// its internal state after creation.
/// </summary>
public readonly struct TerminalSize : IEquatable<TerminalSize>
{
    public static readonly TerminalSize Zero = new(0, 0);
    public static readonly TerminalSize One = new(1, 1);

    /// <summary>
    /// Width, in number of columns
    /// </summary>
    public int Columns { get; }
    
    /// <summary>
    /// Height, in number of rows
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Creates a new terminal size representation with a given width (columns) and height (rows)
    /// </summary>
    /// <param name="columns">Width, in number of columns</param>
    /// <param name="rows">Height, in number of rows</param>
    /// <exception cref="ArgumentException">Thrown when columns or rows are negative</exception>
    public TerminalSize(int columns, int rows)
    {
        if (columns < 0 || rows < 0)
            throw new ArgumentException($"TerminalSize dimensions cannot be less than 0: [columns: {columns}, rows: {rows}]");

        Columns = columns;
        Rows = rows;
    }

    /// <summary>
    /// Creates a new size based on this size, but with a different width
    /// </summary>
    /// <param name="columns">Width of the new size, in columns</param>
    /// <returns>New size based on this one, but with a new width</returns>
    public TerminalSize WithColumns(int columns)
    {
        if (Columns == columns)
            return this;
        if (columns == 0 && Rows == 0)
            return Zero;
        return new TerminalSize(columns, Rows);
    }

    /// <summary>
    /// Creates a new size based on this size, but with a different height
    /// </summary>
    /// <param name="rows">Height of the new size, in rows</param>
    /// <returns>New size based on this one, but with a new height</returns>
    public TerminalSize WithRows(int rows)
    {
        if (Rows == rows)
            return this;
        if (rows == 0 && Columns == 0)
            return Zero;
        return new TerminalSize(Columns, rows);
    }

    /// <summary>
    /// Creates a new TerminalSize object representing a size with the same number of rows, but with a column size offset by a
    /// supplied value. Calling this method with delta 0 will return this, calling it with a positive delta will return
    /// a terminal size <i>delta</i> number of columns wider and for negative numbers shorter.
    /// </summary>
    /// <param name="delta">Column offset</param>
    /// <returns>New terminal size based off this one but with an applied transformation</returns>
    public TerminalSize WithRelativeColumns(int delta)
    {
        if (delta == 0)
            return this;
        // Prevent going below 0 (which would throw an exception)
        return WithColumns(Math.Max(0, Columns + delta));
    }

    /// <summary>
    /// Creates a new TerminalSize object representing a size with the same number of columns, but with a row size offset by a
    /// supplied value. Calling this method with delta 0 will return this, calling it with a positive delta will return
    /// a terminal size <i>delta</i> number of rows longer and for negative numbers shorter.
    /// </summary>
    /// <param name="delta">Row offset</param>
    /// <returns>New terminal size based off this one but with an applied transformation</returns>
    public TerminalSize WithRelativeRows(int delta)
    {
        if (delta == 0)
            return this;
        // Prevent going below 0 (which would throw an exception)
        return WithRows(Math.Max(0, Rows + delta));
    }

    /// <summary>
    /// Creates a new TerminalSize object representing a size based on this object's size but with a delta applied.
    /// This is the same as calling
    /// <code>WithRelativeColumns(delta.Columns).WithRelativeRows(delta.Rows)</code>
    /// </summary>
    /// <param name="delta">Column and row offset</param>
    /// <returns>New terminal size based off this one but with an applied resize</returns>
    public TerminalSize WithRelative(TerminalSize delta)
    {
        return WithRelative(delta.Columns, delta.Rows);
    }

    /// <summary>
    /// Creates a new TerminalSize object representing a size based on this object's size but with a delta applied.
    /// This is the same as calling
    /// <code>WithRelativeColumns(deltaColumns).WithRelativeRows(deltaRows)</code>
    /// </summary>
    /// <param name="deltaColumns">How many extra columns the new TerminalSize will have (negative values are allowed)</param>
    /// <param name="deltaRows">How many extra rows the new TerminalSize will have (negative values are allowed)</param>
    /// <returns>New terminal size based off this one but with an applied resize</returns>
    public TerminalSize WithRelative(int deltaColumns, int deltaRows)
    {
        return WithRelativeRows(deltaRows).WithRelativeColumns(deltaColumns);
    }

    /// <summary>
    /// Takes a different TerminalSize and returns a new TerminalSize that has the largest dimensions of the two,
    /// measured separately. So calling 3x5 on a 5x3 will return 5x5.
    /// </summary>
    /// <param name="other">Other TerminalSize to compare with</param>
    /// <returns>TerminalSize that combines the maximum width between the two and the maximum height</returns>
    public TerminalSize Max(TerminalSize other)
    {
        return WithColumns(Math.Max(Columns, other.Columns))
               .WithRows(Math.Max(Rows, other.Rows));
    }

    /// <summary>
    /// Takes a different TerminalSize and returns a new TerminalSize that has the smallest dimensions of the two,
    /// measured separately. So calling 3x5 on a 5x3 will return 3x3.
    /// </summary>
    /// <param name="other">Other TerminalSize to compare with</param>
    /// <returns>TerminalSize that combines the minimum width between the two and the minimum height</returns>
    public TerminalSize Min(TerminalSize other)
    {
        return WithColumns(Math.Min(Columns, other.Columns))
               .WithRows(Math.Min(Rows, other.Rows));
    }

    /// <summary>
    /// Returns itself if it is equal to the supplied size, otherwise the supplied size. You can use this if you have a
    /// size field which is frequently recalculated but often resolves to the same size; it will keep the same object
    /// in memory instead of swapping it out every cycle.
    /// </summary>
    /// <param name="size">Size you want to return</param>
    /// <returns>Itself if this size equals the size passed in, otherwise the size passed in</returns>
    public TerminalSize With(TerminalSize size)
    {
        return Equals(size) ? this : size;
    }

    public override string ToString()
    {
        return $"{{{Columns}x{Rows}}}";
    }

    public bool Equals(TerminalSize other)
    {
        return Columns == other.Columns && Rows == other.Rows;
    }

    public override bool Equals(object? obj)
    {
        return obj is TerminalSize other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Columns, Rows);
    }

    public static bool operator ==(TerminalSize left, TerminalSize right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TerminalSize left, TerminalSize right)
    {
        return !left.Equals(right);
    }
}