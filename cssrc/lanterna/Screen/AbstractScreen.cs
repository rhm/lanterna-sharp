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
using Lanterna.Input;
using Lanterna.Terminal;

namespace Lanterna.Screen;

/// <summary>
/// This class implements some of the Screen logic that is not directly tied to the actual implementation of how the
/// Screen translates to the terminal. It keeps data structures for the front- and back buffers, the cursor location and
/// some other simpler states.
/// </summary>
public abstract class AbstractScreen : IScreen
{
    private TerminalPosition? _cursorPosition;
    private ScreenBuffer _backBuffer;
    private ScreenBuffer _frontBuffer;
    private readonly TextCharacter _defaultCharacter;
    private TabBehavior _tabBehavior;
    private TerminalSize _terminalSize;
    private TerminalSize? _latestResizeRequest;
    private bool _disposed;

    /// <summary>
    /// Creates a new Screen with the specified initial size
    /// </summary>
    /// <param name="initialSize">Size to initially create the Screen with (can be resized later)</param>
    protected AbstractScreen(TerminalSize initialSize) : this(initialSize, IScreen.DefaultCharacter)
    {
    }

    /// <summary>
    /// Creates a new Screen with the specified initial size and default character
    /// </summary>
    /// <param name="initialSize">Size to initially create the Screen with (can be resized later)</param>
    /// <param name="defaultCharacter">What character to use for the initial state of the screen and expanded areas</param>
    protected AbstractScreen(TerminalSize initialSize, TextCharacter defaultCharacter)
    {
        _frontBuffer = new ScreenBuffer(initialSize, defaultCharacter);
        _backBuffer = new ScreenBuffer(initialSize, defaultCharacter);
        _defaultCharacter = defaultCharacter;
        _cursorPosition = new TerminalPosition(0, 0);
        _tabBehavior = TabBehavior.AlignToColumn4;
        _terminalSize = initialSize;
        _latestResizeRequest = null;
    }

    public virtual TerminalPosition? GetCursorPosition()
    {
        return _cursorPosition;
    }

    public virtual void SetCursorPosition(TerminalPosition? position)
    {
        if (position == null)
        {
            // Skip any validation checks if we just want to hide the cursor
            _cursorPosition = null;
            return;
        }

        var validatedPosition = position.Value;

        if (validatedPosition.Column < 0)
            validatedPosition = validatedPosition.WithColumn(0);
        if (validatedPosition.Row < 0)
            validatedPosition = validatedPosition.WithRow(0);
        if (validatedPosition.Column >= _terminalSize.Columns)
            validatedPosition = validatedPosition.WithColumn(_terminalSize.Columns - 1);
        if (validatedPosition.Row >= _terminalSize.Rows)
            validatedPosition = validatedPosition.WithRow(_terminalSize.Rows - 1);

        _cursorPosition = validatedPosition;
    }

    public virtual void SetTabBehavior(TabBehavior tabBehavior)
    {
        _tabBehavior = tabBehavior;
    }

    public virtual TabBehavior GetTabBehavior()
    {
        return _tabBehavior;
    }

    public virtual void SetCharacter(TerminalPosition position, TextCharacter screenCharacter)
    {
        SetCharacter(position.Column, position.Row, screenCharacter);
    }

    public virtual ITextGraphics NewTextGraphics()
    {
        return new ScreenTextGraphics(this);
    }

    public virtual void SetCharacter(int column, int row, TextCharacter screenCharacter)
    {
        // Handle tabs by converting them to spaces
        if (screenCharacter.CharacterString == "\t")
        {
            // Replace tab with space
            screenCharacter = screenCharacter.WithCharacter(' ');

            // Calculate how many spaces to put based on tab behavior
            string tabReplacement = ReplaceTab("\t", column);
            for (int i = 0; i < tabReplacement.Length; i++)
            {
                _backBuffer.SetCharacterAt(column + i, row, screenCharacter);
            }
        }
        else
        {
            _backBuffer.SetCharacterAt(column, row, screenCharacter);
        }
    }

    /// <summary>
    /// Replaces a tab character with the appropriate number of spaces based on the current tab behavior
    /// </summary>
    /// <param name="tab">The tab character (should be "\t")</param>
    /// <param name="currentColumn">The current column position</param>
    /// <returns>String of spaces to replace the tab with</returns>
    private string ReplaceTab(string tab, int currentColumn)
    {
        return _tabBehavior switch
        {
            TabBehavior.ReplaceWithSpace => " ",
            TabBehavior.AlignToColumn4 => new string(' ', 4 - (currentColumn % 4)),
            TabBehavior.AlignToColumn8 => new string(' ', 8 - (currentColumn % 8)),
            _ => " "
        };
    }

    public virtual TerminalSize GetTerminalSize()
    {
        return _terminalSize;
    }

    public virtual TextCharacter GetFrontCharacter(int column, int row)
    {
        return _frontBuffer.GetCharacterAt(column, row);
    }

    public virtual TextCharacter GetFrontCharacter(TerminalPosition position)
    {
        return GetFrontCharacter(position.Column, position.Row);
    }

    public virtual TextCharacter GetBackCharacter(int column, int row)
    {
        return _backBuffer.GetCharacterAt(column, row);
    }

    public virtual TextCharacter GetBackCharacter(TerminalPosition position)
    {
        return GetBackCharacter(position.Column, position.Row);
    }

    public virtual void Clear()
    {
        _backBuffer.Fill(_defaultCharacter);
    }

    public virtual void ScrollLines(int firstLine, int lastLine, int distance)
    {
        _backBuffer.ScrollLines(firstLine, lastLine, distance, _defaultCharacter);
    }

    public abstract void StartScreen();
    public abstract void StopScreen();
    public abstract void Refresh();
    public abstract void Refresh(RefreshType refreshType);
    public abstract TerminalSize? DoResizeIfNecessary();

    // InputProvider implementation - these need to be implemented by concrete classes
    public abstract KeyStroke? PollInput();
    public abstract KeyStroke ReadInput();
    public abstract KeyStroke? ReadInput(TimeSpan timeout);

    /// <summary>
    /// Resizes both the front and back buffers to the new size
    /// </summary>
    /// <param name="newSize">New size for the buffers</param>
    protected virtual void ResizeBuffers(TerminalSize newSize)
    {
        _frontBuffer = _frontBuffer.Resize(newSize, _defaultCharacter);
        _backBuffer = _backBuffer.Resize(newSize, _defaultCharacter);
        _terminalSize = newSize;
    }

    /// <summary>
    /// Gets the back buffer for use by subclasses
    /// </summary>
    protected ScreenBuffer BackBuffer => _backBuffer;

    /// <summary>
    /// Gets the front buffer for use by subclasses
    /// </summary>
    protected ScreenBuffer FrontBuffer => _frontBuffer;

    /// <summary>
    /// Gets the default character for use by subclasses
    /// </summary>
    protected TextCharacter DefaultCharacter => _defaultCharacter;

    /// <summary>
    /// Sets the latest resize request for processing by DoResizeIfNecessary
    /// </summary>
    /// <param name="newSize">New size requested</param>
    protected void SetLatestResizeRequest(TerminalSize newSize)
    {
        _latestResizeRequest = newSize;
    }

    /// <summary>
    /// Processes any pending resize request
    /// </summary>
    /// <returns>The new size if a resize occurred, otherwise null</returns>
    protected TerminalSize? ProcessResizeIfNecessary()
    {
        if (_latestResizeRequest != null && !_latestResizeRequest.Value.Equals(_terminalSize))
        {
            var newSize = _latestResizeRequest.Value;
            ResizeBuffers(newSize);
            _latestResizeRequest = null;
            return newSize;
        }
        return null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
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