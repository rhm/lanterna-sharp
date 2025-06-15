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
using Lanterna.Terminal.Virtual;

namespace Lanterna.Screen;

/// <summary>
/// This is the default concrete implementation of the IScreen interface, a buffered layer sitting on top of a Terminal.
/// If you want to get started with the Screen layer, this is probably the class you want to use. Remember to start the
/// screen before you can use it and stop it when you are done with it. This will place the terminal in private mode
/// during the screen operations and leave private mode afterwards.
/// </summary>
public class TerminalScreen : AbstractScreen
{
    private readonly ITerminal _terminal;
    private bool _isStarted;
    private bool _fullRedrawHint;

    /// <summary>
    /// Creates a new Screen on top of a supplied terminal, will query the terminal for its size. The screen is initially
    /// blank. The default character used for unused space (the newly initialized state of the screen and new areas after
    /// expanding the terminal size) will be a blank space in 'default' ANSI front- and background color.
    /// </summary>
    /// <param name="terminal">Terminal object to create the TerminalScreen on top of</param>
    public TerminalScreen(ITerminal terminal) : this(terminal, IScreen.DefaultCharacter)
    {
    }

    /// <summary>
    /// Creates a new Screen on top of a supplied terminal, will query the terminal for its size. The screen is initially
    /// blank. You can specify which character you wish to be used to fill the screen initially; this will also be the
    /// character used if the terminal is enlarged and you don't set anything on the new areas.
    /// </summary>
    /// <param name="terminal">Terminal object to create the TerminalScreen on top of</param>
    /// <param name="defaultCharacter">What character to use for the initial state of the screen and expanded areas</param>
    public TerminalScreen(ITerminal terminal, TextCharacter defaultCharacter) 
        : base(terminal.GetTerminalSize(), defaultCharacter)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _terminal.AddResizeListener(new TerminalScreenResizeListener(this));
        _isStarted = false;
        _fullRedrawHint = true;
    }

    /// <summary>
    /// Gets the underlying terminal that this screen is built on top of
    /// </summary>
    public ITerminal GetTerminal()
    {
        return _terminal;
    }

    public override void StartScreen()
    {
        lock (this)
        {
            if (_isStarted)
                return;

            _isStarted = true;
            _terminal.EnterPrivateMode();
            _terminal.GetTerminalSize();
            _terminal.ClearScreen();
            _fullRedrawHint = true;
        }
    }

    public override void StopScreen()
    {
        lock (this)
        {
            if (!_isStarted)
                return;

            _isStarted = false;
            _terminal.ExitPrivateMode();
        }
    }

    public override void Refresh()
    {
        Refresh(RefreshType.Automatic);
    }

    public override void Refresh(RefreshType refreshType)
    {
        lock (this)
        {
            if (!_isStarted)
                return;

            // Check if we need to resize first
            DoResizeIfNecessary();

            // Check if underlying virtual terminal needs a complete refresh and reset the flag
            if (_terminal is IVirtualTerminal virtualTerminal)
            {
                if (virtualTerminal.IsWholeBufferDirtyThenReset())
                {
                    _fullRedrawHint = true;
                }
            }

            switch (refreshType)
            {
                case RefreshType.Complete:
                    RefreshComplete();
                    break;
                case RefreshType.Delta:
                    RefreshDelta();
                    break;
                case RefreshType.Automatic:
                default:
                    RefreshAutomatic();
                    break;
            }

            // Update cursor position
            var cursorPosition = GetCursorPosition();
            if (cursorPosition != null)
            {
                _terminal.SetCursorPosition(cursorPosition.Value);
                _terminal.SetCursorVisible(true);
            }
            else
            {
                _terminal.SetCursorVisible(false);
            }

            _terminal.Flush();
        }
    }

    /// <summary>
    /// Performs a complete refresh by redrawing the entire screen
    /// </summary>
    private void RefreshComplete()
    {
        var terminalSize = GetTerminalSize();
        
        // Mark start of refresh operation to prevent dirty cell overflow
        if (_terminal is IVirtualTerminal virtualTerminal)
        {
            virtualTerminal.BeginRefreshOperation();
        }

        // Reset colors but don't clear the terminal buffer
        _terminal.ResetColorAndSGR();
        _terminal.SetCursorPosition(0, 0);

        // Draw all characters from the back buffer
        for (int row = 0; row < terminalSize.Rows; row++)
        {
            for (int col = 0; col < terminalSize.Columns; col++)
            {
                var backCharacter = BackBuffer.GetCharacterAt(col, row);
                var frontCharacter = FrontBuffer.GetCharacterAt(col, row);

                // Update front buffer and draw to terminal
                FrontBuffer.SetCharacterAt(col, row, backCharacter);
                DrawCharacterToTerminal(col, row, backCharacter);
            }
        }

        // Mark end of refresh operation
        if (_terminal is IVirtualTerminal virtualTerminal2)
        {
            virtualTerminal2.EndRefreshOperation();
        }

        _fullRedrawHint = false;
    }

    /// <summary>
    /// Performs a delta refresh by only updating changed characters
    /// </summary>
    private void RefreshDelta()
    {
        var terminalSize = GetTerminalSize();
        var lastDrawnCharacter = DefaultCharacter;
        var lastPosition = new TerminalPosition(-1, -1);

        // Mark start of refresh operation to prevent dirty cell overflow
        if (_terminal is IVirtualTerminal virtualTerminal)
        {
            virtualTerminal.BeginRefreshOperation();
        }

        for (int row = 0; row < terminalSize.Rows; row++)
        {
            for (int col = 0; col < terminalSize.Columns; col++)
            {
                var backCharacter = BackBuffer.GetCharacterAt(col, row);
                var frontCharacter = FrontBuffer.GetCharacterAt(col, row);

                // Only update if the character has changed
                if (!backCharacter.Equals(frontCharacter))
                {
                    var currentPosition = new TerminalPosition(col, row);
                    
                    // Move cursor if needed (optimize by only moving when necessary)
                    if (!currentPosition.Equals(lastPosition.WithRelative(1, 0)))
                    {
                        _terminal.SetCursorPosition(currentPosition);
                    }

                    // Update front buffer and draw to terminal
                    FrontBuffer.SetCharacterAt(col, row, backCharacter);
                    DrawCharacterToTerminal(col, row, backCharacter);
                    
                    lastDrawnCharacter = backCharacter;
                    lastPosition = currentPosition;
                }
            }
        }

        // Mark end of refresh operation
        if (_terminal is IVirtualTerminal virtualTerminal2)
        {
            virtualTerminal2.EndRefreshOperation();
        }
    }

    /// <summary>
    /// Performs an automatic refresh using heuristics to choose the best method
    /// </summary>
    private void RefreshAutomatic()
    {
        // Use complete refresh if we have a hint that a full redraw is needed
        if (_fullRedrawHint)
        {
            RefreshComplete();
            return;
        }

        // Check if the buffers are very different (more than 25% different)
        int threshold = (GetTerminalSize().Columns * GetTerminalSize().Rows) / 4;
        if (BackBuffer.IsVeryDifferent(FrontBuffer, threshold))
        {
            RefreshComplete();
        }
        else
        {
            RefreshDelta();
        }
    }

    /// <summary>
    /// Draws a single character to the terminal at the specified position
    /// </summary>
    /// <param name="column">Column to draw at</param>
    /// <param name="row">Row to draw at</param>
    /// <param name="character">Character to draw</param>
    private void DrawCharacterToTerminal(int column, int row, TextCharacter character)
    {
        // Set cursor position
        _terminal.SetCursorPosition(column, row);

        // Set colors
        _terminal.SetForegroundColor(character.ForegroundColor);
        _terminal.SetBackgroundColor(character.BackgroundColor);

        // Set modifiers
        foreach (var modifier in character.Modifiers)
        {
            _terminal.EnableSGR(modifier);
        }

        // Draw the character
        _terminal.PutString(character.CharacterString);

        // Reset modifiers (to avoid bleeding into next character)
        foreach (var modifier in character.Modifiers)
        {
            _terminal.DisableSGR(modifier);
        }
    }

    public override TerminalSize? DoResizeIfNecessary()
    {
        return ProcessResizeIfNecessary();
    }

    public override KeyStroke? PollInput()
    {
        return _terminal.PollInput();
    }

    public override KeyStroke ReadInput()
    {
        return _terminal.ReadInput();
    }

    public override KeyStroke? ReadInput(TimeSpan timeout)
    {
        return _terminal.ReadInput(timeout);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_isStarted)
            {
                try
                {
                    StopScreen();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
            _terminal?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Internal resize listener that handles terminal resize events
    /// </summary>
    private class TerminalScreenResizeListener : ITerminalResizeListener
    {
        private readonly TerminalScreen _screen;

        public TerminalScreenResizeListener(TerminalScreen screen)
        {
            _screen = screen;
        }

        public void OnResized(ITerminal terminal, TerminalSize newSize)
        {
            _screen.SetLatestResizeRequest(newSize);
            _screen._fullRedrawHint = true;
        }
    }
}