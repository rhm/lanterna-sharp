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
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Screen;

namespace Lanterna.Terminal;

/// <summary>
/// Containing a some very fundamental functionality that should be common (and usable) to all terminal implementations.
/// All the Terminal implementers within Lanterna extends from this class.
/// </summary>
public abstract class AbstractTerminal : ITerminal
{
    private readonly List<ITerminalResizeListener> _resizeListeners;
    private TerminalSize? _lastKnownSize;
    private bool _disposed;

    protected AbstractTerminal()
    {
        _resizeListeners = new List<ITerminalResizeListener>();
        _lastKnownSize = null;
    }

    public virtual void AddResizeListener(ITerminalResizeListener listener)
    {
        if (listener != null)
        {
            lock (_resizeListeners)
            {
                _resizeListeners.Add(listener);
            }
        }
    }

    public virtual void RemoveResizeListener(ITerminalResizeListener listener)
    {
        if (listener != null)
        {
            lock (_resizeListeners)
            {
                _resizeListeners.Remove(listener);
            }
        }
    }

    /// <summary>
    /// Call this method when the terminal has been resized or the initial size of the terminal has been discovered. It
    /// will trigger all resize listeners, but only if the size has changed from before.
    /// </summary>
    /// <param name="columns">Number of columns in the new size</param>
    /// <param name="rows">Number of rows in the new size</param>
    protected void OnResized(int columns, int rows)
    {
        OnResized(new TerminalSize(columns, rows));
    }

    /// <summary>
    /// Call this method when the terminal has been resized or the initial size of the terminal has been discovered. It
    /// will trigger all resize listeners, but only if the size has changed from before.
    /// </summary>
    /// <param name="newSize">Last discovered terminal size</param>
    protected void OnResized(TerminalSize newSize)
    {
        lock (_resizeListeners)
        {
            if (_lastKnownSize == null || !_lastKnownSize.Value.Equals(newSize))
            {
                _lastKnownSize = newSize;
                foreach (var resizeListener in _resizeListeners)
                {
                    resizeListener.OnResized(this, _lastKnownSize.Value);
                }
            }
        }
    }

    public virtual ITextGraphics NewTextGraphics()
    {
        return new TerminalTextGraphics(this);
    }

    // Abstract methods that must be implemented by concrete terminal classes
    public abstract void EnterPrivateMode();
    public abstract void ExitPrivateMode();
    public abstract void ClearScreen();
    public abstract void SetCursorPosition(int x, int y);
    public abstract void SetCursorPosition(TerminalPosition position);
    public abstract TerminalPosition GetCursorPosition();
    public abstract void SetCursorVisible(bool visible);
    public abstract void PutCharacter(char c);
    public abstract void PutString(string str);
    public abstract void EnableSGR(SGR sgr);
    public abstract void DisableSGR(SGR sgr);
    public abstract void ResetColorAndSGR();
    public abstract void SetForegroundColor(ITextColor color);
    public abstract void SetBackgroundColor(ITextColor color);
    public abstract TerminalSize GetTerminalSize();
    public abstract byte[] EnquireTerminal(TimeSpan timeout);
    public abstract void Bell();
    public abstract void Flush();

    // Abstract InputProvider methods
    public abstract KeyStroke? PollInput();
    public abstract KeyStroke ReadInput();
    public abstract KeyStroke? ReadInput(TimeSpan timeout);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                lock (_resizeListeners)
                {
                    _resizeListeners.Clear();
                }
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Basic implementation of ITextGraphics that works directly with a Terminal
/// </summary>
public class TerminalTextGraphics : ITextGraphics
{
    private readonly ITerminal _terminal;
    private ITextColor _foregroundColor;
    private ITextColor _backgroundColor;
    private readonly HashSet<SGR> _activeModifiers;

    public TerminalTextGraphics(ITerminal terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _foregroundColor = new AnsiTextColor(AnsiColor.Default);
        _backgroundColor = new AnsiTextColor(AnsiColor.Default);
        _activeModifiers = new HashSet<SGR>();
    }

    public TerminalSize Size => _terminal.GetTerminalSize();

    public ITextColor ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            _foregroundColor = value ?? new AnsiTextColor(AnsiColor.Default);
            _terminal.SetForegroundColor(_foregroundColor);
        }
    }

    public ITextColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value ?? new AnsiTextColor(AnsiColor.Default);
            _terminal.SetBackgroundColor(_backgroundColor);
        }
    }


    public void SetCharacter(int column, int row, char character)
    {
        _terminal.SetCursorPosition(column, row);
        _terminal.PutCharacter(character);
    }

    public void SetCharacter(TerminalPosition position, char character)
    {
        SetCharacter(position.Column, position.Row, character);
    }

    public void SetCharacter(int column, int row, TextCharacter character)
    {
        // Apply the character's colors and modifiers
        if (!character.ForegroundColor.Equals(_foregroundColor))
        {
            ForegroundColor = character.ForegroundColor;
        }
        
        if (!character.BackgroundColor.Equals(_backgroundColor))
        {
            BackgroundColor = character.BackgroundColor;
        }

        // Apply modifiers
        var characterModifiers = character.Modifiers;
        var toEnable = characterModifiers.Except(_activeModifiers);
        var toDisable = _activeModifiers.Except(characterModifiers);

        foreach (var sgr in toDisable)
        {
            _terminal.DisableSGR(sgr);
            _activeModifiers.Remove(sgr);
        }

        foreach (var sgr in toEnable)
        {
            _terminal.EnableSGR(sgr);
            _activeModifiers.Add(sgr);
        }

        // Output the character
        _terminal.SetCursorPosition(column, row);
        _terminal.PutString(character.CharacterString);
    }

    public void SetCharacter(TerminalPosition position, TextCharacter character)
    {
        SetCharacter(position.Column, position.Row, character);
    }

    public void PutString(int column, int row, string text)
    {
        _terminal.SetCursorPosition(column, row);
        _terminal.PutString(text);
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

        // Draw top and bottom lines
        for (int x = 0; x < width; x++)
        {
            SetCharacter(column + x, row, character);
            if (height > 1)
                SetCharacter(column + x, row + height - 1, character);
        }

        // Draw left and right lines
        for (int y = 1; y < height - 1; y++)
        {
            SetCharacter(column, row + y, character);
            if (width > 1)
                SetCharacter(column + width - 1, row + y, character);
        }
    }

    public void FillRectangle(TerminalPosition topLeft, TerminalSize size, char character)
    {
        FillRectangle(topLeft.Column, topLeft.Row, size.Columns, size.Rows, character);
    }

    public void FillRectangle(int column, int row, int width, int height, char character)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SetCharacter(column + x, row + y, character);
            }
        }
    }

}

