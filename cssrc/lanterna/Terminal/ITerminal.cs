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
using Lanterna.Input;

namespace Lanterna.Terminal;

/// <summary>
/// This is the main terminal interface, at the lowest level supported by Lanterna. You can write your own
/// implementation of this if you want to target an exotic text terminal specification or another graphical environment
/// (like WPF), but you should probably extend AbstractTerminal instead of implementing this interface directly.
/// 
/// The normal way you interact in .NET with a terminal is through the standard output (Console.Out) and standard error
/// (Console.Error) and it's usually through printing text only. This interface abstracts a terminal at a more fundamental
/// level, expressing methods for not only printing text but also changing colors, moving the cursor new positions,
/// enable special modifiers and get notified when the terminal's size has changed.
/// 
/// If you want to write an application that has a very precise control of the terminal, this is the
/// interface you should be programming against.
/// </summary>
public interface ITerminal : IInputProvider, IDisposable
{
    /// <summary>
    /// Calling this method will, where supported, give your terminal a private area to use, separate from what was there
    /// before. Some terminal emulators will preserve the terminal history and restore it when you exit private mode.
    /// Some terminals will just clear the screen and put the cursor in the top-left corner. Typically, if your terminal
    /// supports scrolling, going into private mode will disable the scrolling and leave you with a fixed screen, which
    /// can be useful if you don't want to deal with what the terminal buffer will look like if the user scrolls up.
    /// </summary>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    /// <exception cref="InvalidOperationException">If you are already in private mode</exception>
    void EnterPrivateMode();

    /// <summary>
    /// If you have previously entered private mode, this method will exit this and, depending on implementation, maybe
    /// restore what the terminal looked like before private mode was entered. If the terminal doesn't support a
    /// secondary buffer for private mode, it will probably make a new line below the private mode and place the cursor
    /// there.
    /// </summary>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    /// <exception cref="InvalidOperationException">If you are not in private mode</exception>
    void ExitPrivateMode();

    /// <summary>
    /// Removes all the characters, colors and graphics from the screen and leaves you with a big empty space. Text
    /// cursor position is undefined after this call (depends on platform and terminal) so you should always call
    /// SetCursorPosition next. Some terminal implementations doesn't reset color and modifier state so it's also good
    /// practice to call ResetColorAndSGR() after this.
    /// </summary>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void ClearScreen();

    /// <summary>
    /// Moves the text cursor to a new location on the terminal. The top-left corner has coordinates 0 x 0 and the bottom-
    /// right corner has coordinates terminal_width-1 x terminal_height-1. You can retrieve the size of the terminal by
    /// calling GetTerminalSize().
    /// </summary>
    /// <param name="x">The 0-indexed column to place the cursor at</param>
    /// <param name="y">The 0-indexed row to place the cursor at</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void SetCursorPosition(int x, int y);

    /// <summary>
    /// Same as calling SetCursorPosition(position.Column, position.Row)
    /// </summary>
    /// <param name="position">Position to place the cursor at</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void SetCursorPosition(TerminalPosition position);

    /// <summary>
    /// Returns the position of the cursor, as reported by the terminal. The top-left corner has coordinates 0 x 0 and
    /// the bottom-right corner has coordinates terminal_width-1 x terminal_height-1.
    /// </summary>
    /// <returns>Position of the cursor</returns>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    TerminalPosition GetCursorPosition();

    /// <summary>
    /// Hides or shows the text cursor, but not all terminal (-emulators) supports this. The text cursor is normally a
    /// text block or an underscore, sometimes blinking, which shows the user where keyboard-entered text is supposed to
    /// show up.
    /// </summary>
    /// <param name="visible">Hides the text cursor if false and shows it if true</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void SetCursorVisible(bool visible);

    /// <summary>
    /// Prints one character to the terminal at the current cursor location. Please note that the cursor will then move
    /// one column to the right, so multiple calls to PutCharacter will print out a text string without the need
    /// to reposition the text cursor. If you reach the end of the line while putting characters using this method, you
    /// can expect the text cursor to move to the beginning of the next line.
    /// 
    /// You can output CJK (Chinese, Japanese, Korean) characters (as well as other regional scripts) but remember that
    /// the terminal that the user is using might not have the required font to render it. Also worth noticing is that
    /// CJK (and some others) characters tend to take up 2 columns per character, simply because they are a square in
    /// their construction as opposed to the somewhat rectangular shape we fit latin characters in.
    /// 
    /// If you try to print non-printable control characters, the terminal is likely to ignore them (all ITerminal
    /// implementations bundled with Lanterna will).
    /// </summary>
    /// <param name="c">Character to place on the terminal</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void PutCharacter(char c);

    /// <summary>
    /// Prints a string to the terminal at the current cursor location. Please note that the cursor will then move
    /// one column to the right, so multiple calls to PutString will print out a text string without the need
    /// to reposition the text cursor. If you reach the end of the line while putting characters using this method, you
    /// can expect the text cursor to move to the beginning of the next line.
    /// 
    /// You can output CJK (Chinese, Japanese, Korean) characters (as well as other regional scripts) but remember that
    /// the terminal that the user is using might not have the required font to render it. Also worth noticing is that
    /// CJK (and some others) characters tend to take up 2 columns per character.
    /// 
    /// If you try to print non-printable control characters, the terminal is likely to ignore them (all ITerminal
    /// implementations bundled with Lanterna will).
    /// 
    /// You can use this method to place emoji characters on the terminal, since they take up more than one char with
    /// .NET's built-in UTF16 encoding.
    /// </summary>
    /// <param name="str">String to place on the terminal</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void PutString(string str);

    /// <summary>
    /// Creates a new ITextGraphics object that uses this Terminal directly when outputting. Keep in mind that you are
    /// probably better off to switch to a Screen to make advanced text graphics more efficient. Also, this ITextGraphics
    /// implementation will not call Flush() after any operation, so you'll need to do that on your own.
    /// </summary>
    /// <returns>ITextGraphics implementation that draws directly using this Terminal interface</returns>
    /// <exception cref="IOException">If there was an I/O error when setting up the ITextGraphics object</exception>
    ITextGraphics NewTextGraphics();

    /// <summary>
    /// Activates an SGR (Selected Graphic Rendition) code. This code modifies a state inside the terminal
    /// that will apply to all characters written afterwards, such as bold, italic, blinking code and so on.
    /// </summary>
    /// <param name="sgr">SGR code to apply</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void EnableSGR(SGR sgr);

    /// <summary>
    /// Deactivates an SGR (Selected Graphic Rendition) code which has previously been activated through
    /// EnableSGR(..).
    /// </summary>
    /// <param name="sgr">SGR code to deactivate</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void DisableSGR(SGR sgr);

    /// <summary>
    /// Removes all currently active SGR codes and sets foreground and background colors back to default.
    /// </summary>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void ResetColorAndSGR();

    /// <summary>
    /// Changes the foreground color for all the following characters put to the terminal. The foreground color is what
    /// color to draw the text in, as opposed to the background color which is the color surrounding the characters.
    /// 
    /// This overload is using the ITextColor interface to define a color, which is a layer of abstraction above the three
    /// different color formats supported (ANSI, indexed and RGB). 
    /// </summary>
    /// <param name="color">Color to use for foreground</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void SetForegroundColor(ITextColor color);

    /// <summary>
    /// Changes the background color for all the following characters put to the terminal. The background color is the
    /// color surrounding the text being printed.
    /// 
    /// This overload is using the ITextColor interface to define a color, which is a layer of abstraction above the three
    /// different color formats supported (ANSI, indexed and RGB).
    /// </summary>
    /// <param name="color">Color to use for the background</param>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void SetBackgroundColor(ITextColor color);

    /// <summary>
    /// Adds a ITerminalResizeListener to be called when the terminal has changed size. There is no guarantee that
    /// this listener will really be invoked when the terminal has changed size, at all depends on the terminal emulator
    /// implementation. Normally on Unix systems the WINCH signal will be sent to the process and lanterna can intercept
    /// this.
    /// 
    /// There are no guarantees on what thread the call will be made on, so please be careful with what kind of operation
    /// you perform in this callback. You should probably not take too long to return.
    /// </summary>
    /// <param name="listener">Listener object to be called when the terminal has been changed</param>
    void AddResizeListener(ITerminalResizeListener listener);

    /// <summary>
    /// Removes a ITerminalResizeListener from the list of listeners to be notified when the terminal has changed
    /// size
    /// </summary>
    /// <param name="listener">Listener object to remove</param>
    void RemoveResizeListener(ITerminalResizeListener listener);

    /// <summary>
    /// Returns the size of the terminal, expressed as a TerminalSize object. Please bear in mind that depending
    /// on the ITerminal implementation, this may or may not be accurate. See the implementing classes for more
    /// information. Most commonly, calling GetTerminalSize() will involve some kind of hack to retrieve the size of the
    /// terminal, like moving the cursor to position 5000x5000 and then read back the location, unless the terminal
    /// implementation has a more smooth way of getting this data. Keep this in mind and see if you can avoid calling
    /// this method too often. There is a helper class, SimpleTerminalResizeListener, that you can use to cache the size
    /// and update it only when resize events are received (which depends on if a resize is detectable, which they are not
    /// on all platforms).
    /// </summary>
    /// <returns>Size of the terminal</returns>
    /// <exception cref="IOException">if there was an I/O error trying to retrieve the size of the terminal</exception>
    TerminalSize GetTerminalSize();

    /// <summary>
    /// Retrieves optional information from the terminal by printing the ENQ (\u0005) character. Terminals and terminal
    /// emulators may or may not respond to this command, sometimes it's configurable.
    /// </summary>
    /// <param name="timeout">How long to wait for the talk-back message, if there's nothing immediately available on the input
    /// stream, you should probably set this to a somewhat small value to prevent unnecessary blockage on the input stream
    /// but large enough to accommodate a round-trip to the user's terminal (~300 ms if you are connection across the globe).</param>
    /// <returns>Answer-back message from the terminal or empty if there was nothing</returns>
    /// <exception cref="IOException">If there was an I/O error while trying to read the enquiry reply</exception>
    byte[] EnquireTerminal(TimeSpan timeout);

    /// <summary>
    /// Prints 0x7 to the terminal, which will make the terminal (emulator) ring a bell (or more likely beep). Not all
    /// terminals implements this.
    /// </summary>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void Bell();

    /// <summary>
    /// Calls Flush() on the underlying Stream object, or whatever other implementation this
    /// terminal is built around. Some implementing classes of this interface doesn't do anything
    /// as it doesn't really apply to them.
    /// </summary>
    /// <exception cref="IOException">If there was an underlying I/O error</exception>
    void Flush();
}

/// <summary>
/// Interface for drawing text and simple graphics on a terminal or screen
/// </summary>
public interface ITextGraphics
{
    /// <summary>
    /// Current foreground color
    /// </summary>
    ITextColor ForegroundColor { get; set; }
    
    /// <summary>
    /// Current background color
    /// </summary>
    ITextColor BackgroundColor { get; set; }
    
    /// <summary>
    /// Sets a character at the specified position
    /// </summary>
    void SetCharacter(int column, int row, char character);
    
    /// <summary>
    /// Sets a character at the specified position
    /// </summary>
    void SetCharacter(TerminalPosition position, char character);
    
    /// <summary>
    /// Sets a styled character at the specified position
    /// </summary>
    void SetCharacter(int column, int row, TextCharacter character);
    
    /// <summary>
    /// Sets a styled character at the specified position
    /// </summary>
    void SetCharacter(TerminalPosition position, TextCharacter character);
    
    /// <summary>
    /// Puts a string at the specified position
    /// </summary>
    void PutString(int column, int row, string text);
    
    /// <summary>
    /// Puts a string at the specified position
    /// </summary>
    void PutString(TerminalPosition position, string text);
    
    /// <summary>
    /// Draws a line between two points
    /// </summary>
    void DrawLine(TerminalPosition fromPoint, TerminalPosition toPoint, char character);
    
    /// <summary>
    /// Draws a line between two points
    /// </summary>
    void DrawLine(int fromColumn, int fromRow, int toColumn, int toRow, char character);
    
    /// <summary>
    /// Draws a rectangle outline
    /// </summary>
    void DrawRectangle(TerminalPosition topLeft, TerminalSize size, char character);
    
    /// <summary>
    /// Draws a rectangle outline
    /// </summary>
    void DrawRectangle(int column, int row, int width, int height, char character);
    
    /// <summary>
    /// Fills a rectangle with a character
    /// </summary>
    void FillRectangle(TerminalPosition topLeft, TerminalSize size, char character);
    
    /// <summary>
    /// Fills a rectangle with a character
    /// </summary>
    void FillRectangle(int column, int row, int width, int height, char character);
}