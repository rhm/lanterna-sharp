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
using Lanterna.Terminal;

namespace Lanterna.Screen;

/// <summary>
/// This is an implementation of ITextGraphics that targets the output to a Screen. The ScreenTextGraphics object is valid
/// after screen resizing and provides efficient drawing operations on the screen's back buffer.
/// </summary>
public class ScreenTextGraphics : ITextGraphics
{
    private readonly IScreen _screen;
    private ITextColor _foregroundColor;
    private ITextColor _backgroundColor;
    private readonly HashSet<SGR> _activeModifiers;

    /// <summary>
    /// Creates a new ScreenTextGraphics targeting the specified screen
    /// </summary>
    /// <param name="screen">Screen we are targeting</param>
    public ScreenTextGraphics(IScreen screen)
    {
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        _foregroundColor = new AnsiTextColor(AnsiColor.Default);
        _backgroundColor = new AnsiTextColor(AnsiColor.Default);
        _activeModifiers = new HashSet<SGR>();
    }

    public ITextColor ForegroundColor
    {
        get => _foregroundColor;
        set => _foregroundColor = value ?? new AnsiTextColor(AnsiColor.Default);
    }

    public ITextColor BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value ?? new AnsiTextColor(AnsiColor.Default);
    }

    public void SetCharacter(int column, int row, char character)
    {
        var textCharacter = TextCharacter.FromCharacter(character, _foregroundColor, _backgroundColor).WithModifiers(_activeModifiers);
        _screen.SetCharacter(column, row, textCharacter);
    }

    public void SetCharacter(TerminalPosition position, char character)
    {
        SetCharacter(position.Column, position.Row, character);
    }

    public void SetCharacter(int column, int row, TextCharacter character)
    {
        _screen.SetCharacter(column, row, character);
    }

    public void SetCharacter(TerminalPosition position, TextCharacter character)
    {
        SetCharacter(position.Column, position.Row, character);
    }

    public void PutString(int column, int row, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var textCharacters = TextCharacter.FromString(text, _foregroundColor, _backgroundColor, _activeModifiers);
        int currentColumn = column;

        foreach (var textCharacter in textCharacters)
        {
            _screen.SetCharacter(currentColumn, row, textCharacter);
            
            // Handle double-width characters
            if (textCharacter.IsDoubleWidth)
            {
                currentColumn += 2;
            }
            else
            {
                currentColumn += 1;
            }
        }
    }

    public void PutString(TerminalPosition position, string text)
    {
        PutString(position.Column, position.Row, text);
    }

    public void DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character)
    {
        DrawLine(fromPoint.Column, fromPoint.Row, toPoint.Column, toPoint.Row, character);
    }

    public void DrawLine(int fromColumn, int fromRow, int toColumn, int toRow, char character)
    {
        // Simple line drawing using Bresenham's algorithm
        int dx = Math.Abs(toColumn - fromColumn);
        int dy = Math.Abs(toRow - fromRow);
        int sx = fromColumn < toColumn ? 1 : -1;
        int sy = fromRow < toRow ? 1 : -1;
        int err = dx - dy;

        int currentColumn = fromColumn;
        int currentRow = fromRow;

        while (true)
        {
            SetCharacter(currentColumn, currentRow, character);

            if (currentColumn == toColumn && currentRow == toRow)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                currentColumn += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                currentRow += sy;
            }
        }
    }

    public void DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        DrawRectangle(topLeft.Column, topLeft.Row, size.Columns, size.Rows, character);
    }

    public void DrawRectangle(int column, int row, int width, int height, char character)
    {
        if (width <= 0 || height <= 0)
            return;

        // Validate coordinates against screen size
        var screenSize = _screen.GetTerminalSize();
        if (column >= screenSize.Columns || row >= screenSize.Rows)
            return;

        // Clamp the rectangle to screen bounds
        int endColumn = Math.Min(column + width, screenSize.Columns);
        int endRow = Math.Min(row + height, screenSize.Rows);
        int startColumn = Math.Max(column, 0);
        int startRow = Math.Max(row, 0);

        width = endColumn - startColumn;
        height = endRow - startRow;

        if (width <= 0 || height <= 0)
            return;

        // Draw top and bottom lines
        for (int x = 0; x < width; x++)
        {
            SetCharacter(startColumn + x, startRow, character);
            if (height > 1)
                SetCharacter(startColumn + x, startRow + height - 1, character);
        }

        // Draw left and right lines
        for (int y = 1; y < height - 1; y++)
        {
            SetCharacter(startColumn, startRow + y, character);
            if (width > 1)
                SetCharacter(startColumn + width - 1, startRow + y, character);
        }
    }

    public void FillRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        FillRectangle(topLeft.Column, topLeft.Row, size.Columns, size.Rows, character);
    }

    public void FillRectangle(int column, int row, int width, int height, char character)
    {
        // Validate coordinates against screen size
        var screenSize = _screen.GetTerminalSize();
        if (column >= screenSize.Columns || row >= screenSize.Rows)
            return;

        // Clamp the rectangle to screen bounds
        int endColumn = Math.Min(column + width, screenSize.Columns);
        int endRow = Math.Min(row + height, screenSize.Rows);
        int startColumn = Math.Max(column, 0);
        int startRow = Math.Max(row, 0);

        for (int y = startRow; y < endRow; y++)
        {
            for (int x = startColumn; x < endColumn; x++)
            {
                SetCharacter(x, y, character);
            }
        }
    }

    /// <summary>
    /// Gets a character from the screen's back buffer at the specified position
    /// </summary>
    /// <param name="column">Column to get the character from</param>
    /// <param name="row">Row to get the character from</param>
    /// <returns>The TextCharacter at the specified position in the back buffer</returns>
    public TextCharacter GetCharacter(int column, int row)
    {
        return _screen.GetBackCharacter(column, row);
    }

    /// <summary>
    /// Gets a character from the screen's back buffer at the specified position
    /// </summary>
    /// <param name="position">Position to get the character from</param>
    /// <returns>The TextCharacter at the specified position in the back buffer</returns>
    public TextCharacter GetCharacter(TerminalPosition position)
    {
        return GetCharacter(position.Column, position.Row);
    }

    /// <summary>
    /// Gets the size of the screen this TextGraphics is targeting
    /// </summary>
    /// <returns>Size of the screen</returns>
    public TerminalSize GetSize()
    {
        return _screen.GetTerminalSize();
    }

    /// <summary>
    /// Enables a text modifier (like bold, italic, etc.)
    /// </summary>
    /// <param name="modifier">Modifier to enable</param>
    public void EnableModifier(SGR modifier)
    {
        _activeModifiers.Add(modifier);
    }

    /// <summary>
    /// Disables a text modifier
    /// </summary>
    /// <param name="modifier">Modifier to disable</param>
    public void DisableModifier(SGR modifier)
    {
        _activeModifiers.Remove(modifier);
    }

    /// <summary>
    /// Gets a copy of the currently active modifiers
    /// </summary>
    /// <returns>Set of active modifiers</returns>
    public HashSet<SGR> GetActiveModifiers()
    {
        return new HashSet<SGR>(_activeModifiers);
    }
}