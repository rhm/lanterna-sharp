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
/// Window manager is a class that is plugged in to a IWindowBasedTextGUI to manage the position and placement
/// of windows. The window manager doesn't contain the list of windows so it normally does not need to maintain much
/// state but it is passed all required objects as the window model changes.
/// </summary>
public interface IWindowManager
{
    /// <summary>
    /// Will be polled by the the IWindowBasedTextGUI to see if the window manager believes an update is required.
    /// For example, it could be that there is no input, no events and none of the components are invalid, but the window
    /// manager decides for some other reason that the GUI needs to be updated, in that case you should return
    /// true here. Please note that returning false will not prevent updates from happening, it's just
    /// stating that the window manager isn't aware of some internal state change that would require an update.
    /// </summary>
    /// <returns>true if the window manager believes the GUI needs to be update, false otherwise</returns>
    bool IsInvalid { get; }

    /// <summary>
    /// Returns the IWindowDecorationRenderer for a particular window
    /// </summary>
    /// <param name="window">Window to get the decoration renderer for</param>
    /// <returns>IWindowDecorationRenderer for the window</returns>
    IWindowDecorationRenderer GetWindowDecorationRenderer(IWindow window);

    /// <summary>
    /// Called whenever a window is added to the IWindowBasedTextGUI. This gives the window manager an opportunity
    /// to setup internal state, if required, or decide on an initial position of the window
    /// </summary>
    /// <param name="textGUI">GUI that the window was added too</param>
    /// <param name="window">Window that was added</param>
    /// <param name="allWindows">All windows, including the new window, in the GUI</param>
    void OnAdded(IWindowBasedTextGUI textGUI, IWindow window, IReadOnlyList<IWindow> allWindows);

    /// <summary>
    /// Called whenever a window is removed from a IWindowBasedTextGUI. This gives the window manager an
    /// opportunity to clear internal state if needed.
    /// </summary>
    /// <param name="textGUI">GUI that the window was removed from</param>
    /// <param name="window">Window that was removed</param>
    /// <param name="allWindows">All windows, excluding the removed window, in the GUI</param>
    void OnRemoved(IWindowBasedTextGUI textGUI, IWindow window, IReadOnlyList<IWindow> allWindows);

    /// <summary>
    /// Called by the GUI system before iterating through all windows during the drawing process. The window manager
    /// should ensure the position and decorated size of all windows at this point by using
    /// window.Position and window.DecoratedSize. Be sure to inspect the window hints
    /// assigned to the window, in case you want to try to honour them. Use the
    /// GetWindowDecorationRenderer method to get the currently assigned window decoration rendering
    /// class which can tell you the decorated size of a window given it's content size.
    /// </summary>
    /// <param name="textGUI">Text GUI that is about to draw the windows</param>
    /// <param name="allWindows">All windows that are going to be drawn, in the order they will be drawn</param>
    /// <param name="screenSize">Size of the terminal that is available to draw on</param>
    void PrepareWindows(IWindowBasedTextGUI textGUI, IReadOnlyList<IWindow> allWindows, TerminalSize screenSize);
}