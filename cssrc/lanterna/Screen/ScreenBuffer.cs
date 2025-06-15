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

using Lanterna.Core;

namespace Lanterna.Screen;

/// <summary>
/// Defines a buffer used by AbstractScreen and its subclasses to keep its state of what's currently displayed and what 
/// the edit buffer looks like. A ScreenBuffer is essentially a two-dimensional array of TextCharacter with some utility
/// methods to inspect and manipulate it in a safe way.
/// </summary>
public class ScreenBuffer
{
    private TextCharacter[,] _buffer;
    private TerminalSize _size;

    /// <summary>
    /// Creates a new ScreenBuffer with a given size and a TextCharacter to initially fill it with
    /// </summary>
    /// <param name="size">Size of the buffer</param>
    /// <param name="filler">What character to set as the initial content of the buffer</param>
    public ScreenBuffer(TerminalSize size, TextCharacter filler)
    {
        _size = size;
        if (size.Columns <= 0 || size.Rows <= 0)
            throw new ArgumentException("Size must have positive dimensions", nameof(size));
        _buffer = new TextCharacter[size.Rows, size.Columns];
        
        // Fill the buffer with the filler character
        Fill(filler);
    }

    /// <summary>
    /// Gets the size of this screen buffer
    /// </summary>
    public TerminalSize Size => _size;

    /// <summary>
    /// Fills the entire buffer with the specified character
    /// </summary>
    /// <param name="filler">Character to fill the buffer with</param>
    public void Fill(TextCharacter filler)
    {
        for (int row = 0; row < _size.Rows; row++)
        {
            for (int col = 0; col < _size.Columns; col++)
            {
                _buffer[row, col] = filler;
            }
        }
    }

    /// <summary>
    /// Creates a new ScreenBuffer with a different size, copying existing content where possible
    /// </summary>
    /// <param name="newSize">New size for the buffer</param>
    /// <param name="filler">Character to use for new areas</param>
    /// <returns>New ScreenBuffer with the specified size</returns>
    public ScreenBuffer Resize(TerminalSize newSize, TextCharacter filler)
    {
        var newBuffer = new ScreenBuffer(newSize, filler);

        // Copy existing content where it fits
        int copyRows = Math.Min(_size.Rows, newSize.Rows);
        int copyCols = Math.Min(_size.Columns, newSize.Columns);

        for (int row = 0; row < copyRows; row++)
        {
            for (int col = 0; col < copyCols; col++)
            {
                newBuffer._buffer[row, col] = _buffer[row, col];
            }
        }

        return newBuffer;
    }

    /// <summary>
    /// Gets the character at the specified position
    /// </summary>
    /// <param name="column">Column (x coordinate)</param>
    /// <param name="row">Row (y coordinate)</param>
    /// <returns>The TextCharacter at the specified position</returns>
    public TextCharacter GetCharacterAt(int column, int row)
    {
        if (column < 0 || column >= _size.Columns || row < 0 || row >= _size.Rows)
        {
            throw new ArgumentOutOfRangeException($"Position [{column}:{row}] is outside buffer bounds {_size}");
        }
        
        return _buffer[row, column];
    }

    /// <summary>
    /// Gets the character at the specified position
    /// </summary>
    /// <param name="position">Position to get the character from</param>
    /// <returns>The TextCharacter at the specified position</returns>
    public TextCharacter GetCharacterAt(TerminalPosition position)
    {
        return GetCharacterAt(position.Column, position.Row);
    }

    /// <summary>
    /// Sets the character at the specified position
    /// </summary>
    /// <param name="column">Column (x coordinate)</param>
    /// <param name="row">Row (y coordinate)</param>
    /// <param name="character">Character to set at the position</param>
    public void SetCharacterAt(int column, int row, TextCharacter character)
    {
        if (column < 0 || column >= _size.Columns || row < 0 || row >= _size.Rows)
        {
            // Silently ignore out-of-bounds writes like the Java version
            return;
        }
        
        _buffer[row, column] = character ?? throw new ArgumentNullException(nameof(character));
    }

    /// <summary>
    /// Sets the character at the specified position
    /// </summary>
    /// <param name="position">Position to set the character at</param>
    /// <param name="character">Character to set at the position</param>
    public void SetCharacterAt(TerminalPosition position, TextCharacter character)
    {
        SetCharacterAt(position.Column, position.Row, character);
    }

    /// <summary>
    /// Checks if this buffer is very different from another buffer, using a threshold for the number of differences
    /// </summary>
    /// <param name="other">Other buffer to compare with</param>
    /// <param name="threshold">Maximum number of differences before considering the buffers "very different"</param>
    /// <returns>True if the buffers differ in more than the threshold number of positions</returns>
    public bool IsVeryDifferent(ScreenBuffer other, int threshold)
    {
        if (!_size.Equals(other._size))
        {
            throw new ArgumentException("Can only call IsVeryDifferent comparing two ScreenBuffers of the same size! This is probably a bug in Lanterna.");
        }

        int differences = 0;
        for (int row = 0; row < _size.Rows; row++)
        {
            for (int col = 0; col < _size.Columns; col++)
            {
                if (!_buffer[row, col].Equals(other._buffer[row, col]))
                {
                    if (++differences >= threshold)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// Copies a region from this buffer to another ScreenBuffer
    /// </summary>
    /// <param name="destination">Destination buffer to copy to</param>
    /// <param name="sourceStartRow">Starting row in this buffer</param>
    /// <param name="sourceRowCount">Number of rows to copy</param>
    /// <param name="sourceStartColumn">Starting column in this buffer</param>
    /// <param name="sourceColumnCount">Number of columns to copy</param>
    /// <param name="destStartRow">Starting row in the destination buffer to copy to</param>
    /// <param name="destStartColumn">Starting column in the destination buffer to copy to</param>
    public void CopyTo(ScreenBuffer destination, int sourceStartRow, int sourceRowCount, int sourceStartColumn, int sourceColumnCount, int destStartRow, int destStartColumn)
    {
        destination.CopyFrom(this, sourceStartRow, sourceRowCount, sourceStartColumn, sourceColumnCount, destStartRow, destStartColumn);
    }

    /// <summary>
    /// Copies a region from another ScreenBuffer into this buffer
    /// </summary>
    /// <param name="source">Source buffer to copy from</param>
    /// <param name="sourceStartRow">Starting row in the source buffer</param>
    /// <param name="sourceRowCount">Number of rows to copy</param>
    /// <param name="sourceStartColumn">Starting column in the source buffer</param>
    /// <param name="sourceColumnCount">Number of columns to copy</param>
    /// <param name="destStartRow">Starting row in this buffer to copy to</param>
    /// <param name="destStartColumn">Starting column in this buffer to copy to</param>
    public void CopyFrom(ScreenBuffer source, int sourceStartRow, int sourceRowCount, int sourceStartColumn, int sourceColumnCount, int destStartRow, int destStartColumn)
    {
        for (int row = 0; row < sourceRowCount; row++)
        {
            for (int col = 0; col < sourceColumnCount; col++)
            {
                int sourceRow = sourceStartRow + row;
                int sourceCol = sourceStartColumn + col;
                int destRow = destStartRow + row;
                int destCol = destStartColumn + col;

                // Check bounds for both source and destination
                if (sourceRow >= 0 && sourceRow < source._size.Rows &&
                    sourceCol >= 0 && sourceCol < source._size.Columns &&
                    destRow >= 0 && destRow < _size.Rows &&
                    destCol >= 0 && destCol < _size.Columns)
                {
                    _buffer[destRow, destCol] = source._buffer[sourceRow, sourceCol];
                }
            }
        }
    }

    /// <summary>
    /// Scrolls the lines in this buffer vertically
    /// </summary>
    /// <param name="firstLine">First line to scroll (0-indexed)</param>
    /// <param name="lastLine">Last line to scroll (0-indexed)</param>
    /// <param name="distance">Distance to scroll (positive scrolls down, negative scrolls up)</param>
    /// <param name="fillCharacter">Character to use for newly exposed areas</param>
    public void ScrollLines(int firstLine, int lastLine, int distance, TextCharacter fillCharacter)
    {
        if (distance == 0 || firstLine < 0 || lastLine >= _size.Rows || firstLine > lastLine)
            return;

        if (distance > 0)
        {
            // Scroll down - move lines up in array
            for (int row = firstLine; row <= lastLine - distance; row++)
            {
                for (int col = 0; col < _size.Columns; col++)
                {
                    _buffer[row, col] = _buffer[row + distance, col];
                }
            }
            
            // Fill the bottom with the fill character
            for (int row = Math.Max(firstLine, lastLine - distance + 1); row <= lastLine; row++)
            {
                for (int col = 0; col < _size.Columns; col++)
                {
                    _buffer[row, col] = fillCharacter;
                }
            }
        }
        else
        {
            // Scroll up - move lines down in array
            distance = -distance;
            for (int row = lastLine; row >= firstLine + distance; row--)
            {
                for (int col = 0; col < _size.Columns; col++)
                {
                    _buffer[row, col] = _buffer[row - distance, col];
                }
            }
            
            // Fill the top with the fill character
            for (int row = firstLine; row < Math.Min(lastLine + 1, firstLine + distance); row++)
            {
                for (int col = 0; col < _size.Columns; col++)
                {
                    _buffer[row, col] = fillCharacter;
                }
            }
        }
    }
}