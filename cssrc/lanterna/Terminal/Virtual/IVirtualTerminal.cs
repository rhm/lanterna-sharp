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

namespace Lanterna.Terminal.Virtual;

/// <summary>
/// A virtual terminal is a kind of terminal emulator implemented inside of Lanterna that exposes the Terminal interface
/// and maintains its state completely internally. The IVirtualTerminal interface extends this interface and
/// allows you to query and modify its internals in a way you can not do with a regular terminal. The AWT and Swing
/// terminal implementations in Lanterna uses the DefaultVirtualTerminal class internally for keeping its state
/// and doing most of the terminal operations.
/// </summary>
public interface IVirtualTerminal : ITerminal
{
    /// <summary>
    /// Changes the "visible size" of the virtual terminal. This is the area at the bottom of the text buffer that is
    /// considered the workable area since the cursor is restricted to this space. If you call this method with a size
    /// that is different from the current size of the virtual terminal, the resize event will be fired on all listeners.
    /// </summary>
    /// <param name="newSize">New size of the virtual terminal</param>
    void SetTerminalSize(TerminalSize newSize);

    /// <summary>
    /// Adds a listener to receive notifications when certain events happens on the virtual terminal. Notice that this is
    /// not the same as the list of ITerminalResizeListener, but as the
    /// IVirtualTerminalListener also allows you to listen on size changes, it can be used for the same purpose.
    /// </summary>
    /// <param name="listener">Listener to receive events from this virtual terminal</param>
    void AddVirtualTerminalListener(IVirtualTerminalListener listener);

    /// <summary>
    /// Removes a listener from this virtual terminal so it will no longer receive events. Notice that this is not the
    /// same as the list of ITerminalResizeListener.
    /// </summary>
    /// <param name="listener">Listener to remove from this virtual terminal</param>
    void RemoveVirtualTerminalListener(IVirtualTerminalListener listener);

    /// <summary>
    /// Sets the number of rows to allow in the non-private buffer above the viewport. The total size of the text buffer
    /// will be backlogSize + terminalSize.Rows. If set to 0, there is no scrollback. Please note that
    /// private mode is unaffected by this and will always have no backlog.
    /// </summary>
    /// <param name="backlogSize">Number of rows of backlog</param>
    void SetBacklogSize(int backlogSize);

    /// <summary>
    /// Checks if the terminal cursor is visible or not
    /// </summary>
    /// <returns>true if the terminal cursor is visible, false otherwise</returns>
    bool IsCursorVisible { get; }

    /// <summary>
    /// Adds a KeyStroke to the input queue of this virtual terminal. This event will be read the next time either
    /// PollInput() or ReadInput() is called, assuming there are no other events before it in the queue.
    /// </summary>
    /// <param name="keyStroke">KeyStroke to add to the input queue of this virtual terminal</param>
    void AddInput(KeyStroke keyStroke);

    /// <summary>
    /// Returns the position of the terminal cursor where the row index is counted from the top of the text buffer,
    /// including all backlog. This means, if there is 500 lines of backlog but the cursor position is set to 0x0, this
    /// method will return 0x500. If you want to get the cursor's position in the viewport, please use
    /// GetCursorPosition() instead.
    /// </summary>
    /// <returns>Cursor position as an offset from the top-left position of the text buffer including any backlog</returns>
    TerminalPosition GetCursorBufferPosition();

    /// <summary>
    /// Returns a character from this virtual terminal, relative to the top-left position of the text buffer including
    /// any backlog. If you want to get a character from the bottom viewport, please use
    /// GetCharacter(TerminalPosition) instead.
    /// </summary>
    /// <param name="position">Position to get the character from</param>
    /// <returns>Text character at the specific position in the text buffer</returns>
    TextCharacter GetBufferCharacter(TerminalPosition position);

    /// <summary>
    /// Returns a character from this virtual terminal, relative to the top-left position of the text buffer including
    /// any backlog. If you want to get a character from the bottom viewport, please use
    /// GetCharacter(int, int) instead.
    /// </summary>
    /// <param name="column">Column to get the character from</param>
    /// <param name="row">Row, counting from the first line in the backlog, to get the character from</param>
    /// <returns>Text character at the specific position in the text buffer</returns>
    TextCharacter GetBufferCharacter(int column, int row);

    /// <summary>
    /// Returns a character from the viewport at the specified coordinates. This method cannot access the backlog, if you
    /// want to fetch a character potentially from the backlog, please use GetBufferCharacter(TerminalPosition)
    /// instead.
    /// </summary>
    /// <param name="position">Position of the character to return</param>
    /// <returns>Text character at the specific position in the viewport</returns>
    TextCharacter GetCharacter(TerminalPosition position);

    /// <summary>
    /// Returns a character from the viewport at the specified coordinates. This method cannot access the backlog, if you
    /// want to fetch a character potentially from the backlog, please use GetBufferCharacter(int,int)
    /// instead.
    /// </summary>
    /// <param name="column">Column in the viewport to get the character from</param>
    /// <param name="row">Row in the viewport to get the character form</param>
    /// <returns>Text character at the specific position in the viewport</returns>
    TextCharacter GetCharacter(int column, int row);

    /// <summary>
    /// Returns the number of lines in the entire text buffer, including any backlog
    /// </summary>
    /// <returns>Number of lines in the buffer</returns>
    int GetBufferLineCount();

    /// <summary>
    /// Iterates over a range of lines in the text buffer
    /// </summary>
    /// <param name="startRow">Index of the first row of the iteration, counting 0 as the first row in the backlog</param>
    /// <param name="endRow">Index of the last row of the iteration (inclusive), counting 0 as the first row in the backlog</param>
    /// <param name="bufferWalker">Callback to invoke on each row in the iteration</param>
    void ForEachLine(int startRow, int endRow, IBufferWalker bufferWalker);

    /// <summary>
    /// Returns a set of terminal positions that have been changed since the last time this method was called. 
    /// These positions represent cells that have been modified and may need to be redrawn.
    /// </summary>
    /// <returns>Set of positions that are marked as dirty</returns>
    SortedSet<TerminalPosition> GetDirtyCells();

    /// <summary>
    /// Returns a set of terminal positions that have been changed since the last time this method was called and then
    /// clears the dirty set. These positions represent cells that have been modified and may need to be redrawn.
    /// </summary>
    /// <returns>Set of positions that were marked as dirty</returns>
    SortedSet<TerminalPosition> GetAndResetDirtyCells();

    /// <summary>
    /// Returns true if the entire buffer is considered dirty and needs a complete refresh, then resets this flag to false.
    /// This typically happens after operations like clearing the screen or switching modes.
    /// </summary>
    /// <returns>true if the whole buffer was dirty, false otherwise</returns>
    bool IsWholeBufferDirtyThenReset();

    /// <summary>
    /// Writes a string to the terminal without tracking dirty cells. This is used by screen refresh operations
    /// where dirty cell tracking is handled at the screen level, not the terminal level.
    /// </summary>
    /// <param name="str">String to write to the terminal</param>
    void PutStringWithoutDirtyTracking(string str);

    /// <summary>
    /// Marks the start of a screen refresh operation. During refresh operations, the dirty cell overflow
    /// protection is disabled to prevent the whole buffer from being marked dirty during large refresh operations.
    /// </summary>
    void BeginRefreshOperation();

    /// <summary>
    /// Marks the end of a screen refresh operation. This re-enables the dirty cell overflow protection.
    /// </summary>
    void EndRefreshOperation();

    /// <summary>
    /// Interface used by IBufferWalker to represent a line in the text buffer when iterating over a range of
    /// lines
    /// </summary>
    public interface IBufferLine
    {
        /// <summary>
        /// Returns a text character from this line in the specific column
        /// </summary>
        /// <param name="column">Column to return the text character from</param>
        /// <returns>Text character in the column of this line</returns>
        TextCharacter GetCharacterAt(int column);
    }

    /// <summary>
    /// Callback interface that is used by ForEachLine(int, int, IBufferWalker) as a way to iterate over a range
    /// of lines in the text buffer
    /// </summary>
    public interface IBufferWalker
    {
        /// <summary>
        /// Invoked separately on each line inside the specified range when calling
        /// ForEachLine(int, int, IBufferWalker)
        /// </summary>
        /// <param name="rowNumber">The row number of this invocation, where 0 means the first line of the backlog</param>
        /// <param name="bufferLine">Object that represents the line and its content on this row</param>
        void OnLine(int rowNumber, IBufferLine bufferLine);
    }
}