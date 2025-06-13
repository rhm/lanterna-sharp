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
/// Tab behavior enum for handling tab characters in screen output
/// </summary>
public enum TabBehavior
{
    /// <summary>
    /// Tabs are replaced with a single space
    /// </summary>
    ReplaceWithSpace,
    
    /// <summary>
    /// Tabs are aligned to column multiples of 4
    /// </summary>
    AlignToColumn4,
    
    /// <summary>
    /// Tabs are aligned to column multiples of 8
    /// </summary>
    AlignToColumn8
}

/// <summary>
/// Refresh type enum for screen refresh operations
/// </summary>
public enum RefreshType
{
    /// <summary>
    /// Only refresh changed areas (delta refresh)
    /// </summary>
    Delta,
    
    /// <summary>
    /// Refresh the entire screen
    /// </summary>
    Complete,
    
    /// <summary>
    /// Refresh automatically determining the best method
    /// </summary>
    Automatic
}

/// <summary>
/// Screen is a fundamental layer in Lanterna, presenting the terminal as a bitmap-like surface where you can perform
/// smaller in-memory operations to a back-buffer, effectively painting out the terminal as you'd like it, and then call
/// Refresh() to have the screen automatically apply the changes in the back-buffer to the real terminal. The 
/// screen tracks what's visible through a front-buffer, but this is completely managed internally and cannot be expected
/// to know what the terminal looks like if it's being modified externally.
/// </summary>
public interface IScreen : IInputProvider, IDisposable
{
    /// <summary>
    /// The default character Screen implementations should use as a filler if there are areas not set to any particular character.
    /// </summary>
    static readonly TextCharacter DefaultCharacter = TextCharacter.FromCharacter(' ', 
        new AnsiTextColor(AnsiColor.Default), 
        new AnsiTextColor(AnsiColor.Default));

    /// <summary>
    /// Before you can use a Screen, you need to start it. By starting the screen, Lanterna will make sure the terminal
    /// is in private mode (Screen only supports private mode), clears it (so that it can set the front and back buffers
    /// to a known value) and places the cursor in the top left corner. After calling StartScreen(), you can begin using
    /// the other methods on this interface. When you want to exit from the screen and return to what you had before,
    /// you can call StopScreen().
    /// </summary>
    void StartScreen();

    /// <summary>
    /// Calling this method will make the underlying terminal leave private mode, effectively going back to whatever
    /// state the terminal was in before calling StartScreen(). Once a screen has been stopped, you can start it
    /// again with StartScreen() which will restore the screen's content to the terminal.
    /// </summary>
    void StopScreen();

    /// <summary>
    /// Erases all the characters on the screen, effectively giving you a blank area. The default background color will
    /// be used. This is effectively the same as calling FillRectangle with the entire screen size.
    /// Please note that calling this method will only affect the back buffer, you need to call Refresh() to make the 
    /// change visible.
    /// </summary>
    void Clear();

    /// <summary>
    /// A screen implementation typically keeps a location on the screen where the cursor will be placed after drawing
    /// and refreshing the buffers, this method returns that location. If it returns null, it means that the terminal 
    /// will attempt to hide the cursor (if supported by the terminal).
    /// </summary>
    /// <returns>Position where the cursor will be located after the screen has been refreshed or null if the cursor is not visible</returns>
    TerminalPosition? GetCursorPosition();

    /// <summary>
    /// A screen implementation typically keeps a location on the screen where the cursor will be placed after drawing
    /// and refreshing the buffers, this method controls that location. If you pass null, it means that the terminal 
    /// will attempt to hide the cursor (if supported by the terminal).
    /// </summary>
    /// <param name="position">TerminalPosition of the new position where the cursor should be placed after Refresh(), or if null, hides the cursor</param>
    void SetCursorPosition(TerminalPosition? position);

    /// <summary>
    /// Gets the behavior for what to do about tab characters. If a tab character is written to the Screen, it would
    /// cause issues because we don't know how the terminal emulator would render it and we wouldn't know what state the
    /// front-buffer is in. Because of this, we convert tabs to a determined number of spaces depending on different
    /// rules that are available.
    /// </summary>
    /// <returns>Tab behavior that is used currently</returns>
    TabBehavior GetTabBehavior();

    /// <summary>
    /// Sets the behavior for what to do about tab characters. If a tab character is written to the Screen, it would
    /// cause issues because we don't know how the terminal emulator would render it and we wouldn't know what state the
    /// front-buffer is in. Because of this, we convert tabs to a determined number of spaces depending on different
    /// rules that are available.
    /// </summary>
    /// <param name="tabBehavior">Tab behavior to use when converting a \t character to spaces</param>
    void SetTabBehavior(TabBehavior tabBehavior);

    /// <summary>
    /// Returns the size of the screen. This call is not blocking but should return the size of the screen as it is
    /// represented by the buffer at the time this method is called.
    /// </summary>
    /// <returns>Size of the screen, in columns and rows</returns>
    TerminalSize GetTerminalSize();

    /// <summary>
    /// Sets a character in the back-buffer to a specified value with specified colors and modifiers.
    /// </summary>
    /// <param name="column">Column of the character to modify (x coordinate)</param>
    /// <param name="row">Row of the character to modify (y coordinate)</param>
    /// <param name="screenCharacter">New data to put at the specified position</param>
    void SetCharacter(int column, int row, TextCharacter screenCharacter);

    /// <summary>
    /// Sets a character in the back-buffer to a specified value with specified colors and modifiers.
    /// </summary>
    /// <param name="position">Which position in the terminal to modify</param>
    /// <param name="screenCharacter">New data to put at the specified position</param>
    void SetCharacter(TerminalPosition position, TextCharacter screenCharacter);

    /// <summary>
    /// Creates a new ITextGraphics object that is targeting this Screen for writing to. Any operations done on this
    /// ITextGraphics will be affecting this screen. Remember to call Refresh() on the screen to see your changes.
    /// </summary>
    /// <returns>New ITextGraphics object targeting this Screen</returns>
    ITextGraphics NewTextGraphics();

    /// <summary>
    /// Reads a character and its associated meta-data from the front-buffer and returns it encapsulated as a
    /// TextCharacter.
    /// </summary>
    /// <param name="column">Which column to get the character from</param>
    /// <param name="row">Which row to get the character from</param>
    /// <returns>A TextCharacter representation of the character in the front-buffer at the specified location</returns>
    TextCharacter GetFrontCharacter(int column, int row);

    /// <summary>
    /// Reads a character and its associated meta-data from the front-buffer and returns it encapsulated as a 
    /// TextCharacter.
    /// </summary>
    /// <param name="position">What position to read the character from</param>
    /// <returns>A TextCharacter representation of the character in the front-buffer at the specified location</returns>
    TextCharacter GetFrontCharacter(TerminalPosition position);

    /// <summary>
    /// Reads a character and its associated meta-data from the back-buffer and returns it encapsulated as a
    /// TextCharacter.
    /// </summary>
    /// <param name="column">Which column to get the character from</param>
    /// <param name="row">Which row to get the character from</param>
    /// <returns>A TextCharacter representation of the character in the back-buffer at the specified location</returns>
    TextCharacter GetBackCharacter(int column, int row);

    /// <summary>
    /// Reads a character and its associated meta-data from the back-buffer and returns it encapsulated as a 
    /// TextCharacter.
    /// </summary>
    /// <param name="position">What position to read the character from</param>
    /// <returns>A TextCharacter representation of the character in the back-buffer at the specified location</returns>
    TextCharacter GetBackCharacter(TerminalPosition position);

    /// <summary>
    /// This method will take the content from the back-buffer and move it into the front-buffer, making the changes
    /// visible to the terminal in the process. The graphics workflow with Screen would involve drawing text and text-like
    /// graphics on the back buffer and then finally calling Refresh() to make it visible to the user.
    /// </summary>
    void Refresh();

    /// <summary>
    /// This method will take the content from the back-buffer and move it into the front-buffer, making the changes
    /// visible to the terminal in the process. The graphics workflow with Screen would involve drawing text and text-like
    /// graphics on the back buffer and then finally calling Refresh() to make it visible to the user.
    /// 
    /// Using this method call instead of Refresh() gives you a little bit more control over how the screen will
    /// be refreshed.
    /// </summary>
    /// <param name="refreshType">What type of refresh to do</param>
    void Refresh(RefreshType refreshType);

    /// <summary>
    /// One problem working with Screens is that whenever the terminal is resized, the front and back buffers needs to be
    /// adjusted accordingly and the program should have a chance to figure out what to do with this extra space (or less
    /// space). The solution is to call, at the start of your rendering code, this method, which will check if the 
    /// terminal has been resized and in that case update the internals of the Screen. After this call finishes, the 
    /// screen's internal buffers will match the most recent size report from the underlying terminal.
    /// </summary>
    /// <returns>If the terminal has been resized since this method was last called, it will return the new size of the
    /// terminal. If not, it will return null.</returns>
    TerminalSize? DoResizeIfNecessary();

    /// <summary>
    /// Scroll a range of lines of this Screen according to given distance.
    /// </summary>
    /// <param name="firstLine">First line to scroll (0-indexed)</param>
    /// <param name="lastLine">Last line to scroll (0-indexed)</param>
    /// <param name="distance">Distance to scroll (positive scrolls down, negative scrolls up)</param>
    void ScrollLines(int firstLine, int lastLine, int distance);
}