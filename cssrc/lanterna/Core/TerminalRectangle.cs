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
 * Copyright (C) 2010-2024 Martin Berglund
 */

namespace Lanterna.Core;

/// <summary>
/// This class is immutable and cannot change its internal state after creation.
/// </summary>
public readonly struct TerminalRectangle : IEquatable<TerminalRectangle>
{
    /// <summary>
    /// Position of the top-left corner of the rectangle
    /// </summary>
    public TerminalPosition Position { get; }
    
    /// <summary>
    /// Size of the rectangle
    /// </summary>
    public TerminalSize Size { get; }
    
    /// <summary>
    /// X coordinate (column) of the left edge
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// Y coordinate (row) of the top edge
    /// </summary>
    public int Y { get; }
    
    /// <summary>
    /// Width of the rectangle in columns
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// Height of the rectangle in rows
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// X coordinate of the right edge (X + Width)
    /// </summary>
    public int XAndWidth { get; }
    
    /// <summary>
    /// Y coordinate of the bottom edge (Y + Height)
    /// </summary>
    public int YAndHeight { get; }

    /// <summary>
    /// Creates a new terminal rect representation at the supplied x y position with the supplied width and height.
    /// Both width and height must be at least zero (non negative) as checked in TerminalSize.
    /// </summary>
    /// <param name="x">X coordinate (column) of the left edge</param>
    /// <param name="y">Y coordinate (row) of the top edge</param>
    /// <param name="width">number of columns</param>
    /// <param name="height">number of rows</param>
    public TerminalRectangle(int x, int y, int width, int height)
    {
        Position = new TerminalPosition(x, y);
        Size = new TerminalSize(width, height);
        
        X = x;
        Y = y;
        Width = width;
        Height = height;
        XAndWidth = x + width;
        YAndHeight = y + height;
    }

    /// <summary>
    /// Creates a new terminal rectangle from a position and size
    /// </summary>
    /// <param name="position">Top-left position</param>
    /// <param name="size">Size of the rectangle</param>
    public TerminalRectangle(TerminalPosition position, TerminalSize size)
        : this(position.Column, position.Row, size.Columns, size.Rows)
    {
    }

    /// <summary>
    /// Returns the width of this rect, in number of columns
    /// </summary>
    public int Columns => Width;

    /// <summary>
    /// Returns the height of this rect representation, in number of rows
    /// </summary>
    public int Rows => Height;

    /// <summary>
    /// Creates a new rect based on this rect, but with a different width
    /// </summary>
    /// <param name="columns">Width of the new rect, in columns</param>
    /// <returns>New rect based on this one, but with a new width</returns>
    public TerminalRectangle WithColumns(int columns)
    {
        return new TerminalRectangle(X, Y, columns, Height);
    }

    /// <summary>
    /// Creates a new rect based on this rect, but with a different height
    /// </summary>
    /// <param name="rows">Height of the new rect, in rows</param>
    /// <returns>New rect based on this one, but with a new height</returns>
    public TerminalRectangle WithRows(int rows)
    {
        return new TerminalRectangle(X, Y, Width, rows);
    }

    /// <summary>
    /// Executes an action if the rectangle contains the specified position
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <param name="action">Action to execute if position is contained</param>
    /// <returns>True if the position was contained and the action was executed</returns>
    public bool WhenContains(TerminalPosition position, Action action)
    {
        return WhenContains(position.Column, position.Row, action);
    }

    /// <summary>
    /// Executes an action if the rectangle contains the specified coordinates
    /// </summary>
    /// <param name="x">X coordinate to check</param>
    /// <param name="y">Y coordinate to check</param>
    /// <param name="action">Action to execute if coordinates are contained</param>
    /// <returns>True if the coordinates were contained and the action was executed</returns>
    public bool WhenContains(int x, int y, Action action)
    {
        if (X <= x && x < XAndWidth && Y <= y && y < YAndHeight)
        {
            action();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the rectangle contains the specified position
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if the position is contained within the rectangle</returns>
    public bool Contains(TerminalPosition position)
    {
        return Contains(position.Column, position.Row);
    }

    /// <summary>
    /// Checks if the rectangle contains the specified coordinates
    /// </summary>
    /// <param name="x">X coordinate to check</param>
    /// <param name="y">Y coordinate to check</param>
    /// <returns>True if the coordinates are contained within the rectangle</returns>
    public bool Contains(int x, int y)
    {
        return X <= x && x < XAndWidth && Y <= y && y < YAndHeight;
    }

    public override string ToString()
    {
        return $"{{x: {X}, y: {Y}, width: {Width}, height: {Height}}}";
    }

    public bool Equals(TerminalRectangle other)
    {
        return Position.Equals(other.Position) && Size.Equals(other.Size);
    }

    public override bool Equals(object? obj)
    {
        return obj is TerminalRectangle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, Size);
    }

    public static bool operator ==(TerminalRectangle left, TerminalRectangle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TerminalRectangle left, TerminalRectangle right)
    {
        return !left.Equals(right);
    }
}