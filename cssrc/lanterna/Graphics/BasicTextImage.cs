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
using Lanterna.Core;

namespace Lanterna.Graphics;

/// <summary>
/// Simple implementation of ITextImage that keeps the content as a two-dimensional TextCharacter array. 
/// Copy operations between two BasicTextImage classes are semi-optimized by using Array.Copy instead 
/// of iterating over each character and copying them over one by one.
/// </summary>
public class BasicTextImage : ITextImage
{
    private readonly TerminalSize _size;
    private readonly TextCharacter?[][] _buffer;

    /// <summary>
    /// Creates a new BasicTextImage with the specified size and fills it initially with space characters using the 
    /// default foreground and background color
    /// </summary>
    /// <param name="columns">Size of the image in number of columns</param>
    /// <param name="rows">Size of the image in number of rows</param>
    public BasicTextImage(int columns, int rows) : this(new TerminalSize(columns, rows))
    {
    }

    /// <summary>
    /// Creates a new BasicTextImage with the specified size and fills it initially with space characters using the 
    /// default foreground and background color
    /// </summary>
    /// <param name="size">Size to make the image</param>
    public BasicTextImage(TerminalSize size) : this(size, new TextCharacter?[0][], TextCharacter.DefaultCharacter)
    {
    }

    /// <summary>
    /// Creates a new BasicTextImage with a given size and a TextCharacter to initially fill it with
    /// </summary>
    /// <param name="size">Size of the image</param>
    /// <param name="initialContent">What character to set as the initial content</param>
    public BasicTextImage(TerminalSize size, TextCharacter initialContent) 
        : this(size, new TextCharacter?[0][], initialContent)
    {
    }

    /// <summary>
    /// Creates a new BasicTextImage by copying a region of a two-dimensional array of TextCharacter:s. 
    /// If the area to be copied to larger than the source array, a filler character is used.
    /// </summary>
    /// <param name="size">Size to create the new BasicTextImage as (and size to copy from the array)</param>
    /// <param name="toCopy">Array to copy initial data from</param>
    /// <param name="initialContent">Filler character to use if the source array is smaller than the requested size</param>
    private BasicTextImage(TerminalSize size, TextCharacter?[][] toCopy, TextCharacter initialContent)
    {
        if (size == null)
            throw new ArgumentNullException(nameof(size), "Cannot create BasicTextImage with null size");
        if (toCopy == null)
            throw new ArgumentNullException(nameof(toCopy), "Cannot create BasicTextImage with null toCopy");
        if (initialContent == null)
            throw new ArgumentNullException(nameof(initialContent), "Cannot create BasicTextImage with null filler");

        _size = size;

        int rows = size.Rows;
        int columns = size.Columns;
        _buffer = new TextCharacter?[rows][];
        
        for (int y = 0; y < rows; y++)
        {
            _buffer[y] = new TextCharacter?[columns];
            for (int x = 0; x < columns; x++)
            {
                if (y < toCopy.Length && x < toCopy[y].Length)
                {
                    _buffer[y][x] = toCopy[y][x];
                }
                else
                {
                    _buffer[y][x] = initialContent;
                }
            }
        }
    }

    public TerminalSize Size => _size;

    public void SetAll(TextCharacter character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character), "Cannot call BasicTextImage.SetAll(..) with null character");

        foreach (var line in _buffer)
        {
            Array.Fill(line, character);
        }
    }

    public ITextImage Resize(TerminalSize newSize, TextCharacter filler)
    {
        if (newSize == null)
            throw new ArgumentNullException(nameof(newSize), "Cannot resize BasicTextImage with null newSize");
        if (filler == null)
            throw new ArgumentNullException(nameof(filler), "Cannot resize BasicTextImage with null filler");

        if (newSize.Rows == _buffer.Length &&
            (_buffer.Length == 0 || newSize.Columns == _buffer[0].Length))
        {
            return this;
        }
        
        return new BasicTextImage(newSize, _buffer, filler);
    }

    public void SetCharacterAt(TerminalPosition position, TextCharacter character)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position), "Cannot call BasicTextImage.SetCharacterAt(..) with null position");
        
        SetCharacterAt(position.Column, position.Row, character);
    }

    public void SetCharacterAt(int column, int row, TextCharacter character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character), "Cannot call BasicTextImage.SetCharacterAt(..) with null character");
        
        if (column < 0 || row < 0 || row >= _buffer.Length || column >= _buffer[0].Length)
        {
            return;
        }

        // Double width character adjustments
        if (column > 0 && _buffer[row][column - 1]?.IsDoubleWidth == true)
        {
            _buffer[row][column - 1] = _buffer[row][column - 1]!.WithCharacter(' ');
        }

        // Assign the character at location we specified
        _buffer[row][column] = character;

        // Double width character adjustments
        if (character.IsDoubleWidth && column + 1 < _buffer[0].Length)
        {
            _buffer[row][column + 1] = character.WithCharacter(' ');
        }
    }

    public TextCharacter? GetCharacterAt(TerminalPosition position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position), "Cannot call BasicTextImage.GetCharacterAt(..) with null position");
        
        return GetCharacterAt(position.Column, position.Row);
    }

    public TextCharacter? GetCharacterAt(int column, int row)
    {
        if (column < 0 || row < 0 || row >= _buffer.Length || column >= _buffer[0].Length)
        {
            return null;
        }

        return _buffer[row][column];
    }

    public void CopyTo(ITextImage destination)
    {
        if (_buffer.Length > 0)
        {
            CopyTo(destination, 0, _buffer.Length, 0, _buffer[0].Length, 0, 0);
        }
    }

    public void CopyTo(ITextImage destination, int startRowIndex, int rows, int startColumnIndex, int columns, int destinationRowOffset, int destinationColumnOffset)
    {
        // If the source image position is negative, offset the whole image
        if (startColumnIndex < 0)
        {
            destinationColumnOffset += -startColumnIndex;
            columns += startColumnIndex;
            startColumnIndex = 0;
        }
        if (startRowIndex < 0)
        {
            destinationRowOffset += -startRowIndex;
            rows += startRowIndex;
            startRowIndex = 0;
        }

        // If the destination offset is negative, adjust the source start indexes
        if (destinationColumnOffset < 0)
        {
            startColumnIndex -= destinationColumnOffset;
            columns += destinationColumnOffset;
            destinationColumnOffset = 0;
        }
        if (destinationRowOffset < 0)
        {
            startRowIndex -= destinationRowOffset;
            rows += destinationRowOffset;
            destinationRowOffset = 0;
        }

        // Make sure we can't copy more than is available
        rows = Math.Min(_buffer.Length - startRowIndex, rows);
        columns = rows > 0 ? Math.Min(_buffer[0].Length - startColumnIndex, columns) : 0;

        // Adjust target lengths as well
        columns = Math.Min(destination.Size.Columns - destinationColumnOffset, columns);
        rows = Math.Min(destination.Size.Rows - destinationRowOffset, rows);

        if (columns <= 0 || rows <= 0)
        {
            return;
        }

        TerminalSize destinationSize = destination.Size;
        if (destination is BasicTextImage basicDestination)
        {
            int targetRow = destinationRowOffset;
            for (int y = startRowIndex; y < startRowIndex + rows && targetRow < destinationSize.Rows; y++)
            {
                Array.Copy(_buffer[y], startColumnIndex, basicDestination._buffer[targetRow++], destinationColumnOffset, columns);
            }
        }
        else
        {
            // Manually copy character by character
            for (int y = startRowIndex; y < startRowIndex + rows; y++)
            {
                for (int x = startColumnIndex; x < startColumnIndex + columns; x++)
                {
                    TextCharacter? character = _buffer[y][x];
                    if (character?.IsDoubleWidth == true)
                    {
                        // If we're about to put a double-width character, first reset the character next to it
                        if (x + 1 < startColumnIndex + columns)
                        {
                            destination.SetCharacterAt(
                                x - startColumnIndex + destinationColumnOffset,
                                y - startRowIndex + destinationRowOffset,
                                character.WithCharacter(' '));
                        }
                        // If the last character is a double-width character, it would exceed the dimension so reset it
                        else if (x + 1 == startColumnIndex + columns)
                        {
                            character = character.WithCharacter(' ');
                        }
                    }
                    
                    if (character != null)
                    {
                        destination.SetCharacterAt(
                            x - startColumnIndex + destinationColumnOffset,
                            y - startRowIndex + destinationRowOffset,
                            character);
                        
                        if (character.IsDoubleWidth)
                        {
                            x++;
                        }
                    }
                }
            }
        }

        // If the character immediately to the left in the destination is double-width, then reset it
        if (destinationColumnOffset > 0)
        {
            int destinationX = destinationColumnOffset - 1;
            for (int y = startRowIndex; y < startRowIndex + rows; y++)
            {
                int destinationY = y - startRowIndex + destinationRowOffset;
                TextCharacter? neighbour = destination.GetCharacterAt(destinationX, destinationY);
                if (neighbour?.IsDoubleWidth == true)
                {
                    destination.SetCharacterAt(destinationX, destinationY, neighbour.WithCharacter(' '));
                }
            }
        }
    }

    public ITextGraphics NewTextGraphics()
    {
        return new BasicTextImageGraphics(this);
    }

    private TextCharacter?[] NewBlankLine()
    {
        var line = new TextCharacter?[_size.Columns];
        Array.Fill(line, TextCharacter.DefaultCharacter);
        return line;
    }

    public void ScrollLines(int firstLine, int lastLine, int distance)
    {
        if (firstLine < 0) firstLine = 0;
        if (lastLine >= _size.Rows) lastLine = _size.Rows - 1;
        
        if (firstLine < lastLine)
        {
            if (distance > 0)
            {
                // scrolling up: start with first line as target:
                int curLine = firstLine;
                // copy lines from further "below":
                for (; curLine <= lastLine - distance; curLine++)
                {
                    _buffer[curLine] = _buffer[curLine + distance];
                }
                // blank out the remaining lines:
                for (; curLine <= lastLine; curLine++)
                {
                    _buffer[curLine] = NewBlankLine();
                }
            }
            else if (distance < 0)
            {
                // scrolling down: start with last line as target:
                int curLine = lastLine; 
                distance = -distance;
                // copy lines from further "above":
                for (; curLine >= firstLine + distance; curLine--)
                {
                    _buffer[curLine] = _buffer[curLine - distance];
                }
                // blank out the remaining lines:
                for (; curLine >= firstLine; curLine--)
                {
                    _buffer[curLine] = NewBlankLine();
                }
            }
            // else: distance == 0 => no-op
        }
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(_size.Rows * (_size.Columns + 1) + 50);
        sb.Append('{').Append(_size.Columns).Append('x').Append(_size.Rows).Append('}').Append('\n');
        
        foreach (var line in _buffer)
        {
            foreach (var tc in line)
            {
                sb.Append(tc?.Character ?? ' ');
            }
            sb.Append('\n');
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// TextGraphics implementation for BasicTextImage
    /// </summary>
    private class BasicTextImageGraphics : AbstractTextGraphics
    {
        private readonly BasicTextImage _textImage;

        public BasicTextImageGraphics(BasicTextImage textImage)
        {
            _textImage = textImage;
        }

        public override ITextGraphics SetCharacter(int columnIndex, int rowIndex, TextCharacter textCharacter)
        {
            _textImage.SetCharacterAt(columnIndex, rowIndex, textCharacter);
            return this;
        }

        public override TextCharacter? GetCharacter(int column, int row)
        {
            return _textImage.GetCharacterAt(column, row);
        }

        public override TerminalSize Size => _textImage._size;

        public override ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size)
        {
            return new SubImageGraphics(_textImage, topLeftCorner, size);
        }
    }

    /// <summary>
    /// Sub-image graphics implementation for drawing to a portion of a BasicTextImage
    /// </summary>
    private class SubImageGraphics : AbstractTextGraphics
    {
        private readonly BasicTextImage _parentImage;
        private readonly TerminalPosition _offset;
        private readonly TerminalSize _size;

        public SubImageGraphics(BasicTextImage parentImage, TerminalPosition offset, TerminalSize size)
        {
            _parentImage = parentImage;
            _offset = offset;
            _size = size;
        }

        public override TerminalSize Size => _size;

        public override ITextGraphics SetCharacter(int columnIndex, int rowIndex, TextCharacter textCharacter)
        {
            if (columnIndex >= 0 && columnIndex < _size.Columns && rowIndex >= 0 && rowIndex < _size.Rows)
            {
                _parentImage.SetCharacterAt(_offset.Column + columnIndex, _offset.Row + rowIndex, textCharacter);
            }
            return this;
        }

        public override TextCharacter? GetCharacter(int column, int row)
        {
            if (column >= 0 && column < _size.Columns && row >= 0 && row < _size.Rows)
            {
                return _parentImage.GetCharacterAt(_offset.Column + column, _offset.Row + row);
            }
            return null;
        }

        public override ITextGraphics NewTextGraphics(TerminalPosition topLeftCorner, TerminalSize size)
        {
            // Create a sub-sub-image graphics
            var newOffset = new TerminalPosition(_offset.Column + topLeftCorner.Column, _offset.Row + topLeftCorner.Row);
            var clampedSize = new TerminalSize(
                Math.Min(size.Columns, _size.Columns - topLeftCorner.Column),
                Math.Min(size.Rows, _size.Rows - topLeftCorner.Row));
            return new SubImageGraphics(_parentImage, newOffset, clampedSize);
        }
    }
}