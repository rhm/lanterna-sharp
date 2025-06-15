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

using System.Collections.Concurrent;
using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Screen;

namespace Lanterna.Terminal.Virtual;

public class DefaultVirtualTerminal : AbstractTerminal, IVirtualTerminal
{
    private readonly TextBuffer _regularTextBuffer;
    private readonly TextBuffer _privateModeTextBuffer;
    private readonly SortedSet<TerminalPosition> _dirtyTerminalCells;
    private readonly List<IVirtualTerminalListener> _listeners;

    private TextBuffer _currentTextBuffer;
    private bool _wholeBufferDirty;

    private TerminalSize _terminalSize;
    private bool _cursorVisible;
    private int _backlogSize;

    private readonly ConcurrentQueue<KeyStroke> _inputQueue;
    private readonly HashSet<SGR> _activeModifiers;
    private ITextColor _activeForegroundColor;
    private ITextColor _activeBackgroundColor;

    // Global coordinates, i.e. relative to the top-left corner of the full buffer
    private TerminalPosition _cursorPosition;

    // Used when switching back from private mode, to restore the earlier cursor position
    private TerminalPosition _savedCursorPosition;

    /// <summary>
    /// Creates a new virtual terminal with an initial size set
    /// </summary>
    public DefaultVirtualTerminal() : this(new TerminalSize(80, 24))
    {
    }

    /// <summary>
    /// Creates a new virtual terminal with an initial size set
    /// </summary>
    /// <param name="initialTerminalSize">Starting size of the virtual terminal</param>
    public DefaultVirtualTerminal(TerminalSize initialTerminalSize)
    {
        _regularTextBuffer = new TextBuffer();
        _privateModeTextBuffer = new TextBuffer();
        _dirtyTerminalCells = new SortedSet<TerminalPosition>();
        _listeners = new List<IVirtualTerminalListener>();

        // Terminal state
        _inputQueue = new ConcurrentQueue<KeyStroke>();
        _activeModifiers = new HashSet<SGR>();
        _activeForegroundColor = new AnsiTextColor(AnsiColor.Default);
        _activeBackgroundColor = new AnsiTextColor(AnsiColor.Default);

        // Start with regular mode
        _currentTextBuffer = _regularTextBuffer;
        _wholeBufferDirty = false;
        _terminalSize = initialTerminalSize;
        _cursorVisible = true;
        _cursorPosition = TerminalPosition.TopLeftCorner;
        _savedCursorPosition = TerminalPosition.TopLeftCorner;
        _backlogSize = 1000;
    }

    #region Terminal interface methods

    public override TerminalSize GetTerminalSize()
    {
        lock (this)
        {
            return _terminalSize;
        }
    }

    public void SetTerminalSize(TerminalSize newSize)
    {
        lock (this)
        {
            _terminalSize = newSize;
            TrimBufferBacklog();
            CorrectCursor();
            foreach (var listener in _listeners)
            {
                listener.OnResized(this, _terminalSize);
            }
            OnResized(newSize);
        }
    }

    public override void EnterPrivateMode()
    {
        lock (this)
        {
            _currentTextBuffer = _privateModeTextBuffer;
            _savedCursorPosition = GetCursorBufferPosition();
            SetCursorPosition(TerminalPosition.TopLeftCorner);
            SetWholeBufferDirty();
        }
    }

    public override void ExitPrivateMode()
    {
        lock (this)
        {
            _currentTextBuffer = _regularTextBuffer;
            _cursorPosition = _savedCursorPosition;
            SetWholeBufferDirty();
        }
    }

    public override void ClearScreen()
    {
        lock (this)
        {
            _currentTextBuffer.Clear();
            SetWholeBufferDirty();
            SetCursorPosition(TerminalPosition.TopLeftCorner);
        }
    }

    public override void SetCursorPosition(int x, int y)
    {
        SetCursorPosition(new TerminalPosition(x, y));
    }

    public override void SetCursorPosition(TerminalPosition cursorPosition)
    {
        lock (this)
        {
            if (_terminalSize.Rows < GetBufferLineCount())
            {
                cursorPosition = cursorPosition.WithRelativeRow(GetBufferLineCount() - _terminalSize.Rows);
            }
            _cursorPosition = cursorPosition;
            CorrectCursor();
        }
    }

    public override TerminalPosition GetCursorPosition()
    {
        lock (this)
        {
            if (GetBufferLineCount() <= _terminalSize.Rows)
            {
                return GetCursorBufferPosition();
            }
            else
            {
                return _cursorPosition.WithRelativeRow(-(GetBufferLineCount() - _terminalSize.Rows));
            }
        }
    }

    public TerminalPosition GetCursorBufferPosition()
    {
        lock (this)
        {
            return _cursorPosition;
        }
    }

    public override void SetCursorVisible(bool visible)
    {
        lock (this)
        {
            _cursorVisible = visible;
        }
    }

    public bool IsCursorVisible => _cursorVisible;

    public override void PutCharacter(char c)
    {
        lock (this)
        {
            if (c == '\n')
            {
                MoveCursorToNextLine();
            }
            else if (TerminalTextUtils.IsPrintableCharacter(c))
            {
                PutCharacter(TextCharacter.FromCharacter(c, _activeForegroundColor, _activeBackgroundColor, _activeModifiers.ToArray()));
            }
        }
    }

    public override void PutString(string str)
    {
        lock (this)
        {
            foreach (char c in str)
            {
                PutCharacter(c);
            }
        }
    }

    public override void EnableSGR(SGR sgr)
    {
        lock (this)
        {
            _activeModifiers.Add(sgr);
        }
    }

    public override void DisableSGR(SGR sgr)
    {
        lock (this)
        {
            _activeModifiers.Remove(sgr);
        }
    }

    public override void ResetColorAndSGR()
    {
        lock (this)
        {
            _activeModifiers.Clear();
            _activeForegroundColor = new AnsiTextColor(AnsiColor.Default);
            _activeBackgroundColor = new AnsiTextColor(AnsiColor.Default);
        }
    }

    public override void SetForegroundColor(ITextColor color)
    {
        lock (this)
        {
            _activeForegroundColor = color;
        }
    }

    public override void SetBackgroundColor(ITextColor color)
    {
        lock (this)
        {
            _activeBackgroundColor = color;
        }
    }

    public override byte[] EnquireTerminal(TimeSpan timeout)
    {
        lock (this)
        {
            return System.Text.Encoding.UTF8.GetBytes(GetType().Name);
        }
    }

    public override void Bell()
    {
        lock (this)
        {
            foreach (var listener in _listeners)
            {
                listener.OnBell();
            }
        }
    }

    public override void Flush()
    {
        lock (this)
        {
            foreach (var listener in _listeners)
            {
                listener.OnFlush();
            }
        }
    }

    public override KeyStroke? PollInput()
    {
        return _inputQueue.TryDequeue(out KeyStroke? result) ? result : null;
    }

    public override KeyStroke ReadInput()
    {
        KeyStroke? keyStroke;
        while (!_inputQueue.TryDequeue(out keyStroke))
        {
            Thread.Sleep(10);
        }
        return keyStroke;
    }

    public override KeyStroke? ReadInput(TimeSpan timeout)
    {
        DateTime endTime = DateTime.Now + timeout;
        while (DateTime.Now < endTime)
        {
            if (_inputQueue.TryDequeue(out KeyStroke? keyStroke))
                return keyStroke;
            Thread.Sleep(10);
        }
        return null;
    }

    public override ITextGraphics NewTextGraphics()
    {
        return new VirtualTerminalTextGraphics(this);
    }

    #endregion

    #region VirtualTerminal specific methods

    public void AddVirtualTerminalListener(IVirtualTerminalListener listener)
    {
        lock (this)
        {
            if (listener != null)
            {
                _listeners.Add(listener);
            }
        }
    }

    public void RemoveVirtualTerminalListener(IVirtualTerminalListener listener)
    {
        lock (this)
        {
            _listeners.Remove(listener);
        }
    }

    public void SetBacklogSize(int backlogSize)
    {
        lock (this)
        {
            _backlogSize = backlogSize;
        }
    }

    public void AddInput(KeyStroke keyStroke)
    {
        _inputQueue.Enqueue(keyStroke);
    }

    public SortedSet<TerminalPosition> GetDirtyCells()
    {
        lock (this)
        {
            return new SortedSet<TerminalPosition>(_dirtyTerminalCells);
        }
    }

    public SortedSet<TerminalPosition> GetAndResetDirtyCells()
    {
        lock (this)
        {
            var copy = new SortedSet<TerminalPosition>(_dirtyTerminalCells);
            _dirtyTerminalCells.Clear();
            return copy;
        }
    }

    public bool IsWholeBufferDirtyThenReset()
    {
        lock (this)
        {
            bool copy = _wholeBufferDirty;
            _wholeBufferDirty = false;
            return copy;
        }
    }

    public TextCharacter GetCharacter(TerminalPosition position)
    {
        return GetCharacter(position.Column, position.Row);
    }

    public TextCharacter GetCharacter(int column, int row)
    {
        lock (this)
        {
            if (_terminalSize.Rows < _currentTextBuffer.GetLineCount())
            {
                row += _currentTextBuffer.GetLineCount() - _terminalSize.Rows;
            }
            return GetBufferCharacter(column, row);
        }
    }

    public TextCharacter GetBufferCharacter(int column, int row)
    {
        return _currentTextBuffer.GetCharacter(row, column);
    }

    public TextCharacter GetBufferCharacter(TerminalPosition position)
    {
        return GetBufferCharacter(position.Column, position.Row);
    }

    public int GetBufferLineCount()
    {
        lock (this)
        {
            return _currentTextBuffer.GetLineCount();
        }
    }

    public void ForEachLine(int startRow, int endRow, IVirtualTerminal.IBufferWalker bufferWalker)
    {
        lock (this)
        {
            var emptyLine = new EmptyBufferLine();
            var enumerator = _currentTextBuffer.GetLinesFrom(startRow);
            
            for (int row = startRow; row <= endRow; row++)
            {
                IVirtualTerminal.IBufferLine bufferLine = emptyLine;
                if (enumerator.MoveNext())
                {
                    var list = enumerator.Current;
                    bufferLine = new ListBufferLine(list);
                }
                bufferWalker.OnLine(row, bufferLine);
            }
        }
    }

    #endregion

    #region Private helper methods

    private void PutCharacter(TextCharacter terminalCharacter)
    {
        if (terminalCharacter.CharacterString == "\t")
        {
            int nrOfSpaces = TabBehaviour.AlignToColumn4.GetTabReplacement(_cursorPosition.Column).Length;
            for (int i = 0; i < nrOfSpaces && _cursorPosition.Column < _terminalSize.Columns - 1; i++)
            {
                PutCharacter(terminalCharacter.WithCharacter(' '));
            }
        }
        else
        {
            bool doubleWidth = terminalCharacter.IsDoubleWidth;
            // If we're at the last column and the user tries to print a double-width character, reset the cell and move
            // to the next line
            if (_cursorPosition.Column == _terminalSize.Columns - 1 && doubleWidth)
            {
                _currentTextBuffer.SetCharacter(_cursorPosition.Row, _cursorPosition.Column, TextCharacter.DefaultCharacter);
                MoveCursorToNextLine();
            }
            if (_cursorPosition.Column == _terminalSize.Columns)
            {
                MoveCursorToNextLine();
            }

            // Update the buffer
            int changeType = _currentTextBuffer.SetCharacter(_cursorPosition.Row, _cursorPosition.Column, terminalCharacter);
            if (!_wholeBufferDirty)
            {
                _dirtyTerminalCells.Add(new TerminalPosition(_cursorPosition.Column, _cursorPosition.Row));
                if (changeType == 1)
                {
                    _dirtyTerminalCells.Add(new TerminalPosition(_cursorPosition.Column + 1, _cursorPosition.Row));
                }
                else if (changeType == 2)
                {
                    _dirtyTerminalCells.Add(new TerminalPosition(_cursorPosition.Column - 1, _cursorPosition.Row));
                }
                if (_dirtyTerminalCells.Count > (_terminalSize.Columns * _terminalSize.Rows * 0.9))
                {
                    SetWholeBufferDirty();
                }
            }

            // Advance cursor
            _cursorPosition = _cursorPosition.WithRelativeColumn(doubleWidth ? 2 : 1);
            if (_cursorPosition.Column >= _terminalSize.Columns)
            {
                MoveCursorToNextLine();
            }
        }
    }

    /// <summary>
    /// Moves the text cursor to the first column of the next line and trims the backlog if necessary
    /// </summary>
    private void MoveCursorToNextLine()
    {
        _cursorPosition = _cursorPosition.WithColumn(0).WithRelativeRow(1);
        if (_cursorPosition.Row >= _currentTextBuffer.GetLineCount())
        {
            _currentTextBuffer.NewLine();
        }
        TrimBufferBacklog();
        CorrectCursor();
    }

    /// <summary>
    /// Marks the whole buffer as dirty so every cell is considered in need to repainting. This is used by methods such
    /// as clear and bell that will affect all content at once.
    /// </summary>
    private void SetWholeBufferDirty()
    {
        _wholeBufferDirty = true;
        _dirtyTerminalCells.Clear();
    }

    private void TrimBufferBacklog()
    {
        // Now see if we need to discard lines from the backlog
        int bufferBacklogSize = _backlogSize;
        if (_currentTextBuffer == _privateModeTextBuffer)
        {
            bufferBacklogSize = 0;
        }
        int trimBacklogRows = _currentTextBuffer.GetLineCount() - (bufferBacklogSize + _terminalSize.Rows);
        if (trimBacklogRows > 0)
        {
            _currentTextBuffer.RemoveTopLines(trimBacklogRows);
            // Adjust cursor position
            _cursorPosition = _cursorPosition.WithRelativeRow(-trimBacklogRows);
            CorrectCursor();
            if (!_wholeBufferDirty)
            {
                // Adjust all "dirty" positions
                var newDirtySet = new SortedSet<TerminalPosition>();
                foreach (var dirtyPosition in _dirtyTerminalCells)
                {
                    var adjustedPosition = dirtyPosition.WithRelativeRow(-trimBacklogRows);
                    if (adjustedPosition.Row >= 0)
                    {
                        newDirtySet.Add(adjustedPosition);
                    }
                }
                _dirtyTerminalCells.Clear();
                foreach (var pos in newDirtySet)
                {
                    _dirtyTerminalCells.Add(pos);
                }
            }
        }
    }

    private void CorrectCursor()
    {
        _cursorPosition = _cursorPosition.WithColumn(Math.Min(_cursorPosition.Column, _terminalSize.Columns - 1));
        _cursorPosition = _cursorPosition.WithRow(Math.Min(_cursorPosition.Row, Math.Max(_terminalSize.Rows, GetBufferLineCount()) - 1));
        _cursorPosition = new TerminalPosition(
            Math.Max(_cursorPosition.Column, 0),
            Math.Max(_cursorPosition.Row, 0));
    }

    #endregion

    public override string ToString()
    {
        return _currentTextBuffer.ToString();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var listener in _listeners)
            {
                listener.OnClose();
            }
        }
        base.Dispose(disposing);
    }

    #region Helper classes for buffer line iteration

    private class EmptyBufferLine : IVirtualTerminal.IBufferLine
    {
        public TextCharacter GetCharacterAt(int column) => TextCharacter.DefaultCharacter;
    }

    private class ListBufferLine : IVirtualTerminal.IBufferLine
    {
        private readonly List<TextCharacter> _list;

        public ListBufferLine(List<TextCharacter> list)
        {
            _list = list;
        }

        public TextCharacter GetCharacterAt(int column)
        {
            if (column >= _list.Count)
            {
                return TextCharacter.DefaultCharacter;
            }
            return _list[column];
        }
    }

    #endregion
}

/// <summary>
/// Basic implementation of ITextGraphics for virtual terminals
/// </summary>
public class VirtualTerminalTextGraphics : ITextGraphics
{
    private readonly IVirtualTerminal _virtualTerminal;

    public VirtualTerminalTextGraphics(IVirtualTerminal virtualTerminal)
    {
        _virtualTerminal = virtualTerminal;
    }

    public ITextColor ForegroundColor { get; set; } = new AnsiTextColor(AnsiColor.Default);
    public ITextColor BackgroundColor { get; set; } = new AnsiTextColor(AnsiColor.Default);

    public void SetCharacter(int column, int row, char character)
    {
        _virtualTerminal.SetCursorPosition(column, row);
        _virtualTerminal.PutCharacter(character);
    }

    public void SetCharacter(TerminalPosition position, char character)
    {
        SetCharacter(position.Column, position.Row, character);
    }

    public void SetCharacter(int column, int row, TextCharacter character)
    {
        _virtualTerminal.SetCursorPosition(column, row);
        // Apply character formatting and output
        _virtualTerminal.SetForegroundColor(character.ForegroundColor);
        _virtualTerminal.SetBackgroundColor(character.BackgroundColor);
        foreach (var modifier in character.Modifiers)
        {
            _virtualTerminal.EnableSGR(modifier);
        }
        _virtualTerminal.PutString(character.CharacterString);
    }

    public void SetCharacter(TerminalPosition position, TextCharacter character)
    {
        SetCharacter(position.Column, position.Row, character);
    }

    public void PutString(int column, int row, string text)
    {
        _virtualTerminal.SetCursorPosition(column, row);
        _virtualTerminal.PutString(text);
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