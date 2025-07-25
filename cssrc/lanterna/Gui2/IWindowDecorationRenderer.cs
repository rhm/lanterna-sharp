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

namespace Lanterna.Gui2;

/// <summary>
/// Interface that defines a class that draws window decorations, i.e. a surrounding layer around the window that usually
/// looks like a border to make it easier for a user to visually separate the windows.
/// </summary>
public interface IWindowDecorationRenderer
{
    /// <summary>
    /// Draws the window decorations for a particular window and returns a new ITextGUIGraphics that is locked to the area
    /// inside of the window decorations where the content of the window should be drawn
    /// </summary>
    /// <param name="textGUI">Which ITextGUI is calling</param>
    /// <param name="graphics">Graphics to use for drawing</param>
    /// <param name="window">Window to draw</param>
    /// <returns>A new ITextGUIGraphics that is limited to the area inside the decorations just drawn</returns>
    ITextGUIGraphics Draw(IWindowBasedTextGUI textGUI, ITextGUIGraphics graphics, IWindow window);

    /// <summary>
    /// Retrieves the full size of the window, including all window decorations, given all components inside the window.
    /// </summary>
    /// <param name="window">Window to calculate size for</param>
    /// <param name="contentAreaSize">Size of the content area in the window</param>
    /// <returns>Full size of the window, including decorations</returns>
    TerminalSize GetDecoratedSize(IWindow window, TerminalSize contentAreaSize);

    /// <summary>
    /// Returns how much to step right and down from the top left position of the window decorations to the top left
    /// position of the actual window
    /// </summary>
    /// <param name="window">Window to get the offset for</param>
    /// <returns>Position of the top left corner of the window, relative to the top left corner of the window decoration</returns>
    TerminalPosition GetOffset(IWindow window);

    /// <summary>
    /// Gets the rectangle representing the title bar area of the window
    /// </summary>
    /// <param name="window">Window to get the title bar rectangle for</param>
    /// <returns>Rectangle representing the title bar area</returns>
    TerminalRectangle GetTitleBarRectangle(IWindow window)
    {
        return new TerminalRectangle(0, 0, window.DecoratedSize.Columns, GetOffset(window).Row);
    }
}