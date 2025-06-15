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

using System.Text;
using System.Text.RegularExpressions;
using Lanterna.Core;

namespace Lanterna.Terminal.Virtual;

/// <summary>
/// This class is used to store lines of text inside of a terminal emulator. As used by DefaultVirtualTerminal, it keeps
/// two TextBuffers, one for private mode and one for normal mode and it can switch between them as needed.
/// </summary>
internal class TextBuffer
{
    private static readonly TextCharacter DoubleWidthCharPadding = TextCharacter.FromCharacter(' ')[0];

    private readonly LinkedList<List<TextCharacter>> _lines;
    private readonly object _lock = new();

    public TextBuffer()
    {
        _lines = new LinkedList<List<TextCharacter>>();
        NewLine();
    }

    public void NewLine()
    {
        lock (_lock)
        {
            _lines.AddLast(new List<TextCharacter>(200));
        }
    }

    public void RemoveTopLines(int numberOfLinesToRemove)
    {
        lock (_lock)
        {
            for (int i = 0; i < numberOfLinesToRemove && _lines.Count > 0; i++)
            {
                _lines.RemoveFirst();
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _lines.Clear();
            NewLine();
        }
    }

    public IEnumerator<List<TextCharacter>> GetLinesFrom(int rowNumber)
    {
        lock (_lock)
        {
            var linesList = _lines.ToList();
            for (int i = rowNumber; i < linesList.Count; i++)
            {
                yield return linesList[i];
            }
        }
    }

    public int GetLineCount()
    {
        lock (_lock)
        {
            return _lines.Count;
        }
    }

    /// <summary>
    /// Sets a character at the specified position and returns information about how the operation affected surrounding characters.
    /// </summary>
    /// <param name="lineNumber">Line number to set the character on</param>
    /// <param name="columnIndex">Column index to set the character at</param>
    /// <param name="textCharacter">Character to set</param>
    /// <returns>0 = normal, 1 = this character and the one to the right, 2 = this character and the one to the left</returns>
    public int SetCharacter(int lineNumber, int columnIndex, TextCharacter? textCharacter)
    {
        if (lineNumber < 0 || columnIndex < 0)
        {
            throw new ArgumentException($"Illegal argument to TextBuffer.SetCharacter(..), lineNumber = {lineNumber}, columnIndex = {columnIndex}");
        }
        
        textCharacter ??= TextCharacter.DefaultCharacter;

        lock (_lock)
        {
            while (lineNumber >= _lines.Count)
            {
                NewLine();
            }
            
            var line = _lines.ElementAt(lineNumber);
            while (line.Count <= columnIndex)
            {
                line.Add(TextCharacter.DefaultCharacter);
            }

            // Default
            int returnStyle = 0;

            // Check if we are overwriting a double-width character, in that case we need to reset the other half
            if (line[columnIndex].IsDoubleWidth)
            {
                if (columnIndex + 1 < line.Count)
                {
                    line[columnIndex + 1] = line[columnIndex].WithCharacter(' ');
                }
                returnStyle = 1; // this character and the one to the right
            }
            else if (ReferenceEquals(line[columnIndex], DoubleWidthCharPadding) && columnIndex > 0)
            {
                line[columnIndex - 1] = TextCharacter.DefaultCharacter;
                returnStyle = 2; // this character and the one to the left
            }
            
            line[columnIndex] = textCharacter;

            if (textCharacter.IsDoubleWidth)
            {
                // We don't report this column as dirty (yet), it's implied since a double-width character is reported
                SetCharacter(lineNumber, columnIndex + 1, DoubleWidthCharPadding);
            }
            
            return returnStyle;
        }
    }

    public TextCharacter GetCharacter(int lineNumber, int columnIndex)
    {
        if (lineNumber < 0 || columnIndex < 0)
        {
            throw new ArgumentException($"Illegal argument to TextBuffer.GetCharacter(..), lineNumber = {lineNumber}, columnIndex = {columnIndex}");
        }

        lock (_lock)
        {
            if (lineNumber >= _lines.Count)
            {
                return TextCharacter.DefaultCharacter;
            }
            
            var line = _lines.ElementAt(lineNumber);
            if (line.Count <= columnIndex)
            {
                return TextCharacter.DefaultCharacter;
            }
            
            TextCharacter textCharacter = line[columnIndex];
            if (ReferenceEquals(textCharacter, DoubleWidthCharPadding) && columnIndex > 0)
            {
                return line[columnIndex - 1];
            }
            
            return textCharacter;
        }
    }

    public override string ToString()
    {
        lock (_lock)
        {
            var result = new StringBuilder();
            foreach (var line in _lines)
            {
                var lineBuilder = new StringBuilder();
                foreach (var c in line)
                {
                    lineBuilder.Append(c.CharacterString);
                }
                // Remove trailing whitespace
                string lineStr = Regex.Replace(lineBuilder.ToString(), @"\s+$", "");
                result.Append(lineStr);
                result.Append('\n');
            }
            return result.ToString();
        }
    }
}